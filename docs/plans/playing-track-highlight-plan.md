# Plan: Keep the playing-track highlight when the playlist is scrolled

## Problem

The track that is playing is shown in red and bold in the playlist. If the list is long enough to scroll, and you scroll that track out of view and then back, it loses the red highlight. The highlight comes back only when the next track starts and is visible.

## Root cause

The "is playing" state is stored in a per-row view model that does not last, and that view model only learns the state from a one-time message.

- `Views/MainWindow.xaml` (`PlaylistView`) is a `ListView` bound to `Playlist.Tracks`. Its `ItemTemplate` hosts `views:TrackView`. `ListView` uses a `VirtualizingStackPanel` by default, so rows that scroll out of view have their containers discarded or recycled.
- `TrackView` is a Catel `UserControl`. Catel builds a new `TrackViewModel` each time the view loads with a `Track` as its `DataContext`, and closes it when the view unloads.
- `TrackViewModel.IsPlaying` defaults to `false`. It is set only in `TrackPlaying(...)` when a `TrackPlayingMessage` arrives. That message is sent once per state change, from `MainWindowViewModel.StartPlay()` (`true`) and `MainWindowViewModel.PlaybackEnded()` (`false`).
- `TrackView.xaml` has a `CurrentTrackStyle` style with `DataTrigger`s on `IsPlaying` that set the red foreground and bold weight.

Scrolling the playing row out of view closes its `TrackViewModel`. Scrolling back creates a new one with `IsPlaying = false`. No message is sent again, so the row is drawn normally. The next `StartPlay()` sends a new message, and that is why the next track gets the highlight if it is visible.

`ScrollPlayingIntoViewBehavior` only scrolls the list. It plays no part in this bug.

## Fix: store the playing state on the `Track` model

The playing state has to live on something that lasts longer than the row's view model. `Track` instances are the list's items and last as long as the playlist, so they fit. `Track` already has a runtime-only flag of the same kind: `IsValid` is marked `[JsonIgnore]`.

### 1. `Models/Track.cs`: add a runtime-only `IsPlaying` property

Add a Catel property next to `IsValid`, using the same pattern:

```csharp
#region IsPlaying property

/// <summary>
/// Gets or sets whether this track is the one playing now. Runtime state only; not persisted.
/// </summary>
[JsonIgnore]
public bool IsPlaying
{
    get { return GetValue<bool>(IsPlayingProperty); }
    set { SetValue(IsPlayingProperty, value); }
}

/// <summary>
/// IsPlaying property data.
/// </summary>
public static readonly IPropertyData IsPlayingProperty = RegisterProperty<bool>(nameof(IsPlaying));

#endregion
```

`[JsonIgnore]` keeps the property out of the saved playlist JSON.

### 2. `ViewModels/TrackViewModel.cs`: map `IsPlaying` to the model

- Add `[ViewModelToModel(nameof(Track))]` to the existing `IsPlaying` property. Catel then reads the value from `Track.IsPlaying` when the view model is created, and keeps the two in sync while it is alive. A new row view model therefore starts with the correct value.
- Remove the `TrackPlayingMessage.Register` and `Unregister` calls and the `TrackPlaying` handler. The model change notifications replace them. You can then remove the `InitializeAsync` and `OnClosingAsync` overrides if nothing else uses them.

### 3. `ViewModels/MainWindowViewModel.cs`: set the flag on the model

Add a small helper and call it from the two places that send `TrackPlayingMessage` today:

```csharp
private void SetTrackPlaying(Track track, bool isPlaying)
{
    if (track != null)
    {
        track.IsPlaying = isPlaying;
    }
}
```

- `StartPlay()`: replace `TrackPlayingMessage.SendWith(new TrackInfoData(CurrentTrack, true))` with `SetTrackPlaying(CurrentTrack, true)`.
- `PlaybackEnded()`: replace `TrackPlayingMessage.SendWith(new TrackInfoData(CurrentTrack, false))` with `SetTrackPlaying(CurrentTrack, false)`.

This keeps the current timing exactly. The stream-reconnect paths return early from `PlaybackEnded()`, so a stream that is reconnecting stays highlighted, as it does now.

Both calls run on the UI thread, the same thread that sends the message today, so the change adds no threading concerns.

### 4. Remove the unused message types

After steps 2 and 3, nothing uses `TrackPlayingMessage` or `TrackInfoData` (`Messaging/TrackPlayingMessage.cs` and `Messaging/TrackInfoData.cs`). Use a symbol search to confirm there are no other references, then delete both files.

### 5. `Views/TrackView.xaml`: no change needed

The `DataTrigger`s already bind to `IsPlaying` on the view model, which now gets its value from the model.

## Risks and checks

- **Dirty tracking:** Catel's `SetValue` marks a `ModelBase` as dirty (`IsDirty`). Check whether `Playlist` or `Track.IsDirty` drives an "unsaved changes" prompt or an auto-save. If it does, changing `IsPlaying` must not trigger it. Options: exclude the property from dirty tracking (override `ShouldPropertyChangeUpdateIsDirty` in `Track` to return `false` for `IsPlayingProperty`), or check that `IsValid` already has the same issue and is handled the same way.
- **Stale flag when the playlist changes:** When a playlist is replaced, loaded, or cleared while a track is playing, the old `Track` keeps `IsPlaying = true`. This does no harm because those instances are no longer shown. If the same instance is reused (for example, a track removed and added again), make sure `PlaybackEnded()` or `StopPlaying()` still clears the flag. Check this by hand (see the manual test list below).
- **Tracks that are not in the playlist:** Event-driven single tracks and bumpers may set `CurrentTrack` to a `Track` that is not in `Playlist.Tracks`. Setting the flag on them is harmless.
- **Same instance listed twice:** If one `Track` instance appears twice in `Tracks`, both rows are highlighted. That is what happens today, because the message handler compares by reference.

## Tests (`Tests/RadioAutomation.Tests`)

1. **Serialization:** Save and load a `Playlist` that contains a track with `IsPlaying = true`, using `IPersistenceService` with a `TempDirectory`. Check that the saved JSON has no `IsPlaying` key and that the loaded track has `IsPlaying == false`.
2. **Row view model starts with the model's value (the regression test):** Set `track.IsPlaying = true`, then create a new `TrackViewModel(track)` and check that `vm.IsPlaying` is `true`. Then set `track.IsPlaying = false` and check that the view model follows. This is the scroll-out/scroll-back case without the UI. If Catel's `ViewModelBase` needs a dispatcher or service locator setup that the test project cannot provide, list this as a manual test and do not add workarounds.
3. If the dirty-tracking change is made, add a test that changing `IsPlaying` leaves `IsDirty` unchanged.

Follow the project rule for known defects: write test 2 first as `[Fact(Skip = "Known defect: playing highlight lost when row is re-virtualized")]`, then remove the skip in the commit that fixes it.

## Manual test

1. Load a playlist large enough to need a scroll bar, and start playback.
2. Scroll the playing track out of view, then scroll back. It should still be red and bold.
3. Repeat while scrolling fast with the scroll bar thumb, which recycles containers, and with the mouse wheel.
4. Let the track finish. The old row should go back to normal and the new track should be highlighted, whether or not it is visible when it starts.
5. Stop playback. The highlight should clear.
6. Play an Immediate event that interrupts the track, then let the playlist resume. The highlight should follow the track that is actually playing.
7. Play an internet stream, force a reconnect, and check that the highlight stays during the retry.
8. Save the playlist and check that the JSON file has no `IsPlaying` field.

## Commits

- `test(playlist): Add skipped test for lost playing highlight on scroll`
- `fix(playlist): Keep playing-track highlight when list is scrolled`, which also removes the skip
- Commit this plan as `chore(plans): Add playing-track highlight fix plan`
