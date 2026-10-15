# Chordyceps — Architecture

> **Purpose:** Define the binding layer structure, class ownership boundaries,
> and inter-system contracts for Chordyceps. Update this file as architecture
> decisions are confirmed during implementation.
>
> This is a living document. Start sparse; add concrete class names and wiring
> details as systems are built and tested.

---

## Layer Map

```text
┌─────────────────────────────────────────────────────┐
│                   INPUT LAYER                       │
│  Detects touch/swipe/keyboard; emits intent events  │
│  Does NOT own chord logic, audio, or UI state       │
└───────────────────────┬─────────────────────────────┘
                        │ intent events
                        ▼
┌─────────────────────────────────────────────────────┐
│                 GAMEPLAY LAYER                      │
│  Interprets input; coordinates string/chord state   │
│  Does NOT know how audio is played internally       │
└──────────┬────────────┬────────────────┬────────────┘
           │            │                │
           ▼            ▼                ▼
┌──────────────┐ ┌─────────────┐ ┌──────────────────┐
│  CHORD/DATA  │ │ GUITAR AUDIO│ │   SONG SYSTEM    │
│     LAYER    │ │    LAYER    │ │                  │
│  Key/chord/  │ │  Voice pool │ │ Song data,       │
│  shape/capo  │ │  Playback   │ │ progression,     │
│  state       │ │  Effects    │ │ playback state   │
└──────┬───────┘ └──────┬──────┘ └────────┬─────────┘
       │                │                 │
       └────────────────┼─────────────────┘
                        │ state / events
                        ▼
┌─────────────────────────────────────────────────────┐
│                    UI LAYER                         │
│  Presents state; forwards player intent             │
│  Does NOT own gameplay rules or audio               │
└───────────────────────┬─────────────────────────────┘
                        │
          ┌─────────────┴──────────────┐
          ▼                            ▼
┌──────────────────┐        ┌─────────────────────────┐
│  3D PRESENTATION │        │  PERSISTENCE / SETTINGS │
│  Character,      │        │  Save/restore prefs      │
│  guitar, camera, │        │  PlayerPrefs or file     │
│  room, lighting  │        └─────────────────────────┘
└──────────────────┘
          │
          ▼
┌─────────────────────────────────────────────────────┐
│          ANALYTICS / MONETIZATION (isolated)        │
│  Narrow interfaces only; failures must not break    │
│  core guitar interaction                            │
└─────────────────────────────────────────────────────┘
```

---

## Layer Ownership Rules

Each entry states: **what the layer owns** and **what it must NOT do**.

### Input Layer

**Owns:** Touch detection, swipe/strum gesture recognition, editor mouse
simulation, PC keyboard bindings.

**Must NOT:** Play audio, change chord state, modify song state, directly
control any UI element.

**Emits:** Input intent events — string touched, string crossed, chord wheel
dragged, chord wheel released, key cycle requested, capo change requested.

**Skill:** `mobile-input`

---

### Gameplay Layer

**Owns:** Coordinating which strings should play for the current chord/key/capo
selection. Bridging input intent to audio requests and chord-system changes.

**Must NOT:** Know how audio voices are allocated internally. Know how the UI
renders the chord wheel. Own song progression timing.

---

### Chord/Data Layer

**Owns:** Currently selected musical key, chord, chord shape, and capo position.
Chord and key data assets (ScriptableObjects). Resolving which chromatic note to play
for the current selection.
- **Circle of Chords Layout:** Circle of Fifths organization. Divided into **three major
  chords on top** (I, IV, V) and **three minor chords below** (ii, vi, iii) relative to key.
- **Chord Shapes:** 6 distinct chord shapes per chord, selected via bottom-left thumbstick.
- **Tuning Management:** Tracks open string tuning offsets. Supports custom tuning presets.

**Must NOT:** Detect input, play audio, drive UI rendering, manage song data.

**Fires events on:** Key changed, chord changed, chord shape changed, capo changed, tuning changed.

**Skill:** `chord-system`, `scriptable-objects`

---

### Guitar Audio Layer

**Owns:** Voice pool (target 18 reusable AudioSources), voice stealing, chromatic
pitch audio bank, audio routing, optional electric effects.

- **Chromatic Pitch Bank:** All note audio clips loaded at startup, **ordered strictly by pitch**
  (semitone sequence from lowest to highest note).
- **String Decoupling:** Audio clips are **not** tied to a specific string. Any string can be
  tuned to any open note.
- **Note Calculation:** `NoteIndex = OpenStringTuningOffset + CapoFret + ChordFretOffset`.
- **Zero Pitch-Shifting:** Never uses Unity's `AudioSource.pitch`. Every pitch has its own authentic sample.

**Must NOT:** Own chord selection, detect input, drive UI, know song progression.

**Receives:** A string-play request specifying which note index to play.
Decides internally how to fulfill it from the voice pool.

**Skill:** `guitar-audio`

---

### Song System

**Owns:** Song library, song metadata, progression data, playback position,
playback state (playing/paused/stopped).

**Must NOT:** Play guitar audio directly, manage chord system state, render
the song list UI, process touch input.

**Fires events on:** Song selected, playback started/paused/stopped, chord
event reached, song ended.

**Skill:** `song-system`, `scriptable-objects`

---

---

### UI Layer

**Owns:** Visual presentation of game state. Receiving player intent from UI
interactions and forwarding it to the appropriate gameplay system.

**Must NOT:** Own chord rules, audio management, song progression logic, or
persistence logic.

**Technology:** UI Toolkit for 2D screens. TextMeshPro where appropriate.
Unity scene GameObjects for 3D presentation.

**Screen Layout Blueprint (based on `Cozy Guitar Practice Interface.png`):**

```text
┌────────────────────────────────────────────────────────────────────────┐
│ [☰ Menu]                [CAPO: - 0 +]             [KEY: C Major] [⚙]   │
│                                                                        │
│ [KEY SELECTOR]                                     [CHORD CYCLE]       │
│   <  C  >                                            Segmented wheel   │
│   C Major                                            (I, IV, V,        │
│   Tuning: Standard                                    ii, vi, iii)     │
│                                                                        │
│                       [3D CHARACTER & GUITAR]                          │
│                         Centered in cozy room                          │
│                                                                        │
│ [CHORD SHAPE]                                      [STRINGS AREA]      │
│  Analog Stick / Dial                                6 horizontal       │
│  (Positions 1 to 6)                                 strings (1e to 6E) │
│                                                     + Strum Bar        │
│                                                                        │
│  [Status Pill: Key / Shape / BPM]                                      │
└────────────────────────────────────────────────────────────────────────┘
```

**Key Layout Elements:**
1. **Center:** 3D character playing acoustic guitar seated in a cozy room.
2. **Bottom-Right:** 6 interactive horizontal strings (1e through 6E) with vertical strum bar.
3. **Upper-Right:** Circular chord cycle / wheel with diatonic chords (I, IV, V, ii, vi, iii).
4. **Top-Center:** Capo changer stepper (`-`, current fret `0`, `+`).
5. **Upper/Middle-Left:** Musical key selector (`< Key >`) and tuning selector.
6. **Bottom-Left:** Chord shape analog stick / dial (positions 1 through 6).

**Skill:** `ui-toolkit`

---

### 3D Presentation Layer

**Owns:** 3D character mesh, acoustic guitar mesh, perspective camera, room environment,
lighting, and character strum/fret animations.

- **3D Environment:** Isometric view room with 2 walls and a floor.
- **Framing:** Medium shot of main character seated on a chair holding the guitar (full view of guitar).
- **Character Idle Animation:** Leans back and forth to simulate natural breathing and blinking.
- **Right Hand (Strum Hand):**
  - Chords: Moves up/down following the exact chronological order of strings strummed.
  - Singular plucks: Moves upward when one of the 3 highest strings (G, B, high e) is plucked;
    moves downward when one of the 3 lowest strings (low E, A, D) is plucked.
- **Left Hand (Fret Hand):**
  - Forms the chord shape corresponding to the active thumbstick selection (shapes 1 to 6).
  - Translates position along the guitar neck based on root fret (e.g., F vs F#) and capo shifts.
- **UI Dynamic Feedback:**
  - Active chord sector on the Circle of Chords glows and scales up.
  - The 6 horizontal strings on the bottom-right visually wobble when plucked or strummed.

**Must NOT:** Own gameplay state, process input directly, manage audio clips.

---

### Persistence / Settings Layer

**Owns:** Saving and restoring user preferences and settings. Save-data schema.

**Must NOT:** Drive gameplay or audio decisions at runtime beyond providing
saved values at startup.

---

### Analytics / Monetization

**Owns:** Analytics events and monetization SDK integration only.

**Must NOT:** Be a dependency of any core guitar interaction system. Failures
in this layer must be silent and non-disruptive.

---

## Key Architecture Rules

1. **Direction of dependency:** Higher layers request services from lower layers
   through events or explicit method calls. Lower layers do not reach up into
   higher layers.
2. **No god objects:** No single MonoBehaviour may span more than one layer.
3. **Events over direct coupling:** Prefer C# events or UnityEvents when a system
   needs to react to state changes in another layer without owning it.
4. **Data-driven content:** Songs, chords, and audio configurations are authored
   as ScriptableObjects. Adding content should not require code changes.
5. **Interface boundaries:** When a layer boundary crosses a significant coupling
   risk, introduce a focused C# interface. Do not introduce interfaces by default
   for every class.

---

## Class Register

> Add concrete class names here as they are created. This prevents duplicate
> systems and helps agents find existing implementations before creating new ones.

| Class | Layer | File | Responsibility |
|---|---|---|---|
| `StringInputType` | Input | [StringInputType.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Input/StringInputType.cs) | Enum defining input interaction kinds (`Tap`, `Strum`) |
| `StringInputData` | Input | [StringInputData.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Input/StringInputData.cs) | Readonly event payload for string interactions |
| `IStringInputListener` | Input | [IStringInputListener.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Input/IStringInputListener.cs) | Contract for components listening to string input |
| `GuitarTouchInput` | Input | [GuitarTouchInput.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Input/GuitarTouchInput.cs) | Multi-touch and mouse gesture detection (overlap + linecast) |
| `GuitarKeyboardInput` | Input | [GuitarKeyboardInput.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Input/GuitarKeyboardInput.cs) | Editor/PC test bindings (numeric keys 1 to 6) |
| `GuitarString` | Gameplay | [GuitarString.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Gameplay/GuitarString.cs) | Individual string GameObject with index and hit area |
| `GuitarController` | Gameplay | [GuitarController.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Gameplay/GuitarController.cs) | Coordinates strings, receives input, dispatches pluck events |

---

## Scene Structure

> Document the main scene hierarchy here as it is built.

| Scene | Purpose |
|---|---|
| `GuitarPrototype.unity` | Main playable guitar scene with `[Systems]`, `[Gameplay]`, `[UI]`, `Main Camera`, `Directional Light`, `Global Volume` |

---

## Known Constraints

- **Unity 6000.3.7** — use modern APIs; avoid deprecated Unity behavior.
- **URP** — all rendering through Universal Render Pipeline.
- **New Input System only** — do not use legacy Input Manager for new code.
- **Android primary target** — all performance decisions must account for
  mid-range Android hardware, not the development PC.
- **No pitch shifting** — do not use `AudioSource.pitch` to transpose guitar
  notes. Use correctly recorded samples.
