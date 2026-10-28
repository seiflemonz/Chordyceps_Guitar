---
name: mobile-performance
description: >-
  Performance and optimization guidelines for Chordyceps on mobile devices, covering frame rate, memory allocation, CPU/GPU budgets, and profiling. Use when optimizing or profiling mobile performance.
---

# Mobile Performance Skill

## Purpose

Define performance practices for Chordyceps as a mobile-first Unity game, prioritizing stable frame rate, responsive input, controlled memory usage, and efficient CPU/GPU usage without premature optimization.

## Core Principle

**Build for mobile from the beginning, but optimize based on real problems rather than assumptions.**

Performance work should protect the player's experience:

- Stable frame rate.
- Responsive input.
- Fast interactions.
- Reasonable memory usage.
- Predictable battery/thermal behavior.
- Fast loading where practical.

Do not sacrifice clean architecture for theoretical micro-optimizations.

## Target Platform

The primary runtime target is mobile.

When making implementation decisions:

1. Consider mobile performance.
2. Prefer predictable resource usage.
3. Avoid unnecessary work.
4. Preserve maintainability.
5. Profile when performance is uncertain.

The development PC should not be treated as representative of the final device.

## CPU

Avoid unnecessary CPU work, especially work that runs every frame.

Be cautious with:

- Large `Update` loops.
- Repeated scene searches.
- Repeated `GetComponent` calls in hot paths.
- Unnecessary polling.
- Repeated allocations.
- Expensive calculations triggered by frequent input events.
- Excessive event processing.
- Unnecessary animation or gameplay processing.

Prefer:

- Cached references.
- Event-driven behavior.
- Work performed only when state changes.
- Small focused systems.
- Reusable objects where appropriate.

Do not optimize a system that does not have a measurable or credible performance concern.

## Garbage Collection

Minimize avoidable allocations during frequently executed gameplay.

Pay particular attention to:

- Per-frame code.
- Touch/mouse movement.
- Guitar string playback.
- Strumming.
- Song playback/progression.
- UI scrolling.
- Repeated gameplay events.

Avoid creating temporary objects unnecessarily in hot paths.

Do not pursue zero-allocation code everywhere. Some allocations are acceptable when they occur infrequently and do not create meaningful pressure.

## Audio Performance

Guitar audio is a core gameplay system and must remain responsive.

Follow the `guitar-audio` skill for detailed architecture.

Performance priorities include:

- Reusing AudioSources through a voice pool.
- Avoiding runtime AudioSource creation/destruction during normal gameplay.
- Keeping the voice pool reasonably sized.
- Avoiding unnecessary audio processing.
- Avoiding unnecessary effects on acoustic guitar audio.
- Controlling the CPU cost of electric guitar effects.
- Using appropriate audio import/compression settings.

The audio system must still support natural overlapping guitar sounds.

Do not reduce the voice pool to an unrealistically small number solely to save resources.

## Input Performance

Input responsiveness is critical.

Follow the `mobile-input` skill for input architecture.

Avoid:

- Expensive work during touch callbacks.
- Scene searches during input.
- Unnecessary allocations during pointer movement.
- Artificial input delays.
- Complex gesture processing that is not required.

Input should detect interaction quickly and pass lightweight intent to gameplay.

## GPU

Mobile GPU performance should be considered when building the 3D presentation.

Pay attention to:

- Excessive overdraw.
- Expensive shaders.
- Too many transparent UI elements.
- Excessive real-time lighting.
- Expensive shadows.
- High-poly assets that provide little visible benefit.
- Unnecessary post-processing.
- Excessive particle effects.

The game does not need AAA rendering quality to look good.

Prioritize the visual elements that actually contribute to the Chordyceps experience.

## UI Performance

Chordyceps contains substantial 2D UI.

Avoid unnecessarily rebuilding large portions of the UI when only a small element changed.

For frequently updated UI:

- Update only what changed.
- Avoid unnecessary layout rebuilds.
- Avoid excessive hierarchy complexity.
- Avoid constantly recreating UI elements.
- Reuse UI elements when appropriate.

Do not optimize UI prematurely before the actual UI architecture exists.

## 3D Performance

The 3D character/guitar presentation should be designed with mobile constraints in mind.

Consider:

- Polygon count.
- Material count.
- Texture memory.
- Lighting complexity.
- Shadow cost.
- Animation cost.
- Number of visible objects.
- Transparency.

Prefer simple assets that maintain the intended visual style.

Avoid expensive rendering techniques unless they provide meaningful visual value.

## Object Pooling

Use pooling when objects are:

- Created frequently.
- Destroyed frequently.
- Short-lived.
- Performance-sensitive.

Good candidates may include:

- Audio voices.
- Repeated gameplay effects.
- Certain UI elements.
- Particles or visual effects.

Do not create an object pool for every object in the game.

Pooling adds complexity and should be justified by the object's lifecycle.

## Asset Memory

Mobile memory is limited.

Pay attention to:

- Texture resolution.
- Texture compression.
- Audio compression.
- Audio clip length.
- Mesh complexity.
- Unnecessary duplicate assets.
- Large assets loaded earlier than necessary.

Avoid keeping large assets resident when they are not needed.

Use appropriate Unity asset-loading mechanisms when controlled loading becomes necessary.

Do not build a complex streaming/loading architecture before the project requires it.

## Loading

Loading time should be considered during development.

Avoid:

- Loading unnecessary content at startup.
- Performing large synchronous operations during gameplay.
- Creating large numbers of objects during a single frame when avoidable.

When content becomes large enough to require it, consider asynchronous loading and appropriate asset management.

Keep the initial loading path simple while the project is small.

## Scenes

Keep scenes focused.

Avoid creating unnecessarily huge scenes containing every system and asset in the game.

Use scene boundaries that make sense for the game's actual content.

Do not split scenes solely to follow a theoretical architecture.

## Physics

Physics should only be used where gameplay requires it.

Avoid:

- Excessive physics objects.
- Unnecessary rigidbodies.
- Unnecessary collision checks.
- Physics simulation for interactions that can be represented more simply.

For the guitar's touch interaction, prefer efficient screen/input-space detection where appropriate rather than introducing unnecessary physics simulation.

## Animation

Animation should be appropriate for the mobile target.

Consider:

- Number of animated objects.
- Animation complexity.
- Update frequency.
- Rig complexity.
- Unnecessary Animator components.

Do not optimize animation quality away before profiling shows that animation is a problem.

## Rendering and Resolution

Design for a range of mobile devices.

Do not assume that every device has the same:

- Resolution.
- GPU.
- Refresh rate.
- Performance budget.

Use scalable rendering settings where appropriate.

Visual quality settings should have sensible defaults for the target devices.

## Profiling

When a performance problem is suspected:

1. Reproduce the problem.
2. Identify whether it is CPU, GPU, memory, loading, audio, or another bottleneck.
3. Measure before changing architecture.
4. Make the smallest meaningful optimization.
5. Measure again.
6. Confirm that the optimization actually helped.

Use Unity's profiling tools when appropriate.

Do not claim a performance improvement without measurement when the change is intended as a measurable optimization.

## Performance Budgets

Use practical budgets rather than arbitrary universal numbers.

When a system becomes performance-sensitive, establish an appropriate budget based on:

- Target devices.
- Expected frame rate.
- Number of simultaneous objects.
- Number of simultaneous audio voices.
- Memory requirements.
- Actual profiling results.

The correct budget may change as the game develops.

## Development vs Release

Development conveniences are acceptable when they do not affect the final build.

Examples:

- Editor-only debugging.
- Development visualization.
- Additional logging.
- Debug controls.
- Editor input simulation.

Ensure expensive development-only behavior is excluded or disabled in release builds where appropriate.

## Profiling Before Refactoring

Do not redesign architecture solely because something "might be slow."

Use this priority:

```text
Suspected problem
      ↓
Reproduce
      ↓
Measure
      ↓
Identify bottleneck
      ↓
Smallest effective optimization
      ↓
Measure again
```

If profiling demonstrates that the architecture itself is responsible for the problem, then an architectural change may be justified.

## Mobile Performance Review

When reviewing a system, consider:

### CPU

- Per-frame work.
- Hot loops.
- Allocations.
- Searches.
- Event frequency.

### GPU

- Draw calls.
- Overdraw.
- Shaders.
- Lighting.
- Shadows.
- Transparency.

### Memory

- Textures.
- Audio.
- Meshes.
- Runtime allocations.
- Loaded assets.

### Responsiveness

- Input latency.
- Audio triggering.
- UI response.
- Frame spikes.

### Battery/Thermals

- Continuous CPU work.
- GPU workload.
- Audio processing.
- Unnecessary background activity.

Only report issues that are relevant to the system being reviewed.

## Optimization Priority

When a real performance problem exists, generally prioritize:

1. Remove unnecessary work.
2. Reduce expensive work frequency.
3. Reuse resources where appropriate.
4. Reduce memory usage.
5. Optimize algorithms/data access.
6. Optimize rendering/audio.
7. Apply lower-level micro-optimizations only when profiling justifies them.

Do not jump directly to micro-optimization.

## Core Rule

**Chordyceps should be mobile-first, not optimization-first. Build clean systems, avoid obvious waste, measure real bottlenecks, and optimize only where the player experience or device limitations justify it.**
