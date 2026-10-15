# Chordyceps --- Project Overview

> **Purpose:** The central reference for Chordyceps' product vision,
> features, technical direction, development workflow, and release
> targets. Keep this document high-level; detailed implementation rules
> belong in focused files under the project documentation.

## 1. Project Identity

**Chordyceps** is a solo-developed, Android-first mobile guitar-playing
simulator built in Unity. Its core fantasy is simple:

> Pick up a virtual guitar, choose a song, and play it.

The intended feel is inspired by the free-play guitar experience in *The
Last of Us Part II*: a player interacts directly with a six-string
guitar, changes chord shapes and keys, and plays music in an immersive
3D scene.

-   **Project:** Chordyceps_Guitar
-   **Engine:** Unity 6000.3.7
-   **Primary platform:** Android
-   **Development model:** One-person project supported by an AI coding
    agent and Git/GitHub
-   **Target launch window:** January 1--15, 2027
-   **Primary performance target:** Aim for a stable 60 FPS on mid-range
    Android devices, while prioritizing responsive guitar input and low
    audio latency.
-   **Primary product priority:** Deliver a small, polished, convincing
    guitar-playing loop before expanding the content or feature set.

## 2. Product Goals and Target Audience

### 2.1 Product goals

1.  Make virtual guitar playing feel immediate, responsive, and
    satisfying on a touchscreen.
2.  Let players explore chords, keys, capo positions, and chord shapes
    without needing a physical guitar.
3.  Let players play along with a curated song library.
4.  Make the interaction visually engaging through a 3D character,
    guitar, and room.
5.  Ship a focused, reliable Android MVP rather than an overambitious
    feature list.
6.  Establish clean, reusable systems that can support future expansion
    without premature complexity.

### 2.2 Intended audience

-   People who enjoy guitar music and want to play along with songs.
-   Beginner and casual guitar players who want to explore chord
    progressions.
-   Music lovers who want a tactile, expressive music toy on their
    phone.
-   Players attracted to immersive, character-based presentation.

### 2.3 Product principles

-   **Playability first:** input and audio responsiveness matter more
    than visual complexity.
-   **Scope discipline:** the MVP should be frozen unless a change is
    essential to release quality.
-   **Mobile first:** design around touch, screen space, memory,
    battery, and performance constraints.
-   **Data-driven content:** prefer reusable data assets over hard-coded
    song and chord definitions.
-   **Clear ownership:** each system should have one primary
    responsibility.
-   **Legal content:** do not redistribute copyrighted music or
    recordings without the required rights.
-   **Incremental development:** build, test, and stabilize small pieces
    before adding more.
-   **Honest status reporting:** never describe code as tested or
    working unless it has actually been tested.

## 3. Core Gameplay Loop

1.  The player enters the guitar scene.
2.  The player chooses a song or enters free-play mode.
3.  The player selects a musical key and chord shape.
4.  The player optionally adjusts the capo.
5.  The player taps individual strings or swipes across the strings to
    strum.
6.  The game plays the appropriate string audio and updates the visual
    presentation.
7.  In song mode, the player follows a song timeline and plays along
    with the progression.
8.  The player can return to the song library, adjust settings, or
    continue experimenting.

The first release succeeds when this loop feels good, is understandable
without extensive instruction, and works reliably on target Android
devices.

## 4. Version 1.0 Feature Scope

### 4.1 Guitar interaction

**Required for v1.0**

-   Six playable guitar strings.
-   Touch input for tapping a single string.
-   Swipe/strum input across strings; a swipe may trigger one or
    multiple strings depending on the path and interaction rules.
-   Multi-touch support where useful and reliable.
-   Responsive input with minimal perceived latency.
-   Input handled through Unity's New Input System.
-   Editor-only mouse and keyboard bindings to support development and
    testing in the Unity Editor.
-   Input code communicates player intent/events to gameplay systems
    rather than owning chord logic or audio playback itself.

### 4.2 Chords and musical controls

**Required for v1.0**

-   A chord wheel or equivalent interface for selecting chord shapes.
-   Twelve musical keys.
-   Three major and three minor chord choices in the currently selected
    key, as defined by the intended chord-wheel design.
-   Six chord shapes available through the chord-shape selector.
-   Controls to cycle through keys and chord shapes.
-   Capo increase/decrease controls.
-   Clear visual feedback for the currently selected key, chord shape,
    and capo position.
-   Reusable, data-driven chord definitions wherever practical.

Musical controls should be understandable on a mobile screen and should
not obstruct the strings or make strumming difficult.

### 4.3 Audio and guitar sound

**Required for v1.0**

-   Audio clips for individual strings/notes as required by supported
    chord shapes.
-   Reliable playback of repeated string taps and quick strums.
-   Audio voice/polyphony management so overlapping notes can play
    together.
-   A controlled limit on simultaneous voices to protect mobile
    performance.
-   Basic guitar effects and tone controls, prioritized after core
    playback is stable.
-   Volume control.
-   Basic reverb.
-   Gain/overdrive or distortion control.
-   Basic bass, mid, treble, and presence controls if they can be
    delivered reliably within the schedule and performance budget.
-   Consistent note onset and carefully prepared audio assets.
-   Audio behavior tuned for low latency on mobile.

**Audio implementation direction**

-   Prefer preloaded/decoded audio assets for frequently played notes
    where memory permits.
-   Avoid creating expensive playback infrastructure on every tap.
-   Use reusable audio playback components and a deliberate
    voice-allocation policy.
-   The current desired voice-pool behavior is not a fixed three
    AudioSources per string: use a shared pool with a target of 18
    voices, allowing several strums to overlap. If the pool is
    exhausted, drop the oldest/least useful voice according to the
    chosen, documented policy. Repeated taps should continue to sound
    until voices must be reclaimed.
-   Profile memory and latency on actual devices; do not assume that
    desktop Editor performance represents Android performance.

The first release does **not** require a physically accurate amplifier
simulator or a complete guitar synthesizer.

### 4.4 Song library and playback

**Required for v1.0**

-   A small, curated song library.
-   A song-selection screen.
-   Song metadata sufficient to display and identify each song.
-   A song timeline or progress display.
-   Song progression/playback data that identifies the chord and timing
    information needed by the game.
-   A way to play along with the selected song.
-   A manually authored content workflow for the initial release.
-   Clear separation between song data and the code that displays or
    plays it.

Only use music, recordings, arrangements, artwork, and other assets for
which the project has the necessary rights or permissions. A technically
accessible online recording is not automatically licensed for
redistribution or in-app use.

### 4.5 3D presentation

**Required for v1.0**

-   A seated character holding a guitar.
-   A clear, medium-shot presentation of the character and instrument.
-   A view in which the guitar remains recognizable and visually
    readable.
-   A simple room environment with a floor and two walls, or a similarly
    constrained environment.
-   Camera framing that keeps the performance and interaction readable
    on a phone.
-   Basic visual polish, lighting, and transitions as time permits.

**Deferred visual complexity**

-   Advanced inverse kinematics (IK) for highly accurate hand movement
    along the guitar neck.
-   Complex animation systems that do not materially improve the MVP.
-   Multiple elaborate environments or extensive character
    customization.

Use a practical animation solution for v1.0. Add advanced IK only when
the core experience is stable and the benefit justifies its
implementation and testing cost.

### 4.6 UI and user experience

**Required for v1.0**

-   Main guitar-playing screen.
-   Chord/key controls.
-   Capo controls.
-   Six-string touch/strum area.
-   Song library and song selection.
-   Song timeline/progress display.
-   Basic tutorial or onboarding.
-   Settings screen.
-   Persistent settings or save data for relevant player preferences.
-   Touch-friendly sizing, spacing, and layout.

**Layout & Visual Benchmark:** `Cozy Guitar Practice Interface.png`
Key screen elements for the main play interface:
1. **Center:** 3D character seated playing guitar in a cozy, atmospheric room.
2. **Bottom-Right:** 6-string horizontal interaction/strumming zone (1e to 6E) with vertical strum bar.
3. **Upper-Right:** Diatonic chord wheel (I, IV, V, ii, vi, iii) with Roman numerals and chord names.
4. **Top-Center:** Capo changer stepper (`-`, current fret, `+`).
5. **Upper/Middle-Left:** Key selector (`< Key >`) and tuning selector.
6. **Bottom-Left:** Chord shape analog stick / dial (positions 1 to 6).

**UI technology direction**

-   Use **UI Toolkit** for 2D-heavy interface screens where it is a good
    fit.
-   Use the normal Unity scene and GameObjects for the 3D character,
    guitar, room, and world presentation.
-   Use TextMeshPro where it remains appropriate, especially for
    existing or world-space text needs. Do not introduce unnecessary UI
    technology changes merely for consistency.
-   Keep UI presentation separate from gameplay rules and audio
    playback.

### 4.7 Tutorial, settings, save data, analytics, and monetization

**Included in the release plan**

-   A short tutorial/onboarding flow explaining the basic interaction.
-   Basic audio and gameplay settings.
-   Saving appropriate user preferences and progress.
-   Minimal analytics to understand stability and broad feature usage,
    with privacy-conscious implementation.
-   A simple monetization implementation appropriate for a small mobile
    release, such as ads and/or optional purchases, only once the core
    experience is stable.
-   Graceful behavior when analytics, advertising, or network services
    are unavailable.

Do not let monetization or analytics delay the core guitar loop. Avoid
collecting unnecessary personal data.

## 5. Explicitly Out of Scope for Version 1.0

These features may be considered after launch, but should not threaten
the initial release:

-   MIDI/XML song playback and general MIDI import.
-   Automatic chord detection or automatic "chordification" of arbitrary
    songs.
-   A huge library containing hundreds or thousands of songs.
-   Multiplayer, social systems, or real-time collaboration.
-   Advanced, physically accurate amp simulation.
-   Complex pitch-shifting or a full synthesizer.
-   Advanced IK and highly detailed finger/fretboard simulation.
-   A large backend/content platform before the basic game is complete.
-   Multiple large environments and extensive cosmetic systems.
-   Any feature that requires a substantial architecture change without
    a clear MVP benefit.

If a deferred feature becomes essential, explain the impact on schedule,
complexity, performance, licensing, and existing systems before changing
the scope.

## 6. Technical Direction

### 6.1 Core technology

-   Unity 6000.3.7.
-   Universal Render Pipeline (URP), unless a demonstrated project
    constraint justifies otherwise.
-   Unity New Input System.
-   Git and GitHub for version control and collaboration with the AI
    agent.
-   ScriptableObjects for reusable, authored game data.
-   UI Toolkit for suitable 2D interface work.
-   TextMeshPro where appropriate.
-   Android as the primary target platform.

### 6.2 Architecture boundaries

Keep these responsibilities separate:

-   **Input layer:** detects touch, tap, swipe, and editor-only input;
    emits intent/events.
-   **Gameplay layer:** interprets input, manages selected
    key/chord/capo, and coordinates gameplay behavior.
-   **Chord/data layer:** stores and provides chord, key, and shape
    definitions.
-   **Guitar audio layer:** selects and plays the correct note/string
    clips, manages voices, and applies audio processing.
-   **Song system:** owns song data, progression, timing, and playback
    state.
-   **UI layer:** presents state and sends player commands without
    becoming the owner of the underlying rules.
-   **3D presentation layer:** controls visual objects, character,
    guitar, camera, and environment.
-   **Persistence/settings layer:** saves and restores supported
    settings and player preferences.
-   **Analytics/monetization integrations:** remain isolated behind
    narrow interfaces so they do not become dependencies of the core
    guitar interaction.

These are responsibility boundaries, not a requirement to create a class
for every bullet. Prefer the smallest useful architecture that keeps
ownership clear.

### 6.3 ScriptableObjects and data-driven design

Use ScriptableObjects when data should be authored, inspected, reused,
or tuned in the Unity Editor. Suitable examples include:

-   Chord definitions and chord shapes.
-   Key/chord mappings.
-   String audio clip mappings.
-   Song metadata and song progression data.
-   Configurable gameplay or audio settings where asset-based
    configuration is beneficial.

Avoid turning every runtime value or every class into a
ScriptableObject. Use plain C# objects or components when they are a
better fit. Keep authored data separate from mutable runtime state
unless shared mutation is explicitly intended.

### 6.4 Performance and responsiveness

-   Treat touch-to-sound latency as a first-class quality requirement.
-   Avoid unnecessary allocations and expensive work in frequent
    input/audio paths.
-   Preload common audio assets when appropriate, while measuring memory
    use.
-   Test overlapping notes, rapid repeated taps, and dense strums.
-   Profile on representative Android devices.
-   Aim for stable 60 FPS on mid-range Android hardware, but investigate
    any trade-off between visual work and responsive audio/input.
-   Keep the scene, UI, and effects appropriate for mobile hardware.
-   Do not optimize blindly: measure first, then make the smallest
    effective change.

## 7. SOLID Principles

SOLID is a design guide for keeping code understandable and changeable.
Apply it pragmatically; do not introduce abstractions merely to
demonstrate a principle.

### S --- Single Responsibility Principle

A class or module should have one primary reason to change.

**Chordyceps examples** - Input detects gestures and publishes intent;
it does not decide which audio clip to play. - Chord selection manages
selected musical state; it does not render the entire UI. - Guitar audio
chooses and plays clips; it does not own the song library screen. - Song
data stores song information; a playback system interprets its timing.

Prefer focused components and avoid "God objects" that own input,
chords, UI, songs, and audio together.

### O --- Open/Closed Principle

Software entities should be open to extension but closed to repeated
modification.

**Chordyceps examples** - Adding a new song should usually mean
authoring song data, not editing the core playback logic. - Adding a
chord shape should use the existing data/configuration path where
possible. - New audio effects should fit into an existing
audio-processing boundary rather than requiring unrelated gameplay code
to change.

Do not build elaborate extension frameworks before a real extension need
exists.

### L --- Liskov Substitution Principle

A subtype should be usable wherever its base type is expected without
breaking the expected behavior or contract.

**Chordyceps examples** - If multiple audio playback implementations
share an interface, each must honor the same playback and
voice-management expectations. - If song sources have a shared contract,
every implementation must provide valid song data and consistent
playback behavior.

Favor composition over inheritance when it produces simpler behavior. Do
not create inheritance hierarchies without a real need.

### I --- Interface Segregation Principle

Clients should not be forced to depend on methods they do not use.

**Chordyceps examples** - A UI element that only needs to request a
chord change should not depend on a large interface that also exposes
audio pooling, song loading, and persistence. - Separate narrow
capabilities when distinct clients need distinct operations.

Keep interfaces small and meaningful; avoid creating interfaces for
every class by default.

### D --- Dependency Inversion Principle

High-level policy should not depend directly on low-level implementation
details. Both should depend on suitable abstractions when that reduces
coupling.

**Chordyceps examples** - Gameplay should request guitar-note playback
through a focused contract rather than knowing how audio voices are
allocated internally. - Song playback should consume song data through a
stable boundary rather than depending on UI widgets. - Analytics and
monetization should be replaceable integrations, not requirements for
the guitar to function.

Use constructor injection, serialized references, or other
Unity-appropriate dependency wiring based on the situation. Do not add a
dependency-injection framework unless project complexity genuinely
justifies it.

### SOLID application rule

Apply the principle when it solves a real problem: mixed
responsibilities, repeated changes, fragile dependencies, hard-to-test
behavior, or a growing feature. Keep the architecture proportional to
the size of the project.

## 8. AI Coding Agent Rules and Development Workflow

The AI coding agent works in the repository under human direction. The
user remains responsible for running and confirming tests.

### 8.1 General coding rules

-   Be conservative. Preserve existing behavior unless the task requires
    a change.
-   Prefer clean, reusable systems without unnecessary complexity.
-   Functions should generally be **20 lines or fewer**, unless there is
    a good reason to exceed that.
-   Explain complex logic; do not over-comment obvious code.
-   Use getters/setters or another controlled access pattern when
    variable accessibility affects architecture.
-   Choose safe, performance-conscious solutions.
-   Inspect relevant dependencies and callers before changing behavior.
-   Mention small issues discovered during work and optionally fix them
    if the fix is genuinely tiny and safe.
-   Refactor only when necessary for the feature or to prevent a clear
    problem.
-   Preserve useful logs.
-   Do not claim that code compiles, runs, or passes tests unless that
    has actually been verified.
-   The agent may commit when a feature is complete, following the
    repository's conventions.
-   Never perform destructive or irreversible changes without explicit
    approval.

### 8.2 Feature workflow

1.  **Understand:** inspect relevant files, dependencies, existing
    patterns, and project instructions.
2.  **Plan:** summarize the proposed change, scope, assumptions, and
    likely affected files.
3.  **Confirm:** ask for approval before implementation when the task
    involves meaningful assumptions or architectural decisions.
4.  **Implement:** make the smallest coherent change that meets the
    requirement.
5.  **Review:** inspect the diff, check for regressions, and identify
    any unaddressed issues.
6.  **Report:** summarize what changed, assumptions, risks, and any
    checks actually performed.
7.  **Hand off testing:** provide concrete Unity Editor/device test
    steps, expected results, edge cases, and relevant Console checks.
8.  **Complete:** the feature is not considered fully confirmed until
    the user tests it and reports the result.

For tiny, unambiguous changes, keep the plan proportionate. If the work
reveals a significant architecture problem, scope expansion, or a risky
decision, stop and explain the issue before proceeding.

### 8.3 Bug-fix workflow

1.  Reproduce or understand the reported behavior from available
    evidence.
2.  Trace the relevant code path and dependencies.
3.  Identify the likely root cause; distinguish confirmed facts from
    hypotheses.
4.  Propose the smallest safe fix.
5.  Avoid unrelated refactoring.
6.  Report the change and provide steps to verify the fix, including
    regression cases.
7.  Do not claim the bug is fixed in runtime until the user or a
    verified test confirms it.

### 8.4 Code review workflow

-   Focus on correctness, regressions, data loss, performance,
    maintainability, and architecture.
-   Inspect callers and dependencies, not just the changed method.
-   Keep findings concise and prioritize significant problems.
-   Preserve useful logging and existing intent.
-   If a major architectural issue is found, stop and explain it rather
    than silently redesigning the project.
-   Mention minor issues separately; do not let them obscure
    release-blocking problems.

### 8.5 Testing ownership

The user performs the project tests. For each feature, the agent should
provide:

-   Setup or scene required.
-   Exact steps to perform.
-   Expected behavior.
-   Edge cases.
-   Unity Console or device logs to inspect.
-   Any platform-specific checks.
-   A clear statement of what was and was not tested by the agent.

## 9. Repository Documentation Structure

Keep project instructions in small, focused Markdown files rather than
relying on one enormous document. This file is the high-level overview;
detailed instructions should live near the relevant workflow.

Suggested structure:

``` text
Chordyceps_Guitar/
├── AGENTS.MD
├── README.md
├── docs/
│   ├── chordyceps-overview.md
│   ├── roadmap.md
│   ├── architecture.md
│   ├── features/
│   │   ├── guitar-input.md
│   │   ├── chords-and-keys.md
│   │   ├── guitar-audio.md
│   │   ├── songs-and-playback.md
│   │   ├── ui-and-presentation.md
│   │   └── save-settings-analytics.md
│   └── workflows/
│       ├── feature.md
│       ├── bug-fix.md
│       └── review.md
└── .agent/
    └── skills/
        ├── Unity-core/
        │   └── SKILL.md
        └── guitar-audio/
            └── SKILL.md
```

This is a suggested organization, not a mandate to create every file
immediately. Add documents when they help the next development task.

## 10. Roadmap and Milestones

The order below prioritizes risk reduction. Adjust individual tasks
based on actual progress, but protect the core gameplay loop and the
launch window.

### Phase 1 --- Project foundation

-   Establish the Unity project, version control, and repository
    conventions.
-   Set up the core scene and mobile build configuration.
-   Establish the initial input, gameplay, and audio boundaries.
-   Document the agent workflow and project constraints.

**Exit condition:** a clean foundation exists and a basic Android build
can be produced.

### Phase 2 --- Playable guitar prototype

-   Implement six-string touch input.
-   Implement tap and swipe/strum behavior.
-   Connect input intent to gameplay and audio.
-   Verify repeated taps, overlapping notes, and responsive playback.

**Exit condition:** the player can interact with the strings and hear
the expected sounds reliably.

### Phase 3 --- Chords, keys, and capo

-   Implement the chord data model and chord-shape selection.
-   Add twelve keys and the planned chord-wheel behavior.
-   Add capo controls.
-   Confirm that musical selections map to the correct string audio.

**Exit condition:** chord/key/capo changes consistently affect the
playable guitar.

### Phase 4 --- Song system

-   Define a data format for songs and progressions.
-   Add a small set of manually authored songs with properly licensed
    content.
-   Implement song selection, timeline/progress, and playback state.
-   Connect song progression to the chord-playing experience.

**Exit condition:** a player can select a song and play along with its
progression.

### Phase 5 --- UI and presentation

-   Complete the main guitar UI and song library.
-   Add the 3D character, guitar, camera, and simple room.
-   Add basic onboarding, settings, and feedback.
-   Test layout and interaction on representative phone aspect ratios.

**Exit condition:** the game is understandable and visually coherent on
a phone.

### Phase 6 --- Audio polish and persistence

-   Tune basic effects and tone controls.
-   Refine audio onset and voice-pool behavior.
-   Add settings persistence and the required save data.
-   Profile latency, memory, and performance on real Android hardware.

**Exit condition:** the core experience is stable, responsive, and
configurable.

### Phase 7 --- Release preparation

-   Add minimal analytics and monetization if they fit the schedule.
-   Test fresh installation, settings persistence, interruptions, and
    failure cases.
-   Fix high-priority bugs and performance problems.
-   Prepare store assets, description, privacy disclosures, and release
    configuration.
-   Produce and test release builds.

**Exit condition:** the MVP meets the release checklist and can be
submitted within the target window.

### Schedule note

The target launch window is **January 1--15, 2027**. The schedule is a
target, not a reason to ship a broken core experience. Freeze scope
early, reserve time for device testing and release preparation, and
defer nonessential features rather than sacrificing stability.

## 11. Definition of Done

A feature is ready for user testing when:

-   It meets the agreed requirement.
-   The implementation follows existing architecture and project rules.
-   Responsibilities and dependencies remain clear.
-   The change is limited to the intended scope.
-   The diff has been reviewed for obvious regressions.
-   The agent has reported assumptions and known limitations.
-   Test steps, expected results, and edge cases are documented.

A feature is **confirmed complete** only after the user runs the tests
and confirms the expected behavior, unless an automated or otherwise
verifiable test genuinely covers the requirement.

## 12. Release Acceptance Checklist

### Gameplay

-   [ ] Six strings respond correctly to touch.
-   [ ] Tap, swipe, and strum behavior is predictable.
-   [ ] Chord shapes and twelve keys work correctly.
-   [ ] Capo controls work correctly.
-   [ ] Repeated and overlapping notes behave acceptably.

### Songs

-   [ ] Song library can be opened and used.
-   [ ] Song data and progression are correct.
-   [ ] Timeline/progress display behaves correctly.
-   [ ] All included audio and song content is appropriately licensed.

### Audio and performance

-   [ ] Audio starts promptly after input.
-   [ ] Voice-pool exhaustion behaves according to the documented
    policy.
-   [ ] Basic effects and volume controls work.
-   [ ] Memory use is acceptable on target devices.
-   [ ] Performance is stable on representative Android hardware.

### UX and presentation

-   [ ] Main UI is readable and touch-friendly.
-   [ ] Tutorial/onboarding explains the core loop.
-   [ ] Character, guitar, and environment present the intended
    experience.
-   [ ] Settings persist as expected.
-   [ ] Common screen sizes and aspect ratios are checked.

### Release readiness

-   [ ] Fresh installation and first launch tested.
-   [ ] App interruption/resume tested.
-   [ ] Important Unity Console errors resolved.
-   [ ] Analytics/monetization failure does not break core gameplay.
-   [ ] Privacy and store requirements reviewed.
-   [ ] Release build tested on a physical Android device.

## 13. Decision-Making Rules

When choosing between implementation options, prioritize in this order:

1.  Correct, satisfying guitar interaction.
2.  Low input/audio latency and stable mobile performance.
3.  Reliable core behavior and clear system ownership.
4.  A usable, understandable mobile UI.
5.  A polished but appropriately small 3D presentation.
6.  Maintainability and future extensibility where they solve real
    problems.
7.  Optional features and extra content.

When a decision could significantly affect architecture, scope,
licensing, performance, or the launch date, the agent should explain the
trade-offs and ask before implementation.

## 14. Final Project North Star

**Chordyceps should feel like a small, responsive virtual guitar that
invites the player to pick it up and make music.**

Every feature should support that experience directly or help the
project ship safely. Build the simplest complete version first, keep the
code clean and modular, test on real devices, and expand only after the
core experience earns it.
