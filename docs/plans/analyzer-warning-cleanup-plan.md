# Analyzer Warning Cleanup Plan

Clears the build warnings produced by the stricter `.editorconfig` / `Directory.Build.props`
(`AnalysisMode=Recommended`, `EnforceCodeStyleInBuild=true`), then promotes every rule marked
`# Make error` once it is clean. Written to be executed **incrementally**: each step is one
commit (or one test-commit + one fix-commit pair), and any step can be done in a later session.

**Baseline (2026-10-26, `dotnet build "Radio Automation.sln" --no-incremental`):**
217 raw warnings / 126 unique, 0 errors. Raw is inflated by the `*_wpftmp` double-compile.
Scope is **warnings only**: `suggestion`-level rules are out of scope.

## How to resume
1. Find the first unchecked step in the [Step list](#step-list).
2. Re-run the baseline command and confirm the unique count matches the **Expected after** column
   of the last checked step (re-baseline here if it does not).
3. Do the step; tick the box; record the new count in the Progress log at the bottom.

Baseline command (unique warnings, from repo root):

```bash
dotnet build "Radio Automation.sln" --no-incremental -nologo -v:q -p:EnableWindowsTargeting=true 2>&1 \
  | grep -E "warning [A-Z]+[0-9]+" | sed -E 's/ \[[^]]*\]$//' | sort -u
```

## Test-first gate (applies to every step rated Questionable)

A change is **Questionable** if there is *any* way it could alter runtime behavior, even slightly
(evaluation count/order, culture, exception type/message, aliasing of shared instances, lifetime,
event semantics, public API shape). Compiler-proven changes (see risk table) are exempt.

For each Questionable step, in this order:

1. **Inventory**: find what already covers the code (Gortex `change(tests)` / `relations(usages)`
   on the symbol). Record the existing tests in the step notes.
2. **Gap check**: if existing tests do not assert the *specific behavior at risk* (listed per step
   below), write characterization tests that do.
3. **Green before**: run the new/existing tests against the **unchanged** code. They must pass.
   If a test fails, the code has a real defect: do **not** fix it in this series. Record it as
   `[Fact(Skip = "Known defect: ...")]` asserting the correct behavior (per CLAUDE.md) and pin the
   *current* behavior in a separate non-skipped test only if the change would alter it.
4. **Commit the tests alone**: `test(<scope>): Pin <behavior> before analyzer cleanup`.
5. **Apply the change** (separate commit): `refactor|fix|perf(<scope>): ...`.
6. **Green after**: the same tests pass **unmodified**. If a test had to change to pass, the change
   is a behavior change: stop, and either choose the behavior-preserving fix form or suppress the
   rule with a justification (Phase 4 style). Do not edit a pinning test to make a cleanup pass.
7. Full suite + warning count check, then tick the step.

**Untestable seams.** The test project forbids network, audio devices, MQTT, and STA/Dispatcher.
When the risky code lives somewhere those are required (`MainWindowViewModel`, `DataWindow`,
`AudioPlayback`, WPF behaviors/`TextBoxExtender`), use this ladder and stop at the first that works:

- (a) Choose the **behavior-preserving form** of the fix (e.g. `StringComparison.CurrentCulture`
  instead of `Ordinal`, passing the culture the old overload used implicitly), so no test is needed
  because behavior is provably identical. Say why in the commit body.
- (b) **Extract** the risky logic into a small pure `internal static` helper in its own preceding
  `refactor` commit (no behavior change, no analyzer fix), add tests for it, then apply the fix to
  the helper. Needs `InternalsVisibleTo` for the test project; check whether it exists first.
- (c) **Suppress** the rule at that site with a justification comment and note it in the Progress log.
- If none is acceptable, **stop and ask**. Do not silently accept an unverified behavior change.

Tests touching static or culture state use `[Collection(NonParallelCollection.Name)]` and restore
`CultureInfo.CurrentCulture`/`CurrentUICulture` in `finally`/`IDisposable`.

## Risk triage

`None` = compiler or analyzer proves equivalence, no new tests. `Low` = equivalent in practice, one
targeted check. `Questionable` = test-first gate applies.

| Rule | Hits | Risk | Why | Existing coverage (verify in step) | Tests required before change |
|---|---|---|---|---|---|
| IDE0074 `??=` | 34 | **Questionable** (properties), None (locals/fields) | `a = a ?? b` always calls the setter; `a ??= b` skips it when `a` is non-null. On Catel `SetValue` or CLR-property setters with side effects (PropertyChanged, persistence, audio) that differs | `PlaylistTests`, `EventTests` (Catel models, testable) | Classify each hit by target kind. Local/field: none. Model property: test that a PropertyChanged/side effect count is unchanged when the value is already set. VM property in `MainWindowViewModel`/`EventScheduleViewModel`/`PreferencesViewModel`/`EditEventViewModel`: ladder (a)/(b)/(c) |
| IDE0054 compound assign | 2 | Low | `x = x + y` → `x += y`: same unless `x` is a side-effecting property getter/setter or an event | none | Confirm target is a local/field; otherwise as IDE0074 |
| IDE0031 `?.` | 11 | Low–Questionable | Target evaluated once instead of twice (benign), but result type becomes nullable and a non-null branch with side effects can be skipped | `PlaylistTests` (Playlist.cs) | Playlist: pin the null and non-null branches. WPF code (`TextBoxExtender`, `TreeViewSelectionHelper`, `DataWindow`): read each; apply only where the guarded expression is a pure call |
| IDE0041 `is null` | 4 | Low–Questionable | `ReferenceEquals(x, null)` → `is null` bypasses a user-defined `==`. Equivalent only if the type does not overload `==` | `PendingEventTests` | Check the compared types for `operator ==`; if any do, pin equality-vs-null behavior first |
| IDE0044 readonly | 3 | None | Compiler-enforced | n/a | none |
| IDE0036 / IDE0040 | 2 / 5 | None | Modifier order / accessibility keyword only | n/a | none |
| IDE0060 unused param | 1 | Low | `Device(…, isDefault)`: removing a parameter changes the ctor signature; callers fixed at compile time. Check it is not bound by reflection/serialization | none | Check for named-arg/reflective use; if `Device` is constructed from a COM/enumeration path, ladder (a) |
| CS8632 | 3 | None if annotations are removed | Removing `?` changes nothing at runtime. Do **not** enable `#nullable` (would produce new warnings) | n/a | none |
| CA1510 | 6 | Low | `ThrowIfNull(x)` keeps `ArgumentNullException` and the param name, but the message text differs | `ConcatenatingAudioFileReaderTests`, others | Only where a public method's null guard is asserted: assert exception **type and `ParamName`**, not message |
| CA1861 | 7 | Low | `new[]{…}` per call → shared static array. Safe only if no caller mutates it | `SampleAggregatorTests`, `FileExtensionsTests`, `PersistenceServiceTests` | Product hits (`PersistenceService`, `PreferencesViewModel`): confirm array is never mutated or exposed. Test hits: none |
| CA1860 / CA1825 | 2 / 1 | None | `Count`/`Length` check or `Array.Empty` | n/a | none |
| CA1869 | 1 | Low | Cached `JsonSerializerOptions` is shared, and mutation after first use throws | `WunderGroundObservationTests`, `JsonSerializationTests` | Existing serialization round-trips are the guard; run them before and after |
| CA1870 | 1 | **Questionable** | `SearchValues` replacing `IndexOfAny` on invalid-path chars in `PreferencesViewModel`: behavior must match | none | Extract validation to a helper if inline; test invalid chars, empty, path with allowed chars, with the same inputs before and after |
| CA1852 seal | 3 | Low | Compile error if anything derives. Check for Catel dynamic proxies/reflection creating subclasses | none | Verify no derivation and no proxying (`InternetStreamEntryViewModel` is a Catel VM) |
| CA1802 const | 1 | None | Private field in `WunderGround` | `WunderGroundTests` | none |
| CA1810 static ctor | 1 | **Questionable** | `AudioPlayback` static ctor ordering and exception semantics (type-initializer failures) | no (WASAPI) | Read it first. If it is pure field setup, inline it (ladder a). If it does logic (device enumeration, registration), suppress with justification (ladder c) |
| CA1859 | 3 | None | Private types and signatures only; compile-checked. Already `suggestion` in Tests | n/a | none |
| CA1849 | 5 | Low | Tests only | `PersistenceServiceTests`, `FileExtensionsTests` | The tests *are* the change. Confirm same assertions still run and pass |
| CA1851 | 4 | **Questionable** | Materializing a lazy sequence runs its side effects once, eagerly, and can change timing/ordering, memory use, or results if the source changes between enumerations | `ConcatenatingAudioFileReaderTests`, `AudioTrackParserServiceTests` | Add enumeration-count tests: feed a counting/side-effecting `IEnumerable` and assert results and number of source enumerations before and after. For `AudioTrackParserService`, assert the folder-scan result is identical for a nested `TempDirectory` tree |
| CA1304 / CA1305 / CA1309 | 4 / 4 / 3 | **Questionable** | Culture-dependent parsing/formatting/comparison. Target culture is `en-US` (project does not support others), so tests pin `en-US` behavior; the fix should still avoid needlessly breaking other cultures | `PercentageConverterTests` | See Phase 3 culture step |
| CA1001 `Clock` | 1 | **Questionable** | Adds `IDisposable`, so owners must dispose it or the timer leaks/ticks after owner death | none | Depends on timer type; see Phase 3 lifetime step |
| CA1816 | 1 | Low | `GC.SuppressFinalize(this)` in `AudioPlayback.Dispose`: only valid if no derived finalizer relies on it | none (WASAPI) | Read class: no finalizer, sealed or not, and `Dispose` idempotent. Ladder (a) |
| CA2211 `EventBus` | 2 | **Questionable** | Public static field `Action<Event>`: changing to a property/`event` changes who may invoke, assign, or clear it. External invokers fail to compile; `= null` resets in tests break | `EventBusTests` (non-parallel) | Existing tests must pass unchanged. Add pins for: subscribe, unsubscribe, trigger with no subscriber (no throw), multicast order |
| CA2211 `ScrollPlayingIntoViewBehavior` | (in above) | None | Likely a `DependencyProperty` field missing `readonly` | n/a | none |
| CA1819 array property | 1 | **Questionable** | `SampleAggregator` FFT array: switching to a copy or span changes aliasing and allocation on the audio hot path | `SampleAggregatorTests` | Pin whether the returned array is the live buffer (aliasing) and its contents. **Default: suppress** (hot path, internal consumer) unless the pins show no consumer relies on aliasing |
| CA1507 | 1 | None | `nameof(track)` yields the same string | n/a | none |
| CA1725 | 1 | Low | Mismatch with interface parameter name. Rename direction matters (see step) | `PersistenceServiceTests` | Fix the **interface** parameter name to `eventSchedule` rather than the implementation to `settings` (the interface name looks like copy-paste). Grep for named-argument callers (compile-checked) |
| Phase 4 suppressions | — | None | Config-only | n/a | none |

## Step list

Counts are *unique* warnings and are estimates. Re-baseline if they drift. Steps are ordered
low-risk to high-risk and group by rule family. "T" = test-first gate applies.

### Phase 0: Safety net (do first, standalone)
- [x] **0.1** Record the baseline and run the full suite green: `dotnet test --project 'Tests\RadioAutomation.Tests\RadioAutomation.Tests.csproj'`. Note pass/skip counts in the Progress log. *No commit.*
- [x] **0.2** Check whether `InternalsVisibleTo` exists for the test project (needed by ladder b). If not, decide whether to add it. *(Commit `build(tests): Expose internals to the test project` only if needed.)*
- [x] **0.3** Build the **testability inventory**: for each file in the table above, record under *Notes* below whether its risky code is reachable from the test project (yes / needs extraction / untestable). This turns the ladder into a concrete choice per file before any code changes.

### Phase 1: Style (IDE) rules
- [x] **1.1 T** IDE0074 + IDE0054 (34 unique). Triage every hit into local / field / model property / VM property. Test commit for model-property hits (PropertyChanged-count pins), then fix commit. Promote `dotnet_style_prefer_compound_assignment` to `error`. *Done: 92.* All 34 were field-backed lazy commands, one local, two float loop locals; no property targets, so no new tests needed.
- [x] **1.2 T** IDE0031 (11). Promote `dotnet_style_null_propagation` to `error`. *Done: 81.* All 11 were `if (x != null) x.Call()` or `x.Prop = v` guards; RHS was pure or already guarded, so the rewrite is equivalent and no new tests were needed.
- [x] **1.3 T** IDE0041 (4). Promote `dotnet_style_prefer_is_null_check_over_reference_equality_method` to `error`. *Done: 77.* Neither `Device` nor `PendingEvent` overloads `operator ==`, so `is null` is equivalent; no tests needed.
- [x] **1.4** IDE0044 (3). Promote `dotnet_style_readonly_field` to `error`. *Done: 74.* Compiler enforces readonly, so no behavior risk; no tests needed.
- [x] **1.5** IDE0036 + IDE0040 (7). Promote `csharp_preferred_modifier_order` to `error`. IDE0040 stays `warning`. *Done: 67.* Only explicit-default modifiers and `async virtual` reordering; no behavior change, no tests needed.
- [x] **1.6** IDE0060 (1). Promote `dotnet_code_quality_unused_parameters` to `error`. *Done: 66.* `Device(MMDevice, bool isDefault)` ignored `isDefault`; per user decision it now sets `IsDefault` (behavior change, nothing read it). Pinned equality and JSON first (`42b0296`); added `InternalsVisibleTo` via `AssemblyInfo.cs` (csproj item is ignored because `GenerateAssemblyInfo` is false).

### Phase 2: Compiler warning
- [x] **2.1** CS8632 (3). Remove `?` annotations in `MainWindowViewModel.cs` (fields, param, handler param); `#nullable` not enabled. *Done: 63.* Annotations are compile-time only, so no behavior change; no tests needed.

### Phase 3: Code analysis (CA)
- [ ] **3.1** Safe mechanical: CA1860, CA1825, CA1802, CA1859, CA1507. *(None: compiler-proven.)*
- [ ] **3.2 T** CA1510 (6). Add `ParamName`+type assertions for public guards that have none, then convert.
- [ ] **3.3 T** CA1861 (7) + CA1869 (1). Confirm no mutation of hoisted arrays; run serialization round-trips before and after.
- [ ] **3.4 T** CA1870 (1). Extract the invalid-path check if inline; pin inputs; convert.
- [ ] **3.5** CA1852 (3). Verify no derivation or proxying, then seal.
- [ ] **3.6** CA1849 (5). Test-only conversion to async APIs; confirm assertions are unchanged.
- [ ] **3.7 T** CA1851 (4). Enumeration-count characterization tests for `ConcatenatingAudioFileReader` and `AudioTrackParserService` first, then materialize.
- [ ] **3.8 T** Culture: CA1305, CA1304, CA1309 (11).
  - **Culture policy:** the project targets `en-US` and does not claim to support other cultures. Culture tests therefore **assert `en-US` behavior** (set `CurrentCulture`/`CurrentUICulture` to `en-US` in the test and restore it afterward). That behavior must be identical before and after every change. Non-`en-US` cultures are not a requirement, but a fix must not gratuitously break them: prefer the form that keeps the old implicit-culture behavior where it costs nothing, and add at most one smoke case (e.g. `de-DE`) per site to confirm no exception, not to pin exact output.
  - `PercentageConverter` (2): extend `PercentageConverterTests` under `en-US` over the inputs the binding can supply (`string` with `.` decimals, `double`, `int`, `null`, non-numeric). **Pin current behavior first.** Then pick the fix: `CultureInfo.CurrentCulture` keeps today's behavior in every culture and is the default; `InvariantCulture` is acceptable only if the `en-US` pins pass unchanged and the value is machine-oriented. Using WPF's `culture` argument is out unless the `en-US` pins pass, since it can differ from the thread culture.
  - `MainWindowViewModel` CA1305 (2): read the call sites. Format of user-visible strings or machine values (MQTT payload, file names)? Machine values → `InvariantCulture` is the fix, but test via an extracted formatter. Display strings → keep `CurrentCulture` explicitly. Ladder (a)/(b).
  - `DataWindow` CA1304 (4) / CA1309 (3): read lines ~217, ~218, ~800, ~806. For `ILanguageService.GetString`, pass the culture the parameterless overload resolves to (verify in Catel source, not from memory). For comparisons, use the explicit `StringComparison` that matches today's semantics; use `Ordinal` only when the strings are identifiers/keys. Ladder (a)/(c). Also check whether `DataWindow.cs` is vendored (~950 lines): if so, prefer a scoped suppression.
- [ ] **3.9 T** Lifetime/design, one commit each:
  - **CA1810** (`AudioPlayback` static ctor): read; inline only if pure field setup, else suppress with justification.
  - **CA1816** (`AudioPlayback.Dispose`): verify no finalizer, then add `GC.SuppressFinalize(this)`.
  - **CA1001** (`Clock`): read `Clock.cs`. If the timer is `System.Timers.Timer`/`System.Threading.Timer`, add tests (ticks raised, `Dispose` stops ticks, double `Dispose` safe) *before* adding `IDisposable`, and update the owner to dispose it. If it is a `DispatcherTimer`, that is untestable (STA): pull the tick source behind a small abstraction or suppress with a note.
  - **CA2211** (`EventBus`, `ScrollPlayingIntoViewBehavior`): add `EventBus` pins first; keep the public usage shape (`EventBus.EventTriggered` subscribe/invoke) working for the existing tests unmodified.
  - **CA1819** (`SampleAggregator`): add aliasing/content pins; default is suppress with a "hot path" justification.
  - **CA1725** (`PersistenceService`/`IPersistenceService`): rename the interface parameter to `eventSchedule`, check named-argument callers, and run `PersistenceServiceTests`.

### Phase 4: Policy decisions (suppress rather than fix), one `build(analyzers)` commit
- [ ] **4.1** CA1707 (`Radio_Automation` namespace) → `none` for the main project; CA1716 (`Event`) → suppress; CA1711 (`NonParallelCollection`) → suppress in `Tests/.editorconfig`; CA5394 (`Random`, shuffle only, not security) → scoped suppression on the 4 sites with justification, **after** confirming via `ThreadSafeRandomTests`/`CollectionExtensionsTests` that those are shuffles, not security tokens; CA1824 → add `[assembly: NeutralResourcesLanguage("en-US")]`. Each line gets a justification comment.
- [ ] **4.2** WiX warnings (`WIX8601`, `WIX1076` / ICE71) are out of scope: the installer project, not the C# rules. Leave and note in Progress log unless you ask otherwise.

### Phase 5: Lock-in
- [ ] **5.1** Promote the "Make error" rules that were already clean and need no fix: `dotnet_style_collection_initializer`, `dotnet_style_explicit_tuple_names`, `csharp_style_conditional_delegate_call`, `csharp_using_directive_placement`. Rebuild `--no-incremental` first to confirm 0 hits. Do **not** promote any `suggestion`-level rule (out of scope).
- [ ] **5.2** Final `--no-incremental` build: expected **0 C# warnings** (WiX excluded) and 0 errors. Full test suite green with the same pass count as 0.1 **plus** the new tests, and no new skips except documented known defects.

## Commit pairing summary

| Step | Test commit first? | Fix commit | Rule promoted |
|---|---|---|---|
| 1.1 | yes (model-property pins) | `refactor(style): Use compound assignment` | prefer_compound_assignment |
| 1.2 | yes (Playlist) | `refactor(style): Use null propagation` | null_propagation |
| 1.3 | if `==` overloaded | `refactor(style): Use is null checks` | is_null_check |
| 1.4–1.6 | no | one commit each | readonly_field, modifier_order, unused_parameters |
| 2.1 | no | `fix(nullable): Drop unscoped annotations` | none |
| 3.2–3.4 | yes | `perf`/`refactor` per family | none |
| 3.7 | yes | `refactor(enumeration): Materialize once` | none |
| 3.8 | yes (culture matrix) | `fix(globalization): Make cultures explicit` | none |
| 3.9 | yes, per item | one commit per item | none |
| 4.1 | no | `build(analyzers): Suppress by policy` | none |
| 5.1 | no | `build(analyzers): Promote clean rules to error` | the four above |

## Notes (fill in during Phase 0.3)
- `InternalsVisibleTo`: **absent** (no csproj/props/source hit). Not added up front; the first step that
  needs ladder (b) adds it in its own `build(tests)` commit. Prefer making an extracted helper `public`
  only if it is already a public type's member.
- Reachability (test project references `Radio Automation`, `NAudioWrapper`, `Weather`; main types are public):
  - **yes (no STA/device):** `Playlist`, `Event`/models, `EventBus`, `PendingEvent`, `PersistenceService`,
    `AudioTrackParserService`, `ConcatenatingAudioFileReader`, `SampleAggregator`, `PercentageConverter`,
    `WunderGround`, extensions/utils.
  - **yes, new tests needed:** `Models/Clock.cs` (uses an `Elapsed` handler, so a `System.Timers.Timer`; confirm in 3.9
    before writing tick/`Dispose` tests).
  - **needs extraction or behavior-preserving form (ladder a/b):** `MainWindowViewModel`, `EventScheduleViewModel`,
    `PreferencesViewModel`, `EditEventViewModel` (public Catel VMs; construction pulls the dependency resolver,
    services, and in `MainWindowViewModel` audio/MQTT, so treat as untestable directly).
  - **untestable (STA / Dispatcher / device):** `Windows/DataWindow.cs` (a `Window`; the header doc comment suggests
    an adapted Catel `DataWindow`, so prefer scoped suppression), `TextBoxExtender`, `TreeViewSelectionHelper`,
    `ScrollPlayingIntoViewBehavior`, `NAudioWrapper/AudioPlayback.cs`, `Device`.
- Existing tests per Questionable rule: see the risk table; confirm with `change(tests)` at the start of each step.
- Test warnings (CA1849/1861/1825/1869/5394/1711) live in the test project, so those steps change tests only.

## Progress log
| Date | Step | Unique warnings after | Tests (pass/skip) | Notes |
|---|---|---|---|---|
| 2026-10-26 | baseline | 126 | 116 / 0 | 217 raw |
| 2026-10-26 | 0.1-0.3 | 126 | 116 / 0 | No code changes. `InternalsVisibleTo` absent; inventory in Notes |
| 2026-10-26 | 1.1 | 92 | 116 / 0 | 34 unique hits (not 36); rule promoted to error |
| 2026-10-26 | 1.2 | 81 | 116 / 0 | 11 hits; rule promoted to error |
| 2026-10-26 | 1.3 | 77 | 116 / 0 | 4 hits; no operator == overloads; rule promoted to error |
| 2026-10-26 | 1.4 | 74 | 116 / 0 | 3 hits; rule promoted to error |
| 2026-10-26 | 1.5 | 67 | 116 / 0 | 7 hits; modifier_order promoted to error |
| 2026-10-26 | 1.6 | 66 | 122 / 0 | 1 hit; Device.IsDefault now set (intentional); IVT added; rule promoted to error |
| 2026-10-26 | 2.1 | 63 | 122 / 0 | 3 hits; annotations removed, #nullable not enabled |
