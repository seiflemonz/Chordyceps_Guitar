# Chord System Skill

## Purpose

Define the chord system architecture for Chordyceps, covering chord data models,
twelve musical keys, chord shapes, capo logic, and the chord-wheel interface.
The chord system is a data/gameplay layer. It does not own input, audio, or UI.

---

## Architecture Boundaries

The chord system has one responsibility: **managing and providing musical selection state**.

It should:

- Store the currently selected key, chord, and capo position.
- Provide chord/note information to the audio system on request.
- React to input intent events (key change, chord change, capo change).
- Expose readable state to the UI layer.

It should **not**:

- Detect or process touch input directly.
- Play audio or manage AudioSources.
- Own song progression or song playback.
- Drive UI rendering or layout.

---

## Data Model

Use ScriptableObjects for authored, reusable chord data. Follow the `scriptable-objects` skill.

### Chord Definition

A chord definition describes one chord in one key.

Suitable fields:

- Musical key (e.g., C, D#, G).
- Chord type (e.g., major, minor).
- Chord shape index (references a shape definition).
- Per-string note data needed for audio playback.

Do not store runtime selection state inside chord data assets.

### Chord Shape

A chord shape defines a finger/fret pattern that can be applied across keys.

Suitable fields:

- Shape name or identifier.
- Finger positions or fret offsets per string.

Shapes are reusable across keys. A shape combined with a key produces a playable chord.

### Key/Chord Mapping & Assigned Strings

The mapping between a key and its available chords is data-driven:

- **Circle of Fifths:** All scales have their chords defined according to the Circle of Fifths.
- **Assigned Strings:** All chords have their assigned strings (mapping which strings are played and their respective fret offsets).
- **Twelve Musical Keys:** Cycled via two key increment/decrement buttons (`<` / `>`) in the middle left.
- **Six Chord Shapes per Scale:** Cycled via the bottom-left thumbstick / analog stick.
- **Three Major & Three Minor Chords:** Available in the current key on the Circle of Chords (top right).
- **Custom Tunings:** Guitar tuning can be dynamically changed with custom presets saved.

Represent this mapping as authored data (ScriptableObjects) rather than hard-coded arrays
embedded in gameplay scripts.

---

## Runtime State

Keep mutable selection state in a runtime system, not in ScriptableObject assets.

Runtime state includes:

- Currently selected key / scale.
- Currently selected chord.
- Currently selected chord shape (1 to 6).
- Current capo position.
- Current tuning offsets.

A chord system component owns this state and exposes it through controlled access.

```csharp
// Example read access; do not expose setters unnecessarily.
public MusicKey CurrentKey { get; private set; }
public ChordData CurrentChord { get; private set; }
public int CurrentChordShapeIndex { get; private set; }
public int CurrentCapo { get; private set; }
```

---

## Capo Controls (Middle Top)

Two capo increment/decrement buttons (`-` / `+`) in the middle top shift all string pitches up or down by semitones.

The capo system should:

- Support increasing and decreasing the capo position via the middle-top buttons.
- Enforce minimum (0) and maximum capo position limits.
- Notify dependent systems when the capo changes.
- Be configurable (min/max values) rather than hard-coded.

Capo position affects which audio clips are selected for each string:
`NoteIndex = OpenStringTuningOffset + CapoFret + ChordFretOffset`.
The audio system receives the effective note rather than applying capo logic itself.

---

## Circle of Chords (Top Right)

The circle of chords is the primary interface for chord selection, positioned in the upper-right quadrant.

### Wheel Layout (Circle of Fifths)

The circle is partitioned relative to the currently active key:
- **Top 3 Segments (Major Chords):** `I` (Tonic), `IV` (Subdominant), `V` (Dominant)
- **Bottom 3 Segments (Minor Chords):** `ii` (Supertonic minor), `vi` (Submediant minor), `iii` (Mediant minor)

Example in C Major:
- Top: `C` (I), `F` (IV), `G` (V)
- Bottom: `Dm` (ii), `Am` (vi), `Em` (iii)

### Visual Feedback

When a chord is tapped or selected:
- The chord name on the circle of chords **glows** and **scales up** its portion of the circle.
- Center displays "TAP A CHORD TO SELECT" or the active chord details.

### Thumbstick Shape Cycling (Bottom Left)

A thumbstick in the bottom left cycles between the **6 chord shapes** on the selected scale. Each position changes the fingering pattern and left-hand fretboard pose.

### Responsibilities

The chord wheel UI:

- Displays available chords in the current key (3 major top, 3 minor below).
- Reflects the currently selected chord visually (glow & scale).
- Sends chord selection intent to the chord system.

The chord system:

- Receives the selection intent.
- Updates the current chord.
- Notifies listeners (UI, audio, animation) of the change.

### Input and Events

The chord wheel should not directly own the chord system state.

Use an event or method call to communicate selection:

```text
Chord Wheel UI
    | (OnChordSelected event or method)
    v
Chord System
    | (state update + event)
    v
Audio System / UI
```

Follow the `mobile-input` skill for touch interaction on the chord wheel.

---

## Events and Communication

Use events when other systems need to react to chord state changes.

Relevant events may include:

- Key changed.
- Chord changed.
- Chord shape changed.
- Capo changed.

Keep event payloads minimal. Pass only what the subscriber needs.

Do not let events become a substitute for clear ownership.
The chord system owns the state; other systems observe it.

---

## SOLID Application

- **S:** The chord system manages musical selection state. It does not play audio,
  detect input, or render the chord wheel.
- **O:** Adding a new chord shape or key should require authoring data, not editing
  the core chord system logic.
- **I:** Expose only the contract each consumer needs. A UI element that only reads
  the current key should not receive the entire chord system interface.
- **D:** The audio system should receive note/chord data through a clear boundary,
  not by directly accessing chord system internals.

---

## Performance

The chord system is not a hot-path performance concern for most operations.

However:

- Avoid expensive searches when resolving a chord from a key/shape combination.
- Cache resolved chord data when the selection is stable.
- Avoid per-frame chord resolution work if the selection has not changed.

---

## Validation

Validate chord data configuration at startup or in the Editor.

Useful checks:

- Required ScriptableObject references are not null.
- All twelve keys have the expected number of available chords.
- Chord shape indices are within valid range.
- Capo min/max are logically consistent.

Prefer early, clear errors over mysterious runtime failures.

---

## Integration Checklist

When implementing or modifying the chord system, confirm:

- [ ] Chord data is authored as ScriptableObjects, not hard-coded.
- [ ] Runtime selection state lives in a runtime component, not in assets.
- [ ] Key, chord, and capo changes fire the appropriate events.
- [ ] The audio system receives the effective note for the selected chord + capo.
- [ ] The UI reflects the correct chord/key/capo without owning the state.
- [ ] Capo limits are enforced and configurable.
- [ ] Chord wheel selection intent flows to the chord system cleanly.

---

## Core Rule

**The chord system owns musical selection state. It provides note/chord information
on request and fires events when selection changes. It does not play audio, process
touch input, or control UI rendering.**
