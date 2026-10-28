---
name: solid-principles
description: >-
  Pragmatic application of SOLID principles to Chordyceps architecture. Use when designing class structures, managing dependencies, or refactoring complex systems.
---

# SOLID Principles Skill

## Purpose

Apply SOLID principles pragmatically to Chordyceps. These principles exist to
prevent mixed responsibilities, brittle dependencies, and hard-to-maintain code.
Apply them when they solve a real problem. Do not introduce abstractions merely
to demonstrate a principle.

---

## When to Apply SOLID

Apply a principle when you encounter one of these real problems:

- A class changes for multiple unrelated reasons.
- Adding a feature requires editing many existing classes.
- One system must know too many details about another.
- Replacing one implementation breaks unrelated code.
- A class interface forces consumers to depend on methods they do not use.
- High-level logic is tightly coupled to low-level detail.

Do not apply SOLID speculatively. Keep the architecture proportional to the
project's actual size and complexity.

---

## S — Single Responsibility Principle

**A class should have one primary reason to change.**

### In Chordyceps

| Class | Its One Responsibility |
|---|---|
| Input component | Detect gestures; emit intent events |
| Chord system | Manage musical selection state |
| Guitar audio system | Select and play the correct string audio |
| Song system | Manage song data, progression, and playback state |
| UI component | Present state and forward player intent |
| Persistence system | Save and restore settings/preferences |

### Warning Signs

- A MonoBehaviour that handles input **and** plays audio **and** manages the chord.
- A "manager" class that owns input, chords, UI, songs, and audio together.
- A class that changes every time any feature is added.

### Rule

> If you can describe a class's responsibility using the word "and," consider whether
> the responsibilities should be separated.

---

## O — Open/Closed Principle

**Entities should be open for extension and closed for modification.**

### In Chordyceps

- Adding a new song should require authoring a new data asset, not editing the
  core song playback logic.
- Adding a chord shape should use the existing data configuration path where possible.
- Adding a new audio effect should fit into an existing audio-processing boundary
  rather than requiring unrelated gameplay code to change.

### Practical Application

Before adding a new feature, ask:

> Can I add this by extending or authoring data, rather than modifying existing code?

If existing code must change every time a new song, chord, or effect is added,
the design is closed to the wrong things.

### Warning Signs

- Every new song requires editing a hard-coded array in a script.
- Adding a chord shape requires adding a new `if/else` branch in core logic.
- Adding an audio effect requires modifying the guitar input handler.

### Rule

> Data-driven content (ScriptableObjects) is the primary open/closed mechanism
> in Chordyceps. Use it where practical.

---

## L — Liskov Substitution Principle

**A subtype must be usable wherever its base type is expected, without breaking behavior.**

### In Chordyceps

- If multiple audio playback implementations share an interface (e.g., `IGuitarAudioPlayer`),
  each must honor the same playback and voice-management contract.
- If song sources share a common interface, every implementation must provide
  valid song data and consistent playback behavior.

### Practical Application

Before using inheritance or interfaces, verify that every concrete type genuinely
satisfies the full contract of the base type.

### Warning Signs

- A subclass that throws `NotImplementedException` on a required method.
- An interface implementation that silently ignores a required behavior.
- A derived class that only works correctly when cast back to its concrete type.

### Rule

> Favor composition over inheritance when it produces simpler behavior.
> Do not create inheritance hierarchies without a real need.

---

## I — Interface Segregation Principle

**Clients should not be forced to depend on methods they do not use.**

### In Chordyceps

- A UI element that only needs to request a chord change should not depend on a
  large interface that also exposes audio pooling, song loading, and persistence.
- A system that only reads the current key should not receive access to the entire
  chord system's mutation API.

### Practical Application

When exposing a system's API, ask:

> What does this specific consumer actually need?

Expose the smallest interface that satisfies the requirement.

### Warning Signs

- Consumers implementing empty methods to satisfy a large interface.
- A single massive interface used by every system in the project.
- A system knowing about unrelated capabilities it never uses.

### Rule

> Keep interfaces small and meaningful. Do not create interfaces for every class
> by default — only when multiple implementations are needed or when the interface
> provides a useful narrow contract for a specific consumer.

---

## D — Dependency Inversion Principle

**High-level policy should not depend directly on low-level implementation details.
Both should depend on suitable abstractions when that reduces coupling.**

### In Chordyceps

- Gameplay should request guitar-note playback through a focused contract
  (`IGuitarAudioPlayer`) rather than knowing how audio voices are allocated internally.
- Song playback should consume song data through a stable boundary rather than
  depending on UI widgets.
- Analytics and monetization should be replaceable integrations, not requirements
  for the guitar to function.

### Dependency Wiring

Use the wiring approach appropriate to the existing architecture:

- **Serialized references** (`[SerializeField]`) for MonoBehaviour-to-MonoBehaviour
  dependencies wired in the Inspector.
- **Constructor injection** for pure C# objects that need explicit dependencies.
- **Event-based decoupling** when a system only needs to react to state changes
  from another system.

Do not add a dependency-injection framework unless project complexity genuinely
justifies it.

### Warning Signs

- A high-level gameplay class directly creating audio voice objects.
- Song playback code that calls UI methods directly.
- Analytics code embedded inside the guitar string interaction handler.

### Rule

> If replacing one system requires changing another unrelated system, the dependency
> boundary is too tight. Introduce a clear contract or event boundary between them.

---

## SOLID in Practice: Decision Guide

Use this guide before introducing a new class or interface.

```text
Is this class changing for more than one reason?
    Yes -> Apply SRP: split the responsibilities.

Can I add this feature without modifying existing classes?
    No  -> Apply OCP: consider data-driven extension.

Does this subtype honor its base type's full contract?
    No  -> Prefer composition over inheritance.

Does this consumer need all methods of this interface?
    No  -> Apply ISP: split into a narrower interface.

Does this high-level system know too much about the low-level detail?
    Yes -> Apply DIP: introduce a boundary/contract.
```

---

## What SOLID Is Not

- SOLID is **not** a mandate to create an interface for every class.
- SOLID is **not** a reason to build elaborate extension frameworks speculatively.
- SOLID is **not** a replacement for simple, readable code.
- SOLID violations are **not** always bugs — sometimes a simpler solution is correct.

The goal is maintainable, understandable code that can grow without fragility.
Apply SOLID proportionally.

---

## Core Rule

**Apply SOLID when it solves a real problem: mixed responsibilities, repeated changes,
fragile dependencies, or hard-to-test behavior. Keep the architecture proportional to
the project's actual size. Do not introduce abstractions for hypothetical future needs.**
