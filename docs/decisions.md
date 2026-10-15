# Chordyceps — Architectural Decisions

> **Purpose:** Record significant design and architectural decisions, the
> reasoning behind them, and any alternatives that were considered and rejected.
> This prevents settled questions from being re-litigated during development.
>
> Add a new entry whenever a non-obvious decision is made. Keep entries concise.
> Reference the relevant task, date, or Git commit where helpful.

---

## Decision Log

### ADR-001 — Voice Pool Over Per-String AudioSources

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Use a shared audio voice pool (target 18 voices) instead of assigning a fixed
number of dedicated AudioSources per guitar string.

**Reasoning:**
- Guitar playing requires overlapping notes, rapid repeated taps, and chord
  strums triggering multiple strings simultaneously.
- A fixed per-string model limits polyphony and wastes AudioSources when strings
  are not playing.
- A shared pool with voice stealing handles all legitimate playback scenarios
  with predictable, configurable resource usage.

**Voice stealing policy:** Oldest active voice is reclaimed first when the pool
is exhausted. This prevents silent failures on rapid input.

**Alternatives considered:**
- Three AudioSources per string (18 fixed) — rejected because the pool is more
  flexible and avoids wasted sources when fewer than all strings play.
- Unlimited dynamic creation — rejected due to uncontrolled memory/CPU cost on
  mobile.

**Skill:** `guitar-audio`

---

### ADR-002 — No AudioSource.pitch for Guitar Notes

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Do not use `AudioSource.pitch` to transpose guitar samples to different notes.
Use correctly recorded samples for each required pitch.

**Reasoning:**
- Unity's pitch modification changes playback speed as well as pitch, producing
  an unnatural sound for guitar samples.
- Pitch-shifted samples sound noticeably wrong at intervals greater than a
  semitone or two.
- Using real samples per note preserves natural timing and tone.

**Implication:** Audio assets must cover every required note across all supported
keys and capo positions. This is a content authoring constraint.

**Alternatives considered:**
- Runtime pitch shifting — rejected due to audio quality degradation.
- True independent pitch shifting (DSP) — deferred; adds significant complexity
  and is out of scope for v1.0.

**Skill:** `guitar-audio`

---

### ADR-003 — UI Toolkit for 2D Screens, GameObjects for 3D

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Use UI Toolkit for all primarily 2D interface screens. Use Unity scene
GameObjects for the 3D character, guitar, room, and world-space presentation.
Retain TextMeshPro where it is already appropriate or for world-space text.

**Reasoning:**
- UI Toolkit is the modern, supported Unity UI system for 2D interfaces.
- GameObjects and the scene hierarchy remain the correct approach for 3D content.
- Mixing both technologies by purpose (not by force) avoids unnecessary migration
  work and keeps each technology in its strongest domain.

**Alternatives considered:**
- UGUI for all UI — rejected in favour of the modern supported path.
- UI Toolkit for world-space — rejected; not designed for world-space use.

**Skill:** `ui-toolkit`

---

### ADR-004 — ScriptableObjects for Authored Content Data

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Use ScriptableObjects for chord definitions, key/chord mappings, chord shapes,
song metadata, song progression data, audio configurations, and other
designer-facing content. Do not hard-code these in scripts.

**Reasoning:**
- Adding a new song or chord shape requires authoring data, not editing code
  (Open/Closed Principle).
- ScriptableObjects are inspectable, shareable, and reusable across scenes.
- Keeps mutable runtime state separate from authored, shared configuration.

**Constraint:** ScriptableObject assets must be treated as read-only during
gameplay. Runtime state (selected chord, capo position, playback position)
lives in runtime MonoBehaviour components, not in assets.

**Skill:** `scriptable-objects`, `chord-system`, `song-system`

---

### ADR-005 — New Input System Only

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Use Unity's new Input System exclusively. Do not use the legacy Input Manager
for any new code.

**Reasoning:**
- New Input System supports multi-touch natively.
- Provides clean action-based abstraction so mobile touch and editor
  mouse/keyboard bindings feed the same gameplay events.
- Legacy Input Manager has poor multi-touch support and is not the strategic path.

**Input file:** `Assets/_Project/InputSystem_Actions.inputactions` (already present).

**Skill:** `mobile-input`

---

### ADR-006 — DSP Buffer Size: Best Latency

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
Set Project Settings > Audio > DSP Buffer Size to **Best Latency**.

**Reasoning:**
Touch-to-sound latency is a first-class quality requirement for a guitar
simulator. The default DSP buffer size introduces perceptible delay on Android
that degrades the playing feel.

**Trade-off:** Best Latency uses more CPU for audio processing. This must be
validated against the performance budget on target devices.

**Workflow:** `android-build`

---

### ADR-007 — Input Layer Emits Intent; Gameplay Decides Response

**Date:** 2026-10-15
**Status:** Accepted

**Decision:**
The input layer detects and reports player intent (string touched, chord wheel
dragged, key change requested). The gameplay layer interprets what that intent
means in the context of current game state and directs the appropriate systems.

**Reasoning:**
- Separates the *what did the player do* question from the *what should the game
  do* question (Single Responsibility Principle).
- Allows the same gameplay logic to be driven by touch, keyboard, or any other
  input source without duplication.
- Keeps the input layer lightweight and testable in isolation.

### ADR-008 — UI Layout & Screen Space Partitioning from Practice Interface Reference

**Date:** 2026-10-15  
**Status:** Accepted  

**Decision:**
The primary play screen layout partitions the screen into 6 key regions matching `Cozy Guitar Practice Interface.png`:
1. **Center:** 3D perspective camera focused on the character playing guitar in the room.
2. **Bottom-Right:** 6-string horizontal touch/strum area with vertical swipe indicator.
3. **Upper-Right:** Diatonic chord wheel (I, IV, V, ii, vi, iii).
4. **Top-Center:** Capo stepper (`-`, fret value, `+`).
5. **Upper/Middle-Left:** Key selector (`< Key >`) & tuning selector.
6. **Bottom-Left:** Chord shape analog stick / dial (1..6).

**Reasoning:**
- Positions touch controls in standard thumb reach zones (left thumb for chord shape/key, right thumb for strings/chord selection).
- Leaves the center screen unoccluded for 3D character, guitar, and room presentation.
- Prevents touch conflicts by isolating the strumming area from chord/shape selectors.

**Reference:** `Cozy Guitar Practice Interface.png`  
**Skill:** `ui-toolkit`, `mobile-input`

---

## Decision Template

Use this format for new entries:

```markdown
### ADR-NNN — Short Title

**Date:** YYYY-MM-DD
**Status:** Accepted / Superseded by ADR-NNN / Rejected

**Decision:**
What was decided.

**Reasoning:**
Why this option was chosen.

**Alternatives considered:**
What else was considered and why it was rejected.

**Skill / Workflow:** (if applicable)
```
