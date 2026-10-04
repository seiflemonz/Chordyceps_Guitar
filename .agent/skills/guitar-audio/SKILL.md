# Guitar Audio Skill

## Purpose

Define the guitar audio architecture for Chordyceps, prioritizing natural sample playback, simultaneous/repeated string sounds, reusable audio voices, mobile performance, and extensibility for electric guitar effects.

## Core Audio Philosophy

- Guitar audio should sound natural and responsive.
- Every played string is an independent audio event.
- A chord may trigger multiple strings simultaneously.
- Repeated plays of the same string must be able to overlap naturally.
- Do not use one permanent AudioSource per guitar string.
- Use a reusable audio voice pool capable of handling multiple simultaneous sounds.

## Audio Voice Pool

The core playback system should use pooled audio voices.

A voice represents a reusable `AudioSource` capable of playing one guitar-string sound.

Requirements:

- Voices are reused rather than continuously instantiated.
- Multiple voices may play the same string sample simultaneously.
- A string can be played again before its previous sound finishes.
- The pool should support simultaneous chord playback.
- Avoid creating and destroying AudioSources during normal gameplay.
- The pool size should be configurable.
- The system should behave predictably when all voices are occupied.

### Voice Stealing

When no free voice is available:

- Reuse the **oldest active voice first**.
- Stop the oldest sound.
- Assign the voice to the new audio event.
- Start the new sound immediately.

Do not silently ignore a requested guitar note simply because the pool is temporarily full.

## String Playback

Treat every string play as an independent audio event.

For example, playing a six-string chord should conceptually produce:

```text
Low E → audio event
A     → audio event
D     → audio event
G     → audio event
B     → audio event
High E → audio event
```

Each event can acquire its own voice from the pool.

This allows:

- Chords.
- Arpeggios.
- Rapid strumming.
- Repeated notes.
- Overlapping notes.
- Multiple plays of the same string.

The gameplay system should determine **what string was played**. The audio system should determine **how that string sound is played**.

## Audio Data

Reusable guitar audio configuration should use ScriptableObjects where appropriate.

Possible data includes:

- Guitar string definitions.
- String audio clips.
- Guitar tuning/configuration.
- Audio playback configuration.
- Electric guitar effect presets.

Avoid hard-coding audio clip references into gameplay scripts.

Audio assets should be referenced through data/configuration rather than scattered throughout gameplay code.

## Pitch

Do **not** use Unity's `AudioSource.pitch` for guitar playback.

Chordyceps should not dynamically pitch-shift guitar samples because Unity's normal pitch modification changes playback speed as well.

Therefore:

- Do not implement dynamic pitch shifting in the core guitar audio system.
- Do not use pitch manipulation to simulate guitar notes.
- Use appropriately recorded samples for the required guitar pitches.
- Preserve the natural timing and pitch of the recorded guitar samples.

If true independent pitch shifting is required in the future, it should be treated as a separate audio technology decision rather than quietly added to the core system.

## Audio Effects

Effects are not part of the core acoustic guitar system.

Electric guitar mode may later support effects such as:

- Gain.
- Distortion.
- EQ.
- Reverb.
- Wobble/modulation.
- Other appropriate guitar processing.

Effects should be:

- Optional.
- Modular.
- Disabled when unnecessary.
- Configurable through reusable data where appropriate.
- Designed so acoustic guitar playback does not depend on the electric guitar system.

Do not build the complete electric effects system before it is required.

## Audio Routing

Keep audio responsibilities separated.

A typical flow should be:

```text
Player Input
     ↓
Guitar Gameplay
     ↓
String/Chord Audio Request
     ↓
Guitar Audio System
     ↓
Voice Pool
     ↓
AudioSource
     ↓
Optional Electric Effects
     ↓
Output
```

The exact class names and architecture may differ based on the existing project.

Do not force this structure if a simpler existing implementation already provides the same separation safely.

## Performance

Mobile performance is a priority.

- Avoid runtime creation/destruction of AudioSources during normal gameplay.
- Reuse audio voices.
- Avoid unnecessary allocations during frequent string playback.
- Avoid expensive searches when requesting an audio voice.
- Cache references used frequently.
- Avoid unnecessary per-frame audio processing.
- Keep the pool reasonably sized rather than creating an excessive number of voices.
- Prefer predictable resource usage over uncontrolled dynamic allocation.
- Do not sacrifice necessary simultaneous playback simply for an arbitrary low voice count.

Performance should be measured against the actual gameplay requirements rather than optimized prematurely.

## Pool Configuration

The voice pool should be configurable.

Configuration should account for:

- Maximum simultaneous voices.
- Voice behavior when the pool is full.
- Audio routing.
- Optional effect processing.
- Other playback settings that are genuinely reusable.

Do not bury important tuning values as unexplained constants inside gameplay code.

## Audio Lifecycle

A voice should have a clear lifecycle:

```text
Available
   ↓
Assigned
   ↓
Playing
   ↓
Finished
   ↓
Available
```

When a voice finishes naturally, it should become available for reuse.

When voice stealing occurs:

```text
Playing
   ↓
Stopped
   ↓
Reassigned
   ↓
Playing
```

The system should not leave stale state attached to a reused voice.

## Audio Clip Management

- Audio clips should be imported with mobile usage in mind.
- Avoid unnecessarily large audio assets.
- Use appropriate compression and import settings for the intended platform.
- Do not blindly apply the same import settings to every audio asset.
- Short frequently played guitar samples should be configured appropriately for repeated playback.
- Long music/audio assets should be treated differently from short instrument samples.

The final import settings should be based on actual project requirements and tested on target hardware where possible.

## Audio System Boundaries

The guitar audio system should handle:

- Finding/assigning a playback voice.
- Playing a requested string sample.
- Managing simultaneous sounds.
- Managing voice reuse.
- Managing voice stealing.
- Routing audio.
- Optional electric effects.

It should not own:

- Touch input.
- Strumming detection.
- Chord selection UI.
- Song progression.
- Chord wheel behavior.
- UI presentation.
- Gameplay decisions about which strings should be played.

Those systems should request audio playback rather than directly manipulating AudioSources.

## Debugging

Audio debugging should be useful without flooding the Unity Console.

Useful diagnostics may include:

- Pool initialization failures.
- Missing audio clips.
- Missing required audio references.
- Invalid configuration.
- Unexpected pool exhaustion.
- Invalid string identifiers.

Do not log every individual string play during normal gameplay.

Temporary playback diagnostics may be added during development and removed when no longer useful.

## Mobile Considerations

The system must be designed for mobile from the beginning.

Pay particular attention to:

- Number of simultaneous voices.
- Audio memory usage.
- Clip import/compression settings.
- Runtime allocations.
- Effect processing cost.
- CPU usage from electric guitar effects.
- Avoiding unnecessary audio processing when effects are disabled.

If a proposed feature could significantly increase mobile audio cost, report the tradeoff before implementing it.

## Architecture Rule

Keep gameplay and audio responsibilities separate:

> **Gameplay decides what was played. Audio decides how it sounds.**

Use reusable data, pooled voices, explicit dependencies, and predictable resource usage. Keep the core system focused on natural guitar playback; electric effects should remain an optional extension.
