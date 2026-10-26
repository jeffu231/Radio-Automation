# Plan: Seed Unit Tests with xUnit.net

**Status:** Proposed
**Date:** 2026-10-26
**Scope:** Add the solution's first automated test project, a set of seed tests over the parts of the code that can be tested in isolation, and a CI step that runs them.

---

## 1. Goals and non-goals

### Goals
- Add one xUnit.net test project to `Radio Automation.sln` that builds with the existing `dotnet restore --runtime win-x64` / `dotnet build` flow.
- Cover a **core of deterministic, side-effect-free (or temp-dir-only) code** across the three code projects: `Radio Automation`, `NAudioWrapper`, and `Weather`.
- Run the tests in CI on every push to `master`, before versioning/publishing.
- Write down the defects found while planning as **skipped tests**, so the follow-up fixes have tests ready to turn on.

### Non-goals (explicitly out of scope for the seed)
- **No production code changes.** Tests describe current behavior. If a test can't be written without a production change, leave it out and note it in §9.
- No ViewModel tests (`MainWindowViewModel`, `EventScheduleViewModel`, etc.). They depend on the Catel IoC container, UI services, and message bus. Do these in a later phase.
- No WASAPI/device tests (`AudioPlayback`), no `Settings` construction (its constructor calls `AudioPlayback.HasDefaultDevice()`, which uses COM/MMDevice and can behave differently on CI runners without audio hardware).
- No MQTT tests (`MqttEventListener`, `MqttPublisherService`). They need a broker, or a refactor to pull out the topic/payload matching logic.
- No `EventScheduler` / `EventTask` tests (they wrap the vendored `JobToolkit.Core.dll`, which is slated for replacement).
- No WPF view/behavior tests (`Views/*`, `Behaviors/*`, `WPFDarkTheme`), and nothing that needs an STA thread or a `Dispatcher`.
- No network calls. The Weather tests only use JSON deserialization and the code path that returns before any HTTP request.

---

## 2. Framework and package choices

| Item | Choice | Notes |
|---|---|---|
| Test framework | **xUnit.net v3** (`xunit.v3`) | Current major version. v3 test projects are executables (`OutputType=Exe`). |
| VSTest adapter | `xunit.runner.visualstudio` (3.x) | Lets `dotnet test` and Rider/VS Test Explorer discover the tests. |
| Test SDK | `Microsoft.NET.Test.Sdk` | Required for `dotnet test` (VSTest mode). |
| Assertions | xUnit built-in `Assert` | Don't add FluentAssertions/Shouldly. Keep dependencies minimal. |
| Mocking | **None** for the seed | Every seed target can be tested with real objects or small hand-written fakes. |

**Package versions:** add the packages with `dotnet add package` so the **latest stable** versions get pinned. Don't copy versions from this document.

```powershell
dotnet add "Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj" package xunit.v3
dotnet add "Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj" package xunit.runner.visualstudio
dotnet add "Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj" package Microsoft.NET.Test.Sdk
```

`global.json` pins SDK `10.0.103` with no `test.runner` entry, so `dotnet test` uses VSTest mode. Don't add Microsoft.Testing.Platform configuration.

---

## 3. Project setup

### 3.1 Location and naming

```
Tests/
  RadioAutomation.Tests/
    RadioAutomation.Tests.csproj
    TestUtilities/
      TempDirectory.cs
      TestJsonOptions.cs
      TestAudio.cs
      FakeSampleProvider.cs
      NonParallelCollection.cs
    Converters/
      EnumBooleanConverterTests.cs
      NullToVisibilityConverterTests.cs
      NotNullToVisibilityConverterTests.cs
      PercentageConverterTests.cs
    Extensions/
      CollectionExtensionsTests.cs
      EnumerationExtensionTests.cs
      FileExtensionsTests.cs
    Models/
      EventTests.cs
      TrackTests.cs
      PlaylistTests.cs
      JsonSerializationTests.cs
    Events/
      PendingEventTests.cs
      EventBusTests.cs
    Services/
      PersistenceServiceTests.cs
      AudioTrackParserServiceTests.cs
    Utils/
      ThreadSafeRandomTests.cs
    Audio/
      SampleAggregatorTests.cs
      ConcatenatingAudioFileReaderTests.cs
    WeatherClient/
      WunderGroundTests.cs
      WunderGroundObservationTests.cs
```

> **Important (namespaces):** Don't name test folders/namespaces `NAudioWrapper` or `Weather`. A namespace like `RadioAutomation.Tests.Weather` shadows the production `Weather` namespace inside the test assembly and causes ambiguous-reference build errors. Use `Audio` and `WeatherClient` as shown above.

Namespace convention: `RadioAutomation.Tests.<Folder>`, e.g. `RadioAutomation.Tests.Converters`.

### 3.2 `RadioAutomation.Tests.csproj`

Create the file by hand (don't use `dotnet new`, so it matches the template below), then run the `dotnet add package` commands from §2 to add the `PackageReference` items.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <OutputType>Exe</OutputType>
    <UseWPF>true</UseWPF>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <RootNamespace>RadioAutomation.Tests</RootNamespace>
    <AssemblyName>RadioAutomation.Tests</AssemblyName>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\Radio Automation\Radio Automation.csproj" />
    <ProjectReference Include="..\..\NAudioWrapper\NAudioWrapper.csproj" />
    <ProjectReference Include="..\..\Weather\Weather.csproj" />
  </ItemGroup>

  <!-- PackageReference items added via `dotnet add package` (see plan §2) -->
</Project>
```

Why these settings:
- `net10.0-windows` + `win-x64`: matches the main app (`Radio Automation.csproj` sets `RuntimeIdentifier=win-x64`). Using the same RID avoids restore/asset mismatches with the CI command `dotnet restore --runtime win-x64`.
- `UseWPF=true`: the converters return `System.Windows.Visibility` / `DependencyProperty.UnsetValue`, and `EnumerationExtension` derives from `MarkupExtension`. The test assembly needs the WindowsDesktop framework reference to compile against these types.
- `OutputType=Exe`: required by xUnit v3.
- `Nullable`/`ImplicitUsings` disabled: matches the main app project. Write explicit `using` directives the same way the production code does.
- NAudio and TagLibSharp come in transitively through the project references (default `PackageReference` flow). Don't add them directly unless the build says they're missing.

**If the build fails with NETSDK1150/NETSDK1151** (executable-reference self-contained mismatch), add `<ValidateExecutableReferencesMatchSelfContained>false</ValidateExecutableReferencesMatchSelfContained>` to the test project's `PropertyGroup`. Don't change the main project.

### 3.3 Add to the solution

```powershell
dotnet sln "Radio Automation.sln" add "Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj" --solution-folder Tests
```

Check that the solution still builds: `dotnet restore --runtime win-x64` then `dotnet build "Radio Automation.sln"`.

### 3.4 Code style for test files
- Tabs for indentation, block-scoped `namespace X { ... }`, braces on new lines. This matches the production code.
- Test class name: `<ClassUnderTest>Tests`, `public sealed class`.
- Test method name: `MethodOrMember_Scenario_ExpectedResult`, e.g. `Convert_NullValue_ReturnsVisible`.
- Use `[Fact]` for single cases and `[Theory]` + `[InlineData]` for tables of inputs.
- Use Arrange / Act / Assert blank-line separation. Don't add `// Arrange` comments.
- Async tests return `Task` (xUnit v3 also supports `ValueTask`). Never use `.Result`/`.Wait()`.
- Pass `TestContext.Current.CancellationToken` where an API takes a token (xUnit v3 analyzers suggest it). None of the seed targets accept one, so this rarely applies.

---

## 4. Shared test utilities (`TestUtilities/`)

### 4.1 `TempDirectory`
An `IDisposable` that creates a unique directory under `Path.GetTempPath()` (e.g. `Path.Combine(Path.GetTempPath(), "RadioAutomation.Tests", Guid.NewGuid().ToString("N"))`), exposes `string Path` and `string File(string name)` (returns the combined path), and recursively deletes the directory in `Dispose()` (wrap the delete in try/catch and ignore `IOException`, since file handles may still be closing).

Every test that touches the filesystem creates its own `TempDirectory` in a `using` statement. Tests never write anywhere else.

### 4.2 `TestJsonOptions`
Mirrors the options built in `App.OnStartup` (`Radio Automation/App.xaml.cs`):

```csharp
public static JsonSerializerOptions Create() => new JsonSerializerOptions
{
	WriteIndented = true,
	DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
	TypeInfoResolver = new DefaultJsonTypeInfoResolver
	{
		Modifiers = { App.IgnoreXmlIgnoredProperties }
	},
	Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
```

- Reference the real `Radio_Automation.App.IgnoreXmlIgnoredProperties` static property. **Don't** copy the lambda. Reading a static property doesn't create a WPF `Application`.
- Add an XML doc comment saying this must stay in sync with `App.OnStartup`. (A later refactor could move the options into a shared factory. Not part of this plan.)

### 4.3 `TestAudio`
A helper that writes small WAV files with NAudio so tests don't need binary fixtures checked in:

```csharp
public static string CreateSineWav(string path, int sampleRate = 44100, int channels = 1, int durationMs = 250)
```

Implementation: `new SignalGenerator(sampleRate, channels) { Type = SignalGeneratorType.Sin, Frequency = 440, Gain = 0.2 }`, `.Take(TimeSpan.FromMilliseconds(durationMs))`, then `WaveFileWriter.CreateWaveFile16(path, provider)`. Return `path`.

### 4.4 `FakeSampleProvider`
An `ISampleProvider` for `SampleAggregator` tests:
- Constructor `(float[] samples, int channels = 1, int sampleRate = 44100)`.
- `WaveFormat => WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channels)`.
- `Read(float[] buffer, int offset, int count)` copies the next `min(count, remaining)` samples into `buffer` starting at `offset`, moves an internal position forward, and returns the number copied (0 when exhausted).

### 4.5 `NonParallelCollection`
```csharp
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class NonParallelCollection
{
	public const string Name = "NonParallel";
}
```
Use `[Collection(NonParallelCollection.Name)]` on any test class that changes static state (`EventBus.EventTriggered`).

---

## 5. Seed test catalog

Each subsection lists the class under test, its source file, and the exact test cases. Expected values come from reading the current source. Where behavior is a known defect, the test is listed under §5.12 as a skipped test, not asserted here.

### 5.1 Converters (`Radio Automation/Converters/`)

**`NullToVisibilityConverterTests`**
| Test | Input | Expected |
|---|---|---|
| `Convert_NullValue_ReturnsVisible` | `null` | `Visibility.Visible` |
| `Convert_NonNullValue_ReturnsCollapsed` | `new object()` | `Visibility.Collapsed` |
| `ConvertBack_Always_ThrowsNotImplemented` | any | `NotImplementedException` |

**`NotNullToVisibilityConverterTests`**: the inverse of the above (`null` gives `Collapsed`, non-null gives `Visible`, and `ConvertBack` throws).

**`EnumBooleanConverterTests`**: use the production `Radio_Automation.Models.Trigger` enum.
| Test | Call | Expected |
|---|---|---|
| `Convert_ValueMatchesParameter_ReturnsTrue` | `Convert(Trigger.Mqtt, typeof(bool), "Mqtt", ...)` | `true` |
| `Convert_ValueDiffersFromParameter_ReturnsFalse` | `Convert(Trigger.Cron, typeof(bool), "Mqtt", ...)` | `false` |
| `Convert_NullParameter_ReturnsUnsetValue` | parameter `null` | `DependencyProperty.UnsetValue` |
| `Convert_UndefinedEnumValue_ReturnsUnsetValue` | value `(Trigger)99`, parameter `"Cron"` | `DependencyProperty.UnsetValue` |
| `Convert_ParameterNotAnEnumName_ThrowsArgumentException` | `Trigger.Cron`, `"Bogus"` | `ArgumentException` (`Enum.Parse` behavior) |
| `ConvertBack_ValidParameter_ReturnsParsedEnum` | `ConvertBack(true, typeof(Trigger), "Mqtt", ...)` | `Trigger.Mqtt` |
| `ConvertBack_NullParameter_ReturnsUnsetValue` | parameter `null` | `DependencyProperty.UnsetValue` |

**`PercentageConverterTests`**: always pass **numeric** (non-string) values so the result doesn't depend on the current culture (`System.Convert.ToDouble(object)` uses the current culture for strings).
| Test | value, parameter | Expected (`int`) |
|---|---|---|
| `Convert_MultipliesAndTruncates` (Theory) | `200.0, 0.5` | `100` |
| | `10.0, 0.999` | `9` (truncation, not rounding) |
| | `0.0, 0.75` | `0` |
| | `100, 1.5` (int value) | `150` |
| `ConvertBack_Always_ThrowsNotImplemented` | any | `NotImplementedException` |

Assert the boxed result type is `int`: `Assert.IsType<int>(result)`.

### 5.2 `EnumerationExtension` (`Radio Automation/Extensions/EnumerationExtension.cs`)

Declare a private test enum inside the test class:
```csharp
private enum Sample
{
	[Description("First Item")] First,
	Second
}
```
| Test | Expected |
|---|---|
| `Ctor_NullType_ThrowsArgumentNullException` | `new EnumerationExtension(null)` throws `ArgumentNullException` |
| `Ctor_NonEnumType_ThrowsArgumentException` | `new EnumerationExtension(typeof(string))` throws `ArgumentException` |
| `Ctor_NullableEnumType_IsAccepted` | `new EnumerationExtension(typeof(Sample?))` doesn't throw. `EnumType == typeof(Sample?)` |
| `ProvideValue_ReturnsOneMemberPerEnumValue` | `ProvideValue(null)` cast to `EnumerationExtension.EnumerationMember[]` has length 2. `Value`s are `Sample.First`, `Sample.Second` in order |
| `ProvideValue_UsesDescriptionAttributeWhenPresent` | member[0].Description == `"First Item"` |
| `ProvideValue_FallsBackToNameWithoutDescription` | member[1].Description == `"Second"` |

### 5.3 `CollectionExtensions` (`Radio Automation/Extensions/CollectionExtensions.cs`)

Shuffle uses the unseeded `ThreadSafeRandom`, so assert **properties of the result**, never a specific order.

| Test | Expected |
|---|---|
| `Shuffle_ObservableCollection_PreservesElements` | 20 ints 0..19. After `Shuffle()`, `OrderBy(x=>x)` equals the original sequence and count stays 20 |
| `Shuffle_FastObservableCollection_PreservesElements` | Same as above with Catel `FastObservableCollection<int>` |
| `Shuffle_EmptyAndSingle_DoNotThrow` (Theory over count 0 and 1, both collection types) | No exception. Contents unchanged |
| `Shuffle_LargeCollection_EventuallyChangesOrder` | 20 elements. Loop up to 10 times calling `Shuffle()`. Assert at least one iteration's order differs from the original (the chance of a false failure is about 10⁻¹⁸⁰) |
| `SortTrackArtist_OrdersByArtist` | Tracks with artists `"Charlie"`, `"Alpha"`, `"Bravo"` end up ordered `Alpha, Bravo, Charlie`. The **same instances** are moved (check with `Assert.Same`) |
| `SortTrackArtist_AlreadySorted_IsUnchanged` | Input already in order. Sequence is unchanged |
| `SortTrackArtist_NullArtist_SortsFirst` | `null` artist ends up at index 0 (default `OrderBy` comparer) |
| `SortTrackName_OrdersByName` | Same pattern using `Name` |

Create tracks with the 7-arg constructor: `new Track(name, artist, "Album", 2020, TimeSpan.FromMinutes(3), new List<Tag>(), new List<Genre>())`. Use plain ASCII strings so culture-sensitive ordering doesn't matter.

### 5.4 `FileExtensions` (`Radio Automation/Extensions/FileExtensions.cs`)

Write raw bytes with `File.WriteAllBytes` into a `TempDirectory`. **Every file must be at least 4 bytes** (see defect D3).

| Test | File bytes | Expected `GetEncoding` |
|---|---|---|
| `GetEncoding_Utf8Bom_ReturnsUtf8` | `EF BB BF 41 42` | `Encoding.UTF8` |
| `GetEncoding_Utf16LeBom_ReturnsUnicode` | `FF FE 41 00` | `Encoding.Unicode` |
| `GetEncoding_Utf16BeBom_ReturnsBigEndianUnicode` | `FE FF 00 41` | `Encoding.BigEndianUnicode` |
| `GetEncoding_NoBom_ReturnsDefault` | `41 42 43 44` (`"ABCD"`) | `Encoding.Default` |

| Test | Expected |
|---|---|
| `ReadAllLinesAsync_ReturnsAllLines` | File written with `File.WriteAllText(path, "one\r\ntwo\nthree")` gives `["one","two","three"]` |
| `ReadAllLinesAsync_Utf16File_DecodesCorrectly` | `File.WriteAllText(path, "héllo\nwörld", Encoding.Unicode)` gives `["héllo","wörld"]` (shows the BOM-detected encoding is applied) |
| `ReadAllLinesAsync_MissingFile_Throws` | `await Assert.ThrowsAsync<FileNotFoundException>(...)` |

### 5.5 Models (`Radio Automation/Models/`)

**`EventTests`**
| Test | Expected |
|---|---|
| `Ctor_Default_SetsDefaults` | `Id != Guid.Empty`. `Trigger == Trigger.Cron`. `MqttExpression` not null. `Enabled == false`. `Demand == Demand.Immediate`. `EventType == EventType.Play` |
| `Ctor_Default_EndDateIsOneYearAfterStart` | `EndDateTime - StartDateTime` is within ±1 second of `StartDateTime.AddYears(1) - StartDateTime` (the two `DateTime.Now` calls are separate, so allow tolerance) |
| `Ctor_Default_GeneratesUniqueIds` | Two new events have different `Id`s |
| `Ctor_WithEventType_SetsEventTypeAndDefaults` | `new Event(EventType.Time)` gives `EventType == Time`, `Id != Guid.Empty`, `Trigger == Cron` |

**`TrackTests`**
| Test | Expected |
|---|---|
| `Ctor_Full_AssignsAllProperties` | All 7 args round-trip to properties. `Id != Guid.Empty` |
| `Ctor_NamePath_SetsStreamDefaults` | `new Track("My Stream", "http://x")` gives `Artist == "Stream"`, `Album == ""`, `Duration == TimeSpan.Zero`, `Year == (uint)DateTime.Now.Year`, `Tags`/`Genres` empty and not null, `Path == "http://x"` |
| `FormattedName_CombinesArtistAndName` | `"Artist - Name"` |
| `IsValid_DefaultsToTrue` | `new Track().IsValid == true` |

**`PlaylistTests`**
| Test | Expected |
|---|---|
| `Ctor_Default_IsEmptyWithZeroTotalTime` | `Tracks.Count == 0`. `TotalTime == TimeSpan.Zero`. `Id != Guid.Empty` |
| `Ctor_WithTracks_CalculatesTotalTime` | Durations 1:00 and 2:30 give `TotalTime == 3:30` |
| `AddingTrack_UpdatesTotalTime` | Start with 1:00. `Tracks.Add(track of 0:45)` gives `TotalTime == 1:45` |
| `RemovingTrack_UpdatesTotalTime` | Start with 1:00 + 2:00. Remove the 2:00 track. `TotalTime == 1:00` |

> Risk: Catel's `FastObservableCollection` may try to dispatch `CollectionChanged` through a dispatcher service. If `AddingTrack_UpdatesTotalTime`/`RemovingTrack_UpdatesTotalTime` fail because the event isn't raised synchronously in the test host, mark them `Skip = "FastObservableCollection dispatching in test host - investigate"` and record it in the PR description. Don't change production code.

**`JsonSerializationTests`**: checks the app's custom JSON contract (`App.IgnoreXmlIgnoredProperties`), using `TestJsonOptions.Create()`.
| Test | Expected |
|---|---|
| `Serialize_Track_OmitsXmlIgnoredCatelProperties` | Find every public instance property on `typeof(Track)` (including inherited) where `IsDefined(typeof(XmlIgnoreAttribute), inherit: false)` is true. **Assert there is at least one** (so the test isn't empty). Serialize a populated `Track` and parse it with `JsonDocument`. Assert none of those property names appear as top-level keys |
| `Serialize_Track_OmitsJsonIgnoredProperties` | No `FormattedName` or `IsValid` keys |
| `Serialize_Track_IncludesDataProperties` | Keys `Name`, `Artist`, `Album`, `Duration`, `Path`, `Id` are present |
| `RoundTrip_Event_PreservesValues` | Event with `Name="Top of hour"`, `Trigger=Mqtt`, `EventType=Time`, `Demand=Delayed`, `Enabled=true`, `Expires=true`, `CronExpression="0 * * * *"`, `MqttExpression={Topic="radio/cmd", Message="time"}`, `File=@"C:\a.mp3"`. After serialize and deserialize, every one of those values plus `Id` is equal |
| `RoundTrip_EventWithDefaultEnums_PreservesValues` | Event with `Trigger=Cron`, `Demand=Immediate`, `EventType=Play` (all value `0`, so `WhenWritingDefault` omits them). Deserializes back to the same values |

### 5.6 Events (`Radio Automation/Events/`)

**`PendingEventTests`**
| Test | Expected |
|---|---|
| `Equals_SameEventAndTime_IsTrue` | Same `Event` instance and same `DateTime` give `Equals` true, `Equals(object)` true, and equal `GetHashCode()` |
| `Equals_DifferentTime_IsFalse` | Same event, times 1 minute apart |
| `Equals_DifferentEvent_IsFalse` | Two separate `new Event()` (different Ids), same time |
| `Equals_Null_IsFalse` | `Equals((PendingEvent)null)` and `Equals((object)null)` are false |
| `Equals_OtherType_IsFalse` | `Equals("string")` is false |
| `Equals_SameReference_IsTrue` | `pending.Equals(pending)` |

**`EventBusTests`**: `[Collection(NonParallelCollection.Name)]`. Implement `IDisposable`: save `EventBus.EventTriggered` in the constructor and restore it in `Dispose()`.
| Test | Expected |
|---|---|
| `OnEventTriggered_NoSubscriber_DoesNotThrow` | Set `EventTriggered = null`, then call. No exception |
| `OnEventTriggered_WithSubscriber_InvokesWithSameEvent` | Subscriber captures its argument. `Assert.Same(e, captured)` |

### 5.7 `ThreadSafeRandomTests` (`Radio Automation/Utils/ThreadSafeRandom.cs`)
| Test | Expected |
|---|---|
| `Instance_IsNotNull` | |
| `Instance_SameThread_ReturnsSameInstance` | Two reads on the same thread are `Assert.Same` |
| `Instance_DifferentThreads_ReturnDifferentInstances` | Read on the test thread, and read inside `new Thread(...)` (start + join). `Assert.NotSame` |

### 5.8 Services (`Radio Automation/Services/`)

**`PersistenceServiceTests`**
Construct with `new PersistenceService(TestJsonOptions.Create())`.

> Side effect: the constructor creates a settings directory under `%AppData%\<entry assembly Company>\<entry assembly Product>`. Under xUnit v3 the entry assembly is `RadioAutomation.Tests`, so this is a test-only folder. That's acceptable. **Don't call `LoadSettingsAsync`/`SaveSettingsAsync`** in the seed (they construct `Settings`, which probes audio devices; see §1).

| Test | Expected |
|---|---|
| `Ctor_NullOptions_Throws` | `ArgumentNullException` (Catel `Argument.IsNotNull`) |
| `SaveThenLoadPlaylist_RoundTripsTracks` | Playlist named `"Seed"` with 2 tracks (distinct Name/Artist/Album/Year/Duration/Path). `SavePlaylistAsync` returns `true`. `LoadPlaylistAsync` gives the same `Name`, 2 tracks in order with equal Name/Artist/Album/Year/Duration/Path, and `TotalTime` equal to the sum of durations |
| `LoadPlaylist_MissingFile_ReturnsEmptyPlaylist` | Not null. `Tracks` empty |
| `LoadPlaylist_CorruptJson_ReturnsEmptyPlaylist` | File contents `"{ not json"` give an empty playlist (exception is swallowed and logged) |
| `SaveThenLoadEventSchedule_RoundTripsEvents` | Schedule `Name="Weekday"` with one Cron event (`CronExpression="0 * * * *"`, `EventType=Time`) and one MQTT event (`Topic="radio/cmd"`, `Message="stop"`, `EventType=Stop`, `Demand=Delayed`, `Enabled=true`). After the round trip: Name, count, and per-event `Id`, `Trigger`, `EventType`, `Demand`, `Enabled`, `CronExpression`, `MqttExpression.Topic/Message` all match |
| `LoadEventSchedule_MissingFile_ReturnsEmptySchedule` | Not null. `Events` empty |
| `ImportZaraPlaylist_ImportsOnlyExistingWellFormedEntries` | In a `TempDirectory`, create `a.wav` with `TestAudio.CreateSineWav`. Write a Zara file (UTF-8, no BOM) with lines: header `"1"`, `"0\t<full path to a.wav>"`, `"0\t<full path to missing.wav>"`, `"malformed line without tab"`. Result has exactly 1 track and its `Path` equals `a.wav`'s path |
| `ImportZaraPlaylist_HeaderOnly_ReturnsEmptyPlaylist` | File with just `"header"` (≥4 bytes) gives 0 tracks |

**`AudioTrackParserServiceTests`**
| Test | Expected |
|---|---|
| `CreateTrackFromFile_MissingFile_ReturnsNull` | Returns `null` (exception is caught and logged) |
| `CreateTrackFromFile_ValidWav_ReturnsTrack` | 500 ms WAV gives a non-null track, `Path` equals the file path, `Duration` within ±50 ms of 500 ms, `Genres` not null and empty |
| `RetrieveAudioTrackFromFile_DelegatesToCreate` | Instance method returns a track with the same `Path` |
| `RetrieveAudioTracksInPath_NonRecursive_ReturnsOnlyAudioFiles` | Dir with `one.wav`, `two.wav`, `notes.txt`, and a subfolder holding `three.wav`. `recurse: false` gives 2 tracks, and their paths are the two top-level WAVs (compare as a set) |

### 5.9 `SampleAggregator` (`NAudioWrapper/SampleAggregator.cs`)
| Test | Expected |
|---|---|
| `Ctor_FftLengthNotPowerOfTwo_Throws` | `new SampleAggregator(fake, 1000)` throws `ArgumentException` |
| `Ctor_DefaultFftLength_Succeeds` | No exception. `WaveFormat` same as the source's |
| `Read_PassesSamplesThroughUnchanged` | Source `[0.1f, 0.2f, 0.3f]`. Reading 3 returns 3, and the buffer equals the source |
| `Read_NotificationCountReached_RaisesMaximumCalculated` | Mono `[0.1f, -0.5f, 0.9f, 0.2f]`, `NotificationCount=4`. Raised exactly once with `MinSample == -0.5f`, `MaxSample == 0.9f` |
| `Read_NotificationCountZero_NeverRaises` | Same samples, `NotificationCount=0`. Never raised |
| `Read_Stereo_OnlySamplesFirstChannel` | Stereo `[0.1f, 0.99f, -0.2f, -0.99f]`, `NotificationCount=2`. Raised once with `Max == 0.1f`, `Min == -0.2f` |
| `Read_AllPositiveSamples_MinIsZero` | Mono `[0.3f, 0.6f]`, `NotificationCount=2`. `MinSample == 0f` (min/max start at 0. This documents current behavior) |
| `Reset_ClearsAccumulatedState` | `NotificationCount=4`. Read 2 samples `[0.9f, -0.9f]`, call `Reset()`, then read 4 samples `[0.1f, -0.1f, 0.2f, -0.2f]`. Raised once with `Max == 0.2f`, `Min == -0.2f` |

Use a separate `FakeSampleProvider` instance per read sequence, or one provider that holds all samples and is read in chunks.

### 5.10 `ConcatenatingAudioFileReader` (`NAudioWrapper/ConcatenatingAudioFileReader.cs`)
Generate WAVs with `TestAudio`. Dispose every `AudioFileReader` (`using` or a `try/finally` over a list).

| Test | Expected |
|---|---|
| `Ctor_Null_ThrowsArgumentNullException` | |
| `Ctor_Empty_ThrowsArgumentException` | `Array.Empty<AudioFileReader>()` |
| `Ctor_MismatchedSampleRate_Throws` | 44100 Hz mono + 22050 Hz mono throws `ArgumentException` |
| `Ctor_MismatchedChannels_Throws` | 44100 mono + 44100 stereo throws `ArgumentException` |
| `Ctor_MatchingInputs_LengthIsSumOfInputs` | Two 44100 mono WAVs (250 ms and 500 ms). `Length == r1.Length + r2.Length`. `WaveFormat` equals `r1.WaveFormat` |
| `Read_ReadsAllInputsSequentially` | Read floats in a loop with a 4096 buffer until 0. Total floats read `== (r1.Length + r2.Length) / 4` (readers output IEEE float, 4 bytes per sample) |

Open fresh readers for the `Read` test. Don't reuse readers whose `Length` was read in another test.

### 5.11 Weather (`Weather/`)

**`WunderGroundObservationTests`**: deserialize with `new JsonSerializerOptions(JsonSerializerDefaults.Web)`. That's what `HttpClient.GetFromJsonAsync` uses in production.

Sample payload (keep it as a `const string` in the test class):
```json
{"observations":[{"stationID":"KTEST123","obsTimeUtc":"2026-10-26T14:00:00Z","neighborhood":"Test Hill","country":"US","humidity":65,"winddir":270,"imperial":{"temp":72.5,"dewpt":60.1,"heatIndex":73.0,"windChill":72.0,"windSpeed":5.0,"windGust":8.0,"pressure":30.01,"precipRate":0.0,"precipTotal":0.12}}]}
```
| Test | Expected |
|---|---|
| `Deserialize_SamplePayload_MapsTopLevelFields` | One observation. `StationId=="KTEST123"`, `Name=="Test Hill"`, `Country=="US"`, `Humidity==65`, `WindDirection==270`, `ObservationTime == new DateTime(2026,10,26,14,0,0, DateTimeKind.Utc)` (compare `.ToUniversalTime()`) |
| `Deserialize_SamplePayload_MapsImperialFields` | `Temp==72.5`, `DewPoint==60.1`, `HeatIndex==73`, `WindSpeed==5`, `WindGust==8`, `Pressure==30.01`, `PrecipTotal==0.12` (use `Assert.Equal(expected, actual, precision: 3)`) |
| `Ctor_Default_ImperialValuesAreZero` | `new WunderGroundObservation().Temp == 0` (no `NullReferenceException`, since the constructor creates `Imperial`) |
| `Deserialize_EmptyObservations_YieldsEmptyList` | `{"observations":[]}` gives `Observations` empty |

**`WunderGroundTests`**: only paths that return **before** any HTTP call.
| Test (Theory) | stationId, apiKey | Expected |
|---|---|---|
| `GetObservationAsync_MissingCredentials_ReturnsEmptyObservation` | `(null,"key")`, `("", "key")`, `("STATION", null)`, `("STATION", "")` | Returns a non-null `WunderGroundObservation` with `Temp == 0` and `StationId == null` |

### 5.12 Skipped tests for known defects

Add these as `[Fact(Skip = "Known defect Dn: <short description> - see docs/plans/unit-tests-seed-plan.md §9")]`. Each one asserts the **correct** behavior, so un-skipping it once the defect is fixed checks the fix. Put each in the test class for the matching component.

| Id | Test class | Test name | Asserts (correct behavior) |
|---|---|---|---|
| D1 | `TrackTests` | `Ctor_NamePath_AssignsId` | `new Track("s","p").Id != Guid.Empty` |
| D2 | `TrackTests` | `GetStringTags_EmptyList_ReturnsEmptyString` | `""` (currently throws `ArgumentOutOfRangeException`) |
| D3 | `FileExtensionsTests` | `GetEncoding_FileShorterThanFourBytes_ReturnsDefault` | 2-byte file gives `Encoding.Default` (currently throws `EndOfStreamException`) |
| D4 | `AudioTrackParserServiceTests` | `RetrieveAudioTracksInPath_UnreadableAudioFile_IsSkipped` | A `.mp3` containing random bytes is **not** returned as a `null` entry |
| D5 | `AudioTrackParserServiceTests` | `RetrieveAudioTracksInPath_UppercaseExtension_IsIncluded` | `LOUD.WAV` is returned |
| D6 | `AudioTrackParserServiceTests` | `RetrieveAudioTracksInPath_Recursive_ReturnsEachFileOnce` | root + sub + sub/sub WAVs give exactly 3 tracks with distinct paths |
| D7 | `EnumerationExtensionTests` | `ProvideValue_NullableEnum_ReturnsUnderlyingMembers` | `new EnumerationExtension(typeof(Sample?)).ProvideValue(null)` returns 2 members |

---

## 6. CI integration (`.github/workflows/build.yml`)

Add a test step **right after** "Restore dependencies" and **before** "Setup git"/"Versioning", so a failing test stops versioning and release:

```yaml
      - name: Test
        run: dotnet test '.\Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj' --no-restore --logger "trx;LogFileName=test-results.trx" --results-directory TestResults
```

Notes:
- Target the **test project**, not the solution. Running `dotnet test` on the `.sln` also builds `Radio-Automation-Installer.wixproj`, which expects publish output that doesn't exist yet at this point in the pipeline.
- `--no-restore` is safe because the previous step already restored with `--runtime win-x64`, and the test project uses the same RID.
- Optional (nice to have, not required): upload `TestResults/*.trx` with `actions/upload-artifact` using `if: always()`.
- Don't change `paths-ignore`. Test changes should trigger CI.
- The later "Installer" step (`dotnet build -c Release /p:Version=...`) builds the whole solution, which now includes the test project. That's fine (it only compiles). Check that it still succeeds locally with `dotnet build -c Release /p:Version=9.9.9`.

---

## 7. Documentation updates

- `CLAUDE.md`: replace "There are no automated tests in this project." with a short **Testing** section:
  - Framework: xUnit.net v3, project `Tests/RadioAutomation.Tests`.
  - Command: `dotnet test 'Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj'`.
  - Conventions: temp-dir-only filesystem access through `TempDirectory`. No network, audio devices, MQTT broker, or STA/Dispatcher. Use `NonParallelCollection` for static state.
  - Add the test project to the Solution Structure table.
- Don't touch `README.md` or `CHANGELOG.md` (Versionize generates the changelog).

---

## 8. Implementation order and verification

Do these in order. Build and run tests after each step.

1. **Scaffold** (§3): create the csproj, add packages, add to the solution, add one trivial test (e.g. `NullToVisibilityConverterTests`). Run:
   ```powershell
   dotnet restore --runtime win-x64
   dotnet build "Radio Automation.sln"
   dotnet test "Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj"
   ```
   Fix any reference/RID issues here before writing more tests.
2. **Utilities** (§4).
3. **Pure-logic tests**: §5.1, §5.2, §5.5 (Event/Track), §5.6, §5.7.
4. **Collection + JSON tests**: §5.3, §5.5 (Playlist, JsonSerialization).
5. **Filesystem tests**: §5.4, §5.8.
6. **Audio tests**: §5.9, §5.10.
7. **Weather tests**: §5.11.
8. **Skipped defect tests**: §5.12.
9. **CI + docs**: §6, §7. Run the full local sequence again, including `dotnet build -c Release /p:Version=9.9.9`.
10. Run the test suite **3 times in a row** to check for flakiness (parallel execution, temp-dir collisions, static state).

### Acceptance criteria
- `dotnet test` on the test project passes locally with **0 failures**. Only the §5.12 tests (and any Playlist tests skipped under the §5.5 risk note) are skipped.
- About 110 test cases (counting each `InlineData` row, including the 7 skipped ones) across the 20 test classes listed in §3.1.
- No test touches the network, an audio device, the real user settings file, or any path outside `Path.GetTempPath()` (other than the unavoidable `PersistenceService` constructor directory noted in §5.8).
- No production source files changed. The only non-test file changes are `Radio Automation.sln`, `.github/workflows/build.yml`, and `CLAUDE.md`.
- `dotnet build "Radio Automation.sln"` and `dotnet build -c Release /p:Version=9.9.9` both still succeed.
- Commits follow the repo's Husky/Conventional Commits rules, e.g.:
  - `test(Tests): Add xUnit test project and seed unit tests`
  - `ci(build): Run unit tests before versioning`
  - `docs(CLAUDE): Document test project and conventions`

---

## 9. Known defects found while planning (for follow-up, not fixed here)

| Id | Location | Defect |
|---|---|---|
| D1 | `Models/Track.cs` `Track(string name, string path)` | Doesn't chain `: this()`, so stream tracks get `Id == Guid.Empty`. |
| D2 | `Models/Track.cs` `GetStringTags` / `GetStringGenres` | `sb.Remove(sb.Length - 2, 2)` throws on an empty list. It also appends `tag.ToString()` (the Catel `ModelBase` string) instead of `tag.Name`. |
| D3 | `Extensions/FileExtensions.cs` `GetEncoding` | `ReadExactly(bom, 0, 4)` throws `EndOfStreamException` for files under 4 bytes. The UTF-32 check matches the **big-endian** BOM (`00 00 FE FF`) but returns little-endian `Encoding.UTF32`. A UTF-32 LE BOM (`FF FE 00 00`) is detected as UTF-16 LE. |
| D4 | `Services/AudioTrackParserService.cs` `RetrieveAudioTracksFromFolder` | `CreateTrackFromFile` returns `null` instead of throwing, so the surrounding try/catch never runs and `null` entries get added to the list. |
| D5 | same | The extension filter uses case-sensitive `EndsWith`, so `.MP3`/`.WAV` files are ignored. |
| D6 | same, `RecurseFoldersForTracks` | Adds each subfolder's tracks twice, then `AddRange`s the shared list into itself, so the result has many duplicates. |
| D7 | `Extensions/EnumerationExtension.cs` `ProvideValue` | Accepts `Nullable<TEnum>` in the setter but calls `Enum.GetValues(EnumType)` on the nullable type, which throws. |
| D8 | `NAudioWrapper/ConcatenatingAudioFileReader.cs` | `Position` is an auto-property that isn't connected to the underlying readers, so seeking does nothing. Not covered by a seed test. Note it for the audio phase. |

---

## 10. Suggested next phases (after the seed)

1. Pull the MQTT topic/payload matching out of `MqttEventListener` into a pure function (`IEnumerable<Event> Match(topic, payload)`), then test it.
2. Put an interface over `AudioPlayback` so `MainWindowViewModel` playback/queue logic (Immediate vs Delayed demand) can be tested with fakes.
3. Move the JSON options into a shared factory used by both `App.OnStartup` and the tests.
4. Fix D1–D8 and un-skip the matching tests.
5. Add coverage reporting (`coverlet.collector` + `--collect:"XPlat Code Coverage"`) once the suite is stable.
