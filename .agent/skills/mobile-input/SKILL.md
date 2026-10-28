---
name: mobile-input
description: >-
  Mobile touch and input handling for Chordyceps using Unity's Input System, including multi-touch guitar interaction, strum detection, and PC editor simulation. Use when working on input handling or touch controls.
---

# Mobile Input Skill

## Purpose

Define responsive, low-latency input for Chordyceps using Unity's Input System, with multi-touch guitar interaction, editor simulation, PC development bindings, and strict separation between input and gameplay.

## Input System

- Use Unity's **new Input System**.
- Do not introduce the legacy Input Manager for new functionality.
- Keep input handling separate from gameplay logic.
- Input systems should translate raw device input into meaningful input intent/events.

Examples:

- String touched.
- String interaction started.
- String interaction ended.
- Chord wheel selection changed.
- Chord wheel interaction started.
- Chord wheel interaction ended.

Gameplay systems decide what those events mean.

## Single Responsibility

The input layer should answer:

> **"What did the player do?"**

It should not answer:

> **"What should the game do because of it?"**

For example:

```text
Touch string
    ↓
Input System
    ↓
String interaction event
    ↓
Guitar Gameplay
    ↓
Play string
```

Do not make input components directly:

- Play guitar audio.
- Change chords.
- Modify song state.
- Trigger UI behavior.
- Control gameplay state.

Keep those responsibilities in their appropriate systems.

## Multi-Touch

Multi-touch is supported from the beginning.

The system must not assume there is only one active touch.

Each active touch should be tracked independently where required so that simultaneous interactions do not overwrite or interfere with one another.

Potential simultaneous interactions include:

- Multiple string touches.
- String interaction + chord-wheel interaction.
- Multiple fingers interacting with different guitar areas.

Avoid global single-touch state when it would prevent legitimate simultaneous input.

## String Interaction (Bottom-Right Rectangle)

Interaction occurs within a dedicated **rectangle containing the 6 strings from bottom to top (Low E to High e)** in the bottom right of the screen:

- **Pluck (Tap):** The user taps for a singular string pluck.
- **Chord (Slide/Strum):** The user slides their finger along several strings in one movement.
- **Visual Wobble:** Plucked and strummed strings visually wobble upon contact.

A swipe across one string does **not** automatically mean that multiple strings should play.
The input system reports the actual interaction/path information required by gameplay to determine which strings were crossed.

Conceptually:

```text
Finger movement / tap
      ↓
Detect interaction in bottom-right strings rectangle
      ↓
Determine affected guitar string(s)
      ↓
Send string interaction event (type, index, velocity)
      ↓
Guitar gameplay triggers wobble + sound request
```

Do not hard-code "swipe = strum all strings."

## String Hit Detection

String interaction should be based on the actual touch position and movement.

The system should be capable of determining:

- Which string was interacted with.
- When interaction began.
- When the finger moved across a string.
- When interaction ended.
- Whether another string was crossed during the movement.

Avoid unnecessary physics queries or hierarchy searches for every input event.

Prefer efficient, predictable hit detection appropriate for the guitar's known screen-space representation.

## Input Latency

Responsiveness is a high priority.

Choose the input approach that minimizes unnecessary latency while remaining maintainable.

- Process input as close as practical to when Unity receives it.
- Avoid unnecessary intermediate polling layers.
- Avoid artificial delays between input detection and gameplay events.
- Do not use coroutines or timers for input unless they are genuinely required for gesture interpretation.
- Avoid expensive work inside input callbacks.
- Keep input event processing lightweight.
- Do not perform audio loading, asset searches, or expensive calculations directly during a touch event.

The input layer should quickly identify the interaction and pass it to the appropriate gameplay system.

## Update and Polling

Do not automatically use `Update()` for all input.

Use the Input System's event/callback mechanisms where they provide a clean and responsive solution.

Use polling when it is genuinely appropriate, such as tracking continuous pointer movement where the interaction requires it.

The goal is not "never use Update."

The goal is:

> **Use the input mechanism that provides the lowest practical latency and simplest reliable implementation for the interaction.**

## Chord Wheel

The chord wheel supports direct touch interaction.

It should support:

- Selecting a chord.
- Dragging around the wheel.
- Clear identification of the currently selected chord.
- Responsive movement while dragging.
- Appropriate handling of touch beginning, movement, and ending.

The input layer should report the player's wheel interaction rather than directly changing the active chord.

The chord system/UI decides how the wheel responds.

## Editor Touch Simulation

During development in the Unity Editor, mouse input should be capable of simulating relevant touch interactions.

This allows guitar interaction to be tested without requiring a mobile device for every iteration.

Editor simulation should:

- Reproduce the relevant interaction semantics.
- Allow testing taps.
- Allow testing dragging/swiping.
- Allow testing chord-wheel interaction.

Do not make editor simulation part of the mobile runtime architecture unnecessarily.

## PC Build Controls

PC builds have dedicated development/playtesting bindings.

- Keyboard keys may be assigned to individual guitar strings.
- Mouse controls the chord wheel.
- PC input should ultimately produce the same gameplay-level intent/events as mobile input.

Conceptually:

```text
Mobile Touch ──────┐
                   ├──> Input Intent/Event ──> Gameplay
PC Keyboard/Mouse ─┘
```

Gameplay systems should not need separate implementations for mobile and PC input.

## Input Abstraction

Input sources should be replaceable.

The guitar gameplay system should not care whether an interaction originated from:

- Touch.
- Mouse.
- Keyboard.
- Another supported input device.

It should receive meaningful gameplay input.

This keeps the core guitar mechanics platform-independent.

## Touch Ownership

Input areas have strict, isolated ownership matching the primordial vision:

```text
Bottom-Right (Strings Rectangle)
    → Guitar String Input: 6 strings from bottom to top (Low E to High e),
      tap for pluck, slide across several strings for chord strum, wobble feedback

Upper-Right (Circle of Chords)
    → Chord selection: 3 major on top, 3 minor below relative to key;
      selected chord glows and scales up

Bottom-Left (Thumbstick)
    → Shape Selector: Analog stick cycling the 6 chord shapes on the selected scale

Middle-Left (Key Stepper)
    → Two buttons (< / >) cycling between scales and chord shapes; tuning preset selector

Middle-Top (Capo Stepper)
    → Two buttons (- / +) incrementing/decrementing capo position
```

Avoid allowing unrelated UI elements and gameplay areas to respond to the same touch unintentionally.
When touch interactions overlap, establish explicit priority/ownership rules rather than relying on accidental event ordering.

## Gesture Complexity

Do not introduce complex gesture recognition unless the gameplay actually requires it.

Prioritize:

1. Reliable string interaction.
2. Low latency.
3. Multi-touch.
4. Accurate movement tracking.
5. Clear gameplay events.

More advanced gesture interpretation should only be added when a concrete gameplay requirement exists.

## Performance

Input processing must be suitable for mobile.

- Avoid unnecessary allocations during touch movement.
- Avoid expensive searches on every pointer update.
- Avoid repeatedly querying the scene hierarchy.
- Avoid unnecessary physics calculations.
- Keep callbacks lightweight.
- Cache references where appropriate.
- Avoid excessive logging during continuous touch movement.
- Do not sacrifice responsiveness for unnecessary abstraction.

Continuous finger movement can generate many input events, so hot-path input code must remain lightweight.

## Debugging

Input debugging should identify problems without creating log spam.

Useful diagnostics include:

- Invalid input configuration.
- Missing action references.
- Unexpected input state.
- Invalid string/chord-wheel references.

Do not log every touch movement or frame during normal operation.

When debugging gestures, temporary diagnostics may visualize or report the interaction path and detected targets.

## Core Rule

**Input detects and reports player intent; gameplay decides what that intent does.**

Use Unity's new Input System, support multi-touch, keep interactions responsive, and ensure mobile and PC input ultimately feed the same gameplay systems.
