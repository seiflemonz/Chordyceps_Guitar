# Chordyceps — Roadmap

> **Purpose:** Track the current development phase, task status, and what is
> blocked or deferred. This is the agent's source of truth for development order.
> Update this file as phases are entered, tasks completed, and priorities shift.
>
> Phases are ordered by risk reduction, not by feature desirability.
> Protect the core guitar loop and the launch window above all else.

---

## Target Launch Window

**January 1–15, 2027**

Freeze MVP scope early. Defer non-essential features rather than shipping a
broken core experience.

---

## Current Phase

> **Update this section whenever the active phase changes.**

**Active Phase:** Phase 3 — Chords, Keys, and Capo
**Phase Status:** 🟡 In Progress

---

## Phase 1 — Project Foundation

**Goal:** A clean foundation exists and a basic Android build can be produced.

| Task                                                  | Status  | Notes                                                                             |
| ----------------------------------------------------- | ------- | --------------------------------------------------------------------------------- |
| Unity project created                                 | ✅ Done | Unity 6000.3.7, URP                                                               |
| Git / GitHub version control                          | ✅ Done |                                                                                   |
| `.gitignore` and `.gitattributes`                     | ✅ Done |                                                                                   |
| `AGENTS.MD` agent rules                               | ✅ Done |                                                                                   |
| Project overview document                             | ✅ Done | `Chordyceps_Project_Overview.md`                                                  |
| Core skills and workflows                             | ✅ Done | 9 skills, 4 workflows                                                             |
| Architecture and decisions docs                       | ✅ Done | `docs/architecture.md`, `docs/decisions.md`                                       |
| Findings log                                          | ✅ Done | `docs/findings.md`                                                                |
| Roadmap                                               | ✅ Done | This file                                                                         |
| New Input System configured                           | ✅ Done | `InputSystem_Actions.inputactions` present                                        |
| Core scene setup                                      | ✅ Done | Roots `[Systems]`, `[Gameplay]`, `[UI]`, Camera, Light in `GuitarPrototype.unity` |
| Mobile build configuration                            | ✅ Done | Min SDK 26, IL2CPP confirmed, ARM64, SDK tools installed                          |
| Basic Android build verified                          | ✅ Done | APK built and verified                                                            |
| Input / gameplay / audio layer boundaries established | ✅ Done | `Scripts/Core`, `Input`, `Gameplay`, `Audio`, `Chords`, `Song`, `UI` scaffolded   |
| Audio low latency configuration                       | ✅ Done | DSP buffer size set to Best Latency (256) per ADR-002                             |

**Exit condition:** A clean foundation exists and a basic Android build can be produced. (Passed ✅)

---

## Phase 2 — Playable Guitar Prototype

**Goal:** The player can interact with strings and hear the expected sounds reliably.

| Task                                             | Status  | Notes                                                             |
| ------------------------------------------------ | ------- | ----------------------------------------------------------------- |
| Six-string touch input in bottom-right rectangle | ✅ Done | 6 strings configured with BoxCollider2D & GuitarString            |
| Tap and swipe/strum detection                    | ✅ Done | Multi-touch, mouse drag strum & numeric keys 1-6 verified         |
| String Y-position alignment                      | ✅ Done | Low E (string 6) at bottom, High e (string 1) at top              |
| Audio voice pool implementation                  | ✅ Done | 18 pooled AudioSources, oldest-voice stealing confirmed           |
| Chromatic audio bank (pitch-ordered at startup)  | ✅ Done | ScriptableObject & Editor auto-sorter confirmed (43 clips D2-G#5) |
| Connect input intent → gameplay → audio          | ✅ Done | GuitarController wired; audio playback confirmed                  |
| Repeated taps verified                           | ✅ Done | Confirmed on Samsung Galaxy A70                                   |
| Overlapping notes verified                       | ✅ Done | Confirmed on Samsung Galaxy A70                                   |
| Responsive audio latency verified                | ✅ Done | Confirmed on Samsung Galaxy A70 (Best Latency DSP)                |

**Exit condition:** The player can interact with the strings and hear expected sounds reliably. (Passed ✅)

---

## Phase 3 — Chords, Keys, and Capo

**Goal:** Chord/key/capo changes consistently affect the playable guitar.

| Task                                   | Status     | Notes                                                                                                       |
| -------------------------------------- | ---------- | ----------------------------------------------------------------------------------------------------------- |
| Chord data model (ScriptableObjects)   | ✅ Done    | 24 shared chord definitions + 12 diatonic scales in Circle of Fifths order                                  |
| Twelve musical keys & scale navigation | ✅ Done    | All 12 keys generated, scene synced, keyboard navigation via `[` / `]`                                      |
| Major/Minor key mode toggle            | ✅ Done    | `KeySelectionController` toggle drives `ChordManager.SetKeyMode`; confirmed working by developer            |
| Chord shape selection (6 shapes)       | ✅ Done    | Bottom-left UI Toolkit analog selector maps degrees 1/5/2/6/3/4 to the current scale's playable chord slots |
| Circle of Chords UI wheel              | ✅ Done    | Top-right UI Toolkit wheel: 3 major, 3 minor, active glow bloom & scale animation                           |
| Capo controls (+/-)                    | ⬜ Pending | Middle-top buttons increment/decrement capo fret                                                            |
| Custom tuning preset manager           | ⬜ Pending | Change tunings, save presets; strings increment/decrement naturally                                         |
| Chord/key/capo → correct audio clips   | ⬜ Pending | Requires device test                                                                                        |

**Exit condition:** Chord/key/capo changes consistently affect the playable guitar.

---

## Phase 4 — Song System

**Goal:** A player can select a song and play along with its progression.

| Task                             | Status     | Notes                            |
| -------------------------------- | ---------- | -------------------------------- |
| Song data format defined         | ⬜ Pending |                                  |
| Initial licensed songs authored  | ⬜ Pending | Content rights must be confirmed |
| Song selection screen            | ⬜ Pending |                                  |
| Song timeline / progress display | ⬜ Pending |                                  |
| Song progression → chord events  | ⬜ Pending |                                  |
| Play-along experience verified   | ⬜ Pending | Requires device test             |

**Exit condition:** A player can select a song and play along with its progression.

---

## Phase 5 — UI and Presentation

**Goal:** The game is understandable and visually coherent on a phone.

| Task                                   | Status     | Notes                                                                         |
| -------------------------------------- | ---------- | ----------------------------------------------------------------------------- |
| Main guitar UI complete                | ⬜ Pending | Layout matching Cozy Guitar Practice Interface reference                      |
| Strings visual wobble feedback         | ✅ Done    | LineRenderer standing-wave wobble confirmed                                   |
| Circle of Chords glow & scale feedback | ⬜ Pending | Active slice glows and scales up                                              |
| 3D isometric room environment          | ⬜ Pending | 2 walls and a floor                                                           |
| 3D character and guitar models         | ⬜ Pending | Main character on chair, full view of guitar, medium shot                     |
| Character idle animation               | ⬜ Pending | Leans back/forth simulating breathing and blinking                            |
| Strumming hand animation (right hand)  | ⬜ Pending | Moves up/down with strum order; plucks: up for top 3, down for bottom 3       |
| Fretting hand animation (left hand)    | ⬜ Pending | Chord shape (1..6) from thumbstick; shifts along neck for fret offsets & capo |
| Song library UI complete               | ⬜ Pending |                                                                               |
| Camera framing for phone               | ⬜ Pending | Perspective camera framing room and character                                 |
| Basic onboarding / tutorial            | ⬜ Pending |                                                                               |
| Settings screen                        | ⬜ Pending |                                                                               |
| Phone aspect ratio testing             | ⬜ Pending | Requires device test                                                          |

**Exit condition:** The game is understandable and visually coherent on a phone.

---

## Phase 6 — Audio Polish and Persistence

**Goal:** The core experience is stable, responsive, and configurable.

| Task                               | Status         | Notes                                                                       |
| ---------------------------------- | -------------- | --------------------------------------------------------------------------- |
| Electric guitar mode + amp UI      | 🟡 In Progress | HUD⇄amp toggle, 7 knobs. Basic logic done; requires 43 individual DI notes. |
| Basic audio effects (reverb, gain) | 🟡 In Progress | Mixer chain implemented; requires final note assets.                        |
| Tone controls (bass, mid, treble)  | 🟡 In Progress | Mixer EQ bands implemented; requires final note assets.                     |
| Voice-pool behavior tuned          | ⬜ Pending     | Requires device profiling                                                   |
| Settings persistence               | ⬜ Pending     |                                                                             |
| Latency profiled on real device    | ⬜ Pending     |                                                                             |
| Memory profiled on real device     | ⬜ Pending     |                                                                             |

**Exit condition:** Core experience is stable, responsive, and configurable.

---

## Phase 7 — Release Preparation

**Goal:** The MVP meets the release checklist and can be submitted within the target window.

| Task                                 | Status     | Notes                     |
| ------------------------------------ | ---------- | ------------------------- |
| Analytics integration (minimal)      | ⬜ Pending | Graceful failure required |
| Monetization (if schedule allows)    | ⬜ Pending |                           |
| Fresh install tested                 | ⬜ Pending |                           |
| App interruption / resume tested     | ⬜ Pending |                           |
| All critical Console errors resolved | ⬜ Pending |                           |
| Store assets prepared                | ⬜ Pending |                           |
| Privacy disclosures reviewed         | ⬜ Pending |                           |
| Release build tested on device       | ⬜ Pending |                           |

**Exit condition:** MVP meets the release checklist and can be submitted.

---

## Deferred Features (Post-Launch)

These are explicitly out of scope for v1.0:

- MIDI/XML song import
- Automatic chord detection
- Large song library (hundreds of songs)
- Multiplayer or social features
- Physically accurate amp simulation
- Complex pitch shifting or full synthesizer
- Advanced IK for hand/finger animation
- Multiple large environments
- Extensive character customization
- Large backend / content platform

---

## Status Legend

| Symbol         | Meaning                       |
| -------------- | ----------------------------- |
| ✅ Done        | Completed and confirmed       |
| 🟡 In Progress | Actively being worked on      |
| ⬜ Pending     | Not yet started               |
| ⏸️ Blocked     | Cannot proceed; blocker noted |
| ⏭️ Deferred    | Intentionally postponed       |
