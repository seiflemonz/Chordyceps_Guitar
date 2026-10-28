---
name: scriptable-objects
description: >-
  ScriptableObject architecture and best practices for Chordyceps, separating reusable static data from runtime state. Use when creating or modifying ScriptableObjects and data assets.
---

# Scriptable Objects Skill

## Purpose

Define safe and maintainable ScriptableObject usage in Chordyceps, separating reusable project data from runtime state while supporting clean architecture, Inspector workflows, and mobile-friendly asset management.

## Core Principle

ScriptableObjects are primarily **data assets**, not gameplay objects.

Use them when data needs to be:

- Reused by multiple objects.
- Configured in the Unity Editor.
- Shared across systems.
- Stored as project assets.
- Referenced without duplicating configuration.

Do not use ScriptableObjects simply because they are available.

## Appropriate Uses

ScriptableObjects are appropriate for reusable Chordyceps data such as:

- Chord definitions.
- Guitar string definitions.
- Guitar configurations.
- Song data.
- Song metadata.
- Audio configurations.
- Electric guitar effect presets.
- Gameplay configuration.
- Other reusable designer-facing data.

Example conceptual structure:

```text
ChordData
    ├── Chord name
    ├── Key
    ├── Chord type
    └── String/fingering information

SongData
    ├── Song metadata
    ├── Chord progression
    └── Song timing/data

GuitarConfiguration
    ├── String configuration
    ├── Tuning
    └── Playback configuration
```

The exact asset structure should evolve with the actual requirements rather than being created speculatively.

## Runtime State

Do not store mutable gameplay state directly in ScriptableObject assets by default.

Examples of runtime state that should generally remain outside the asset:

- Currently selected chord.
- Current capo position.
- Current song position.
- Current playback state.
- Current player interaction.
- Temporary gameplay values.
- Session-specific settings.

The general architecture should be:

```text
ScriptableObject
    ↓
Configuration / Shared Data
    ↓
Runtime System
    ↓
Runtime State
```

This prevents one runtime instance from accidentally modifying shared project data.

## Runtime Mutation

ScriptableObject assets should be treated as effectively read-only during normal gameplay.

Do not modify serialized asset fields at runtime unless there is a deliberate architectural reason.

Be particularly careful because a ScriptableObject is a shared asset. Multiple systems can reference the same instance.

For example, this is dangerous:

```csharp
chordData.IsSelected = true;
```

if `IsSelected` represents runtime state.

Prefer:

```text
ChordData
    → immutable chord information

ChordSystem
    → currently selected chord
```

## Runtime Data Copies

Do not automatically copy every ScriptableObject into another runtime object.

Reading shared configuration directly is often appropriate when:

- The data is immutable during gameplay.
- The data is lightweight.
- Multiple systems benefit from the same source of truth.
- No runtime transformation is required.

Create runtime representations when:

- Data must change during gameplay.
- Runtime state is substantially different from asset data.
- Performance requires a specialized representation.
- The asset format is intended for authoring rather than runtime use.
- Multiple runtime instances need independent state.

Choose based on the actual system rather than following either approach universally.

## Asset Granularity

Prefer focused ScriptableObjects.

Good:

```text
ChordData
SongData
GuitarConfiguration
AudioEffectPreset
```

Avoid creating a single enormous asset such as:

```text
EverythingGameData
```

that contains unrelated systems and configuration.

Focused assets:

- Reduce coupling.
- Improve Inspector usability.
- Make assets reusable.
- Make dependencies easier to understand.
- Reduce unnecessary data changes.

However, do not split every field into its own ScriptableObject. A configuration asset should contain data that logically belongs together.

## References Between Assets

ScriptableObjects may reference other ScriptableObjects when the relationship represents real shared data.

For example:

```text
SongData
    ↓
ChordData
    ↓
Audio/String Data
```

Use references when they improve reuse and maintainability.

Avoid unnecessary chains of dependencies that make assets difficult to understand or load.

Before introducing asset references, consider:

- Whether the referenced data is genuinely shared.
- Whether the dependency is necessary.
- Whether it creates an undesirable architectural coupling.
- Whether it has mobile memory/loading implications.

## Asset References and Mobile

ScriptableObjects themselves are lightweight data containers, but their referenced assets can have significant memory implications.

Do not assume that using a ScriptableObject automatically makes asset loading free.

Consider:

- Audio clips referenced by data.
- Textures.
- Prefabs.
- Other large assets.
- Asset dependency chains.
- When assets are loaded and released.

Large assets should not be unnecessarily referenced by always-loaded configuration assets if doing so would cause unwanted memory residency.

Use Unity's appropriate asset-loading systems when the project requires controlled loading.

Do not introduce an advanced asset-loading architecture prematurely.

## Editor Workflow

ScriptableObjects should be easy to create and configure in the Unity Editor.

Use:

```csharp
[CreateAssetMenu]
```

when a data type is intended to be created as a project asset.

Inspector-facing data should have:

- Clear names.
- Logical grouping.
- Appropriate serialized fields.
- Sensible defaults.
- Clear validation where necessary.

Do not build custom inspectors or editor tooling unless the default Inspector becomes insufficient or the tooling provides meaningful productivity/safety benefits.

## Validation

Validate ScriptableObject configuration when invalid data could cause difficult runtime problems.

Validation may detect:

- Missing required references.
- Invalid values.
- Duplicate identifiers.
- Invalid chord/string configurations.
- Impossible combinations of settings.

Prefer early, understandable errors over mysterious runtime failures.

Do not add validation for every field simply for completeness.

## IDs and Identity

When data needs to be referenced reliably, use an appropriate stable identifier.

Do not rely on:

- Display names.
- Array positions.
- Object names alone.

For example, a song's display title can change without necessarily changing its underlying identity.

Use stable identifiers when the system genuinely requires them.

Do not introduce IDs everywhere without a concrete requirement.

## Serialization

Use Unity serialization intentionally.

Prefer:

```csharp
[SerializeField] private ChordData chord;
```

over exposing implementation fields publicly.

Expose read access through properties or methods when other runtime systems need access.

Keep asset data clearly separated from implementation details.

## ScriptableObjects and Architecture

ScriptableObjects should support architecture, not replace it.

Do not use ScriptableObjects as a substitute for:

- Dependency injection.
- Event systems.
- Runtime state management.
- Service locators.
- Global mutable state.
- Communication between unrelated systems.

A ScriptableObject can provide configuration to a system without becoming responsible for the system's behavior.

## Shared State Warning

Remember that ScriptableObjects are reference types.

If two systems reference the same asset:

```text
System A ──┐
           ├──> SharedData.asset
System B ──┘
```

they are accessing the same object.

Never assume that changing the asset through one reference creates an independent copy.

This is especially important for runtime modifications.

## Collections

Use serialized collections when they make authoring and data organization clear.

For example:

```text
SongData
    └── Chord Events
        ├── C
        ├── Am
        ├── G
        └── F
```

Do not create deeply nested or overly complicated serialized structures unless the gameplay system genuinely requires them.

Keep data formats understandable to both the developer and the systems consuming them.

## Chordyceps Data Direction

The project should generally follow:

```text
                    ScriptableObjects
                          │
             ┌────────────┼────────────┐
             ↓            ↓            ↓
         Chord Data    Song Data   Guitar Data
             │            │            │
             └────────────┼────────────┘
                          ↓
                    Runtime Systems
                          ↓
                     Runtime State
```

The exact architecture may change as the project develops.

Do not build the entire game's data architecture before the systems that consume it exist.

## Performance

ScriptableObjects should not be used as a performance optimization by default.

Performance decisions should consider:

- Asset loading.
- Memory residency.
- Reference dependencies.
- Runtime allocations.
- Data access frequency.

For frequently accessed immutable data, direct ScriptableObject references are often perfectly reasonable.

If profiling demonstrates a problem, optimize the actual bottleneck rather than redesigning the entire data architecture speculatively.

## Core Rule

**Use ScriptableObjects for reusable, editor-configurable data. Keep mutable gameplay state in runtime systems. Prefer focused assets, explicit dependencies, safe references, and simple data structures.**
