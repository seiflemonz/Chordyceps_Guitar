# Chord Maker — Implementation Notes

> **Purpose:** Record what the Chord Maker window does, the logic behind it, and
> what still needs verifying. Update this file as the feature changes.
>
> **Status:** 🟡 In Progress — core browsing/editing/creating confirmed by the
> developer; the Clear button, the save-to-default check, and barre drawing are
> implemented but not yet confirmed. Barre drawing is an explicit trial.

**Date:** 2026-11-03

---

## What it is

A full-screen UI Toolkit screen that lists every chord definition in the game and
lets the player change a definition's fingering or create new chords of their own.
It is opened from the Settings menu.

It is the only place in the project that renders a chord diagram, and the only
place that writes player-authored chord data.

**Layout note:** unlike the HUD, this screen is not overlayed on the main game —
it takes the whole screen via its own `UIDocument`.

---

## Scene wiring

| Object                      | Role                                                                       |
| --------------------------- | -------------------------------------------------------------------------- |
| `[ChordMaker]` `UIDocument` | Hosts `ChordMaker.uxml` / `ChordMaker.uss`; carries `ChordMakerController` |
| `[SettingsMenu]`            | Calls `ChordMakerController.Open()`                                        |
| `ChordManager`              | Authoritative chord state; the Chord Maker only previews and overrides it  |
| `GuitarController`          | Drives the audition strings                                                |

`ChordMakerController` requires a `UIDocument` on its own GameObject and resolves
`_chordManager` / `_guitarController` by `FindFirstObjectByType` if the serialized
references are left empty.

---

## Screen contents

- **Centre:** a large vertical guitar neck (`ChordNeckElement`) — six string lines,
  a nut and five fret spaces, drawn with `Painter2D`. Anchored to the centre bottom
  and grown with a taller : wider ratio than the main game's strings.
- **Above the neck:** the selected chord's name, centred.
- **Left of the neck:** the fret-window label plus shift up / shift down buttons.
- **Horizontal strip of chord chips:** one chip per listed chord, scrolled
  horizontally, with the active chip highlighted.
- **A square `+` button:** always visible beside the strip and deliberately outside
  the scroll logic.
- **Bottom right:** the same six playable strings as the main game, so the chord
  being viewed or edited can be auditioned while the window is open.
- **Toolbar:** Back (leave the window) always; Edit, Delete and Reset while just
  browsing; Clear, Cancel and Save while editing. Buttons appear and disappear with
  the current mode rather than being disabled.

---

## Features

### Browsing

- Lists every chord definition reachable from `ChordManager` — all keys, all scales,
  de-duplicated by scale — followed by any player-created chords.
- Selecting a chip shows the chord on the neck, centred name included, and auditions
  it through the playable strings.
- Chips are visually marked when a chord is player-created or modified.

### Editing a pre-existing chord

- **Edit** opens a working copy of the selected chord's fingering.
- Tap a fret cell to place that fret on the string, or tap the same cell again to
  open the string. Tap the X/O marker above the nut to mute or unmute a string.
- Shift up / shift down move the visible fret window one fret at a time.
- **Clear** empties the whole fingering: every string back to open, and the window
  back to the first fret.
- Dots are amber while editing and green while just viewing.
- **Save** stores the edit as an override (see _Override layer_ below).
- **Cancel** discards the working copy and restores the chord as it was.
- **Reset** clears every override at once, returning all pre-existing chords to their
  asset defaults. Player-created chords are not affected.

### Creating a chord

- **`+`** starts a blank fingering named `New Chord`.
- Saving asks for a name. Names are trimmed, must be non-empty, and must be unique
  across every listed chord — definitions, overrides and existing customs alike.
- A player-created chord can be renamed and re-saved; **Delete** removes it after a
  confirmation, and it is the only kind of chord that can be deleted.

### Leaving with unsaved work

- Switching chips, pressing `+`, or saving over an open draft while it has unsaved
  changes raises a modal: **Cancel** / **Don't Save** / **Save**.
- The prompt is only shown while the draft is actually dirty.

---

## Logic

### Fret window

The neck shows five frets at a time, and `ChordNeckElement.DeriveBaseFret` picks which
five from the fingering:

- If the highest fretted note is **5 or lower**, the window stays at fret 1 and **no**
  fret-number label is drawn — this is what keeps `Em` and `A` from appearing shifted.
- Otherwise the window starts at the lowest fretted note (clamped to 1–12) and the
  label is shown.

The chord is never transposed to fit; the window moves instead. A fingering spanning
more than five frets is therefore clipped at the bottom edge.

### Override layer

`ChordManager` resolves a string's fret in this order:

```text
preview (Chord Maker only) → override (saved player edit) → ScriptableObject default
```

- Editing a **pre-existing** chord writes an override keyed by the chord's name.
  Because `ChordManager` reads overrides game-wide, a definition changed here plays
  that way in the main game until it is reset — e.g. turning D into a barre variant
  makes the main game play the barre variant.
- **Player-created** chords are not overrides and are visible in the Chord Maker only.
- Both kinds live in the same store, so `CustomChordStore` is a read/write catalogue
  for the Chord Maker and the source of overrides for `ChordManager` at the same time.

### Save detects a return to default

Saving a pre-existing chord compares the working copy against the asset's original
fingering. If they match, the override is **removed** instead of written, so the
chord is marked unaltered again. This applies only to non-custom chords.

### Preview lifecycle

The audition is a preview on `ChordManager`, not a real selection, so it must never
outlive the window:

- `Open` and every selection apply the preview.
- `Close` clears it **last**, after the selection has been re-applied. If it were
  cleared first, `ApplySelection` would set it straight back and the main game would
  stay frozen on the chord shown in the Chord Maker.
- `OnDisable` clears it too, and the draft's audition frets are dropped last during
  closure so they cannot leak into main-game resolution.

### Barre drawing (trial)

`ChordBarre.TryFind` derives a barre from the frets alone — nothing is authored per
chord, so it applies to definition assets, overrides and player-created chords alike,
and it updates live while editing. A barre exists when:

1. the lowest fretted note is held on **two or more** strings, and
2. either something else is fretted **above** it, **or** it spans **more than four**
   strings, and
3. no string **between** its two ends is left open or muted.

Condition 2 is what separates a real barre from a shape that merely shares a note:
`A` `[-1,0,2,2,2,0]` and `Em` `[0,2,2,0,0,0]` are excluded, while `F`, `B`, `Ab` and
friends are kept. Condition 3 exists because a barre is one finger lying flat, so it
cannot cross an open or muted string.

When found, `ChordNeckElement` draws it as a rounded capsule behind the dots —
thickness 0.62 × a dot's diameter, alpha 0.55 — and skips it if its fret falls
outside the visible window.

---

## Data and persistence

- `CustomChordStore` (static) persists to `PlayerPrefs` under
  `chordyceps.chord_store_v1` as a `JsonUtility` payload containing two lists:
  `Overrides` and `Customs`.
- Records are sanitised on load: malformed entries are dropped and every fret array
  is forced to six entries, so a corrupt save cannot crash the game.
- `ChordEntry` is the serializable record: `Id`, `Name`, `Frets[6]`, `IsCustom`,
  `IsMajor`. Overrides are keyed by the source chord's name; customs by a generated id.
- There is no migration path yet — bumping the key would silently discard player work.

---

## Files

| File                                                                                                                                                                                                | Responsibility                                                      |
| --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- |
| [ChordMakerController.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordMakerController.cs)                                                                           | Orchestration only: window, toolbar, save/delete/reset flows        |
| [ChordCatalog.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordCatalog.cs)                                                                                           | Builds and owns the listed chords; name lookup and uniqueness       |
| [ChordDraft.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordDraft.cs)                                                                                               | The working copy: fret toggles, mute toggles, base fret, dirty flag |
| [ChordListItem.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordListItem.cs)                                                                                         | One listed chord; default-vs-current comparison                     |
| [ChordStripView.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordStripView.cs)                                                                                       | The chip strip and its selection event                              |
| [ChordNeckElement.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/UI/ChordNeckElement.cs)                                                                                   | Draws the neck, dots and barre; emits tap/shift intents             |
| [ChordBarre.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Chords/ChordBarre.cs)                                                                                           | Pure-C# barre detection from a fret array                           |
| [CustomChordStore.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Chords/CustomChordStore.cs)                                                                               | Player-authored chord persistence                                   |
| [ChordEntry.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Chords/ChordEntry.cs)                                                                                           | Serializable chord record                                           |
| [ChordManager.cs](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/Scripts/Chords/ChordManager.cs)                                                                                       | Preview → override → default resolution                             |
| [ChordMaker.uxml](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/UI/ChordMaker.uxml) / [ChordMaker.uss](file:///d:/UNITY%20GAMING/Chordyceps_Guitar/Assets/_Project/UI/ChordMaker.uss) | Layout and styling                                                  |

Reused rather than duplicated: `ModalDialog` (messages, name input) and
`GuitarStringPanel` (the audition strings).

---

## Verification status

| Area                                                            | Status                                              |
| --------------------------------------------------------------- | --------------------------------------------------- |
| Window, neck, chip strip, `+` button, centred name              | Confirmed by developer                              |
| Editing, auditioning, saving, cancelling, unsaved-changes guard | Confirmed by developer                              |
| Playable strings in the bottom-right corner                     | Confirmed by developer                              |
| Stale chord after closing the window                            | Fix implemented — see `docs/findings.md`            |
| Clear button                                                    | Implemented, awaiting developer verification        |
| Save-to-default clears the override                             | Implemented, awaiting developer verification        |
| Delete / Reset confirmations                                    | Implemented, awaiting developer verification        |
| Barre drawing                                                   | **Trial** — implemented, not verified in the Editor |

---

## Known limitations

- `ChordMakerController` remains the largest UI script in the project. It was split
  once into the four collaborating classes above, but it still owns every toolbar and
  modal flow.
- The barre rule is literal, so a common shape such as `D` `[-1,-1,0,2,3,2]` gets a
  barre at fret 2 that visually crosses the B string, which is really fretted at
  fret 3. Refining the rule further was explicitly deferred (FIND-008).
- Mouse wheel scrolling in UI Toolkit `ScrollView`s does not work in the Editor
  (FIND-005); the chord strip relies on dragging.
- Chords edited here change the main game. There is no "preview only" mode.
- No save-format versioning: a future schema change would need a migration or would
  discard player-authored chords.
