# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

Radio Automation is a Windows WPF desktop application (net10.0-windows, win-x64) that functions as an off-hours audio player. It supports playlist management, cron-based event scheduling, MQTT-triggered events, and weather-based announcements (time/temperature bumpers in Zara format).

## Build Commands

```bash
# Restore dependencies (required before first build)
dotnet restore --runtime win-x64

# Build the solution
dotnet build "Radio Automation.sln"

# Publish the main app (uses a publish profile)
dotnet publish 'Radio Automation\Radio Automation.csproj' -p:PublishProfile=BuildProfile.pubxml /p:EnableWindowsTargeting=true

# Build the MSI installer (WiX project)
dotnet build -c Release /p:Version=<version>
```

There are no automated tests in this project.

## Commit Message Convention

Commits are linted via Husky. Messages must follow Conventional Commits:

```
<type>(<scope>): <subject>
```

Valid types: `feat`, `fix`, `docs`, `chore`, `style`, `refactor`, `perf`, `test`, `wip`, `ci`, `build`, `log`, `revert`

Subject must be 4+ characters and the line must be under 90 characters total. No trailing period or whitespace.

## CI / Versioning

The CI workflow (`.github/workflows/build.yml`) runs on push to `master`. It uses [Versionize](https://github.com/versionize/versionize) to auto-bump the version from conventional commit messages, generates a CHANGELOG, publishes the app, builds the MSI, creates a GitHub release, and submits to Winget (`JTDev.RadioAutomation`). Releases only happen when Versionize determines one is required — commits that don't trigger a version bump are no-ops in CI.

## Architecture Overview

[Catel MVVM](https://github.com/Catel/Catel) for the MVVM framework and WPF for the UI.
[Naudio](https://github.com/naudio/naudio) is used for audio playback. WASAPI is used for exclusive mode to provide near realtime playback with no latency.
[TagLibSharp](https://github.com/mono/taglib-sharp) is used for ID3/metadata parsing.

The eventbus is an internal message queue used to trigger events based on cron timings or MQTT messages. The user defines the events in the Events panel of the UI.
Events that have a demand of Immediate interrupt the current track and play the next one. 
Events that have a demand of Delayed are queued and played after the current track in the order they were defined.
The playlist is stored in json format to the location of the users choice. The location is saved in the user preferences to load at startup.
The weather is fetched from the Wunderground API and displayed in the UI. It can be used for time/temperature bumpers.
Event triggers are stored in a .evs file in json format in a location of the users choice.
JobToolKit.Core is used for cron-based event scheduling. It needs to be replaced with an alternative at some point. It was modified from an open source project that is no longer maintained.

### Solution Structure

| Project | Purpose |
|---|---|
| `Radio Automation` | Main WPF app — UI, ViewModels, Services, Models |
| `NAudioWrapper` | Audio playback abstraction over NAudio |
| `Weather` | Wunderground API client for temperature/humidity |
| `WPFDarkTheme` | Custom WPF dark theme/styling |
| `Resources` | Shared resources |
| `Radio-Automation-Installer` | WiX MSI installer project |

### MVVM Framework: Catel

The app uses **Catel.MVVM** throughout. Key patterns to know:

- All model properties use `GetValue<T>(Property)` / `SetValue(Property, value)` with `RegisterProperty<T>()` — never standard C# auto-properties on `ModelBase`/`ViewModelBase` subclasses.
- `[Model]` attribute on a ViewModel property wraps a model and enables `[ViewModelToModel("ModelName")]` attribute on sibling properties for automatic two-way sync.
- Commands are `Command` or `TaskCommand` (async) instances, lazily initialized with `??=`.
- Services (file dialogs, busy indicator, persistence) are resolved via `this.GetDependencyResolver().Resolve<IService>()` inside ViewModels.
- Inter-ViewModel messaging uses Catel's message bus: `TrackPlayingMessage.Register(this, handler)` / `Unregister` / `SendWith`.

### Audio Playback (`NAudioWrapper`)

`AudioPlayback` is the central playback engine:
- Uses **WASAPI Exclusive mode** (`WasapiOut` with `AudioClientShareMode.Exclusive`).
- Single-file playback goes through `MediaFoundationReader` (supports most Windows media formats).
- Multi-file (time/temp bumpers) uses `ConcatenatingAudioFileReader` which chains multiple `AudioFileReader` instances. All inputs must share the same sample rate and channel count.
- Volume is controlled via `VolumeSampleProvider` in the audio chain. Level metering is via `MeteringSampleProvider` → `SampleAggregator`.
- Implements `IMMNotificationClient` for audio device change notifications.

### Event System

Events have two trigger types (enum `Trigger`): `Cron` and `Mqtt`.

**Cron events** — `EventScheduler` wraps `JobToolkit.Core` (a local library at `Radio Automation\Library\JobToolkit.Core.dll`). It schedules `EventTask` instances using NCrontab.Advanced cron expressions. On load, all existing jobs are dequeued and rebuilt from the current `EventSchedule`.

**MQTT events** — `MqttEventListener` connects to a broker (from `Settings`) and subscribes to topics defined on each `Event.MqttExpression`. On message receipt, it matches topic + payload to trigger the appropriate event.

**MQTT song publishing** — `MqttPublisherService` (implements `IMqttPublisherService`, registered in `App.xaml.cs`, injected into `MainWindowViewModel`) maintains its own outbound connection to the same broker and publishes current song info as a retained JSON payload (`{"title","artist","album"}`) to a user-configured topic each time a track changes. It is intentionally separate from `MqttEventListener` so subscribe and publish concerns don't share state. Both are connected at startup and when the event schedule is reloaded.

Both trigger paths converge at `EventBus.OnEventTriggered(Event e)` — a static `Action<Event>` delegate that the main ViewModel subscribes to.

Events have a `Demand` property: `Immediate` (interrupts current playback) or `Delayed` (queued to play between tracks).

Event types (`EventType`): `Play`, `Stop`, `Pause`, `Temperature`, `Time`, `Humidity`, `Playlist`, `SingleTrack`.

### Data Persistence

- **Playlist / app state**: JSON-serialized to a file via `IPersistenceService`.
- **Event schedules**: Saved/loaded as `.evs` files via `IPersistenceService`. `EventSchedule` is a `ModelBase` with an `ObservableCollection<Event>`.
- **User preferences / Settings**: JSON-serialized via `IPersistenceService` to the user's AppData folder.

### Track Parsing

`AudioTrackParserService` (implements `IAudioTrackParserService`) reads ID3/metadata from audio files using **TagLibSharp**. It supports `.mp3`, `.wav`, `.flac`, `.ogg`, `.wma`. Playlist sources include folder scanning (recursive), M3U files (via OneWay.M3U), Zara playlists, and internet streams.