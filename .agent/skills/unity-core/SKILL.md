---
name: unity-core
description: >-
  Core Unity development practices and lifecycle conventions for Chordyceps targeting Unity 6000.3.7. Use when creating MonoBehaviours, configuring components, or structuring scenes.
---

# Unity Core Skill

## Purpose

Provide safe, maintainable, modern Unity development practices for **Unity 6000.3.7**, with a focus on clean architecture, performance, and predictable behavior.

## Unity Version

- Target **Unity 6000.3.7**.
- Prefer modern Unity APIs and workflows available in this version.
- Avoid deprecated or legacy approaches unless the existing project already depends on them and changing them would be unnecessary scope.

## Architecture

- Prefer small, focused `MonoBehaviour` components with one clear responsibility.
- Favor composition over large "god" controllers.
- Keep gameplay logic independent where practical from presentation, input, and UI.
- Reuse existing systems before creating new ones.
- Do not introduce architecture or abstractions without a concrete need.
- If a requested feature exposes an architectural problem that cannot be safely solved locally, stop and report it before changing the architecture.

## MonoBehaviour Lifecycle

Use Unity lifecycle methods only when their behavior is actually required.

- Avoid unnecessary `Update`, `FixedUpdate`, `LateUpdate`, `Awake`, or `Start`.
- Prefer event-driven or explicitly called methods when continuous lifecycle execution is unnecessary.
- Use:
  - `Update` for frame-dependent logic.
  - `FixedUpdate` for physics-related logic that requires fixed-timestep execution.
  - `LateUpdate` when execution must happen after normal frame updates.
  - `Awake` for initialization that must occur when the component is loaded.
  - `Start` for initialization that depends on other components having completed their `Awake`.

- If adding a continuous lifecycle method, consider its performance cost and explain why it is necessary.

## Serialization and Inspector References

Prioritize project safety, maintainability, and performance.

- Prefer serialized references when an object can reasonably be assigned through the Inspector.
- Avoid repeatedly searching for objects at runtime with:
  - `GameObject.Find`
  - `FindObjectOfType`
  - repeated `GetComponent` calls in hot paths
  - unnecessary hierarchy searches

- Cache references when they are needed repeatedly.
- Do not use runtime searches simply for convenience when a safe serialized reference is practical.
- Avoid public fields unless public Inspector/runtime access is intentionally part of the design.
- Use the safest serialization/access pattern appropriate for the system rather than following a rigid rule.

Example:

```csharp
[SerializeField] private GuitarAudioPlayer audioPlayer;
```

rather than exposing internal implementation details unnecessarily.

## Accessibility

Control access to state based on architectural needs.

- Keep implementation details private by default.
- Use methods when state should only be changed through controlled behavior.
- Use properties/getters when other systems need read access.
- Use setters only when external systems genuinely need to modify the value.
- If variable accessibility could affect architecture, prefer controlled getters/setters or methods over exposing the field directly.
- Avoid making fields public merely to solve Inspector or access errors quickly.

Example:

```csharp
[SerializeField] private bool isPlaying;

public bool IsPlaying => isPlaying;
```

If external modification is required, use a controlled property or method rather than exposing the backing field unnecessarily.

## Performance

Performance decisions should be appropriate for a mobile game.

- Avoid unnecessary per-frame work.
- Avoid allocations in frequently executed code where practical.
- Avoid repeated hierarchy/component searches during gameplay.
- Cache frequently accessed references.
- Prefer event-driven logic when continuous polling is unnecessary.
- Consider garbage collection when designing frequently executed systems.
- Do not prematurely optimize code that is not performance-sensitive.
- When reviewing or implementing a system, flag complexity that could realistically become a performance problem on the target mobile hardware.

Performance optimizations must not make the code significantly harder to understand without a meaningful benefit.

## Methods

- Functions should generally remain **20 lines or fewer**.
- Split large functions into smaller methods when doing so improves readability or responsibility separation.
- Do not split code into meaningless one-line methods solely to satisfy the limit.
- Each method should have a clear purpose.

## References and Dependencies

- Prefer explicit dependencies over hidden dependencies.
- Avoid unnecessary singleton usage.
- Do not introduce global state unless the system genuinely requires it.
- Prefer dependency injection, serialized references, or explicit initialization when appropriate to the existing architecture.
- Do not add a new dependency/package without explaining why it is needed and what impact it has.

## Null and Reference Safety

Unity references should fail predictably.

- Validate critical serialized references where appropriate.
- Avoid silently continuing when a required reference is missing.
- Prefer clear error messages that identify the missing reference and affected system.
- Do not add excessive defensive checks to code where the reference is guaranteed by architecture.
- Use assertions when an invalid state represents a programmer/setup error and an assertion improves diagnosis.

Example:

```csharp
private void Awake()
{
    Debug.Assert(audioPlayer != null, "GuitarController requires an AudioPlayer.");
}
```

## Unity Debugging

Use the Unity Console as a development tool without creating unnecessary noise.

- Logs should explain meaningful state or events.
- Avoid logging every frame or every repeated input event.
- Use warnings for recoverable configuration/problems.
- Use errors when a system cannot function correctly.
- Use assertions for assumptions that should always be true.
- Remove temporary debugging code when it no longer provides value.
- Debug logs may remain when they provide useful ongoing diagnostics.
- Never claim a system works merely because it compiles; the developer must test it.

## GameObject and Component Design

- Keep components focused.
- Avoid putting unrelated gameplay, UI, audio, input, and presentation responsibilities into one component.
- Prefer communicating between systems through explicit interfaces, events, methods, or controlled references.
- Avoid excessive component coupling.
- Before creating a new component, inspect whether an existing component already owns the responsibility.

## Coroutines

Use coroutines when their asynchronous, frame-based behavior is actually useful.

Appropriate uses include:

- Delays.
- Multi-frame sequences.
- Timed gameplay behavior.
- Waiting for Unity-specific conditions.

Do not use a coroutine simply because a normal method would be sufficient.

## Events

Use events when systems need to react to state changes without tightly coupling themselves to the system producing the event.

Examples:

- Guitar string played.
- Chord changed.
- Song position changed.
- Audio state changed.

Do not introduce an event system for simple direct communication where a method call is clearer.

## ScriptableObjects

When a system requires reusable project data, configuration, or designer-editable assets, consider ScriptableObjects.

Examples relevant to Chordyceps include:

- Chord definitions.
- Guitar configuration.
- Song data.
- Audio configuration.
- Gameplay configuration.

Follow the dedicated `scriptable-objects` skill for detailed ScriptableObject practices.

## Input

Do not hard-code platform-specific input behavior into gameplay systems when avoidable.

Gameplay systems should preferably receive meaningful actions such as:

- Strum.
- String touched.
- Chord selected.
- Capo changed.

rather than directly depending on UI or touch implementation.

Follow the dedicated `mobile-input` skill when implementing touch/mobile interaction.

## Audio

Keep audio playback responsibilities separate from gameplay state where practical.

For Chordyceps:

- Gameplay determines what should happen.
- Audio systems determine how it is played.
- Avoid embedding complex audio playback logic directly inside input components.

Follow the dedicated `guitar-audio` skill for guitar-specific audio architecture.

## Code Changes

Before modifying existing code:

1. Inspect the relevant scripts.
2. Understand how the existing system works.
3. Reuse existing functionality where appropriate.
4. Make the smallest safe change required.
5. Preserve existing behavior unless the feature explicitly requires changing it.
6. Avoid unrelated refactoring.

If an existing implementation is unsafe or problematic, report it according to the active workflow rather than silently rewriting it.

## Core Rule

**Build simple, explicit, reusable Unity systems that are appropriate for Unity 6000.3.7 and mobile performance. Prefer clear architecture and safe dependencies over clever or unnecessarily complex solutions.**
