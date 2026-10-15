# Chordyceps

**A solo-developed, Android-first mobile guitar-playing simulator built in Unity.**

> Pick up a virtual guitar, choose a song, and play it.

---

## Project at a Glance

| Item | Value |
|---|---|
| Engine | Unity 6000.3.7 |
| Render Pipeline | Universal Render Pipeline (URP) |
| Primary Platform | Android |
| Target Launch | January 1–15, 2027 |
| Development Model | Solo developer + AI coding agent |

---

## Opening the Project

1. Clone the repository.
2. Open Unity Hub → **Open** → select the `Chordyceps_Guitar` folder.
3. Unity version: **6000.3.7** — use this exact version to avoid serialization
   drift.
4. Allow Unity to import assets on first open (may take a few minutes).
5. Set the build target to **Android** in File > Build Settings before working
   on mobile-specific code.

---

## Key Documents

| Document | Purpose |
|---|---|
| [`AGENTS.MD`](./AGENTS.MD) | AI agent rules, workflow, coding standards — **read first** |
| [`Chordyceps_Project_Overview.md`](./Chordyceps_Project_Overview.md) | Full product vision, SOLID principles, roadmap, release checklist |
| [`docs/architecture.md`](./docs/architecture.md) | Layer map, class ownership, system boundaries |
| [`docs/roadmap.md`](./docs/roadmap.md) | Phase status and task tracking |
| [`docs/decisions.md`](./docs/decisions.md) | Architectural decision log |
| [`docs/findings.md`](./docs/findings.md) | Bugs, issues, and technical observations |

---

## Agent Skills

Skills are loaded on demand from `.agent/skills/`. An agent should read only
the skills relevant to the current task.

| Skill | When to Use |
|---|---|
| `unity-core` | Any Unity component, lifecycle, or serialization work |
| `guitar-audio` | Voice pool, string playback, audio routing |
| `scriptable-objects` | Data assets, runtime state, validation |
| `mobile-input` | Touch, swipe, strum, Input System, editor simulation |
| `mobile-performance` | CPU/GPU/memory, profiling, optimization |
| `chord-system` | Chord data, keys, shapes, capo, chord wheel |
| `song-system` | Song data, progression, playback state, selection |
| `ui-toolkit` | UI Toolkit screens, UXML/USS, mobile layout |
| `solid-principles` | Applying SRP/OCP/LSP/ISP/DIP to this project |

---

## Agent Workflows

Workflows are in `.agent/workflows/`. Use the matching workflow for every task.

| Workflow | When to Use |
|---|---|
| `feature.md` | Implementing a new feature |
| `bug-fix.md` | Diagnosing and fixing a bug |
| `review.md` | Reviewing existing code or systems |
| `android-build.md` | Building for Android, device testing, release preparation |

---

## Project Principles (Short Form)

1. **Guitar interaction first** — latency and responsiveness before visual complexity.
2. **Scope discipline** — freeze MVP scope; defer everything that is not essential.
3. **Data-driven content** — songs, chords, and audio via ScriptableObjects, not hard-coded.
4. **One responsibility per class** — no god objects.
5. **Test on real devices** — Editor and emulator performance does not represent Android.
6. **Honest status** — never claim code works unless a human has tested and confirmed it.

---

## Repository Structure

```text
Chordyceps_Guitar/
├── AGENTS.MD                        # Agent rules and workflow
├── README.md                        # This file
├── Chordyceps_Project_Overview.md   # Full project specification
├── docs/
│   ├── architecture.md              # Layer map and system boundaries
│   ├── decisions.md                 # Architectural decision log
│   ├── findings.md                  # Bugs and technical observations
│   ├── roadmap.md                   # Phase and task tracking
│   └── features/                    # Per-feature implementation notes (created on demand)
├── .agent/
│   ├── skills/                      # Agent skill files
│   └── workflows/                   # Agent workflow files
└── Assets/
    └── _Project/                    # All project assets live here
```
