# Song System Skill

## Purpose

Define the song system architecture for Chordyceps, covering song data, progression
timelines, playback state, and song selection. The song system stores and serves song
information. It does not own the chord system, guitar input, or UI presentation.

---

## Architecture Boundaries

The song system has one primary responsibility: **managing song data, progression,
and playback state**.

It should:

- Store song metadata and chord progression data.
- Track current playback position within a song.
- Advance progression state during song play.
- Notify dependent systems of chord/timing events during playback.
- Provide the song library for selection.

It should **not**:

- Play guitar audio directly.
- Manage the chord system's selected key or shape.
- Own the UI that displays the song list or timeline.
- Process touch/input events.
- Load or serve arbitrary online audio streams.

---

## Data Model

Use ScriptableObjects for song data. Follow the `scriptable-objects` skill.

### Song Metadata

Suitable fields for a song metadata asset:

- Song title (display name).
- Artist/attribution (if applicable).
- Tempo (BPM).
- Duration.
- Stable identifier (do not rely on display name alone for identity).
- Artwork reference (if applicable).

### Song Progression Data

Song progression defines the chord timeline for a song.

A progression is a sequence of chord events. Each event specifies:

- When the chord change occurs (beat, timestamp, or bar).
- Which chord should be active (key + chord type, or a ChordData reference).

Keep the progression format simple for v1.0. A manually authored format is
acceptable for the initial release.

Separate progression data from metadata so they can evolve independently.

### Song Library

A song library asset may aggregate references to available songs.

Suitable fields:

- List of song metadata assets.

The library should be data-driven. Adding a new song means authoring a new
asset and adding it to the library, not modifying playback code.

---

## Playback State

Keep playback state in a runtime system, not in ScriptableObject assets.

Runtime playback state includes:

- Currently selected song.
- Current playback position (time or beat).
- Whether the song is playing, paused, or stopped.
- The upcoming/current chord event in the progression.

A song playback system owns this state and exposes it through controlled access.

```csharp
public SongData CurrentSong { get; private set; }
public bool IsPlaying { get; private set; }
public float PlaybackPosition { get; private set; }
```

---

## Playback Flow

A typical playback flow:

```text
Player selects song
    |
    v
Song System loads song data
    |
    v
Player starts playback
    |
    v
Song System advances playback position each frame
    |
    v
Song System fires chord-change event at the correct time
    |
    v
Chord System receives event and updates active chord
    |
    v
Audio System responds to chord change
```

The song system fires events; it does not directly command the chord system or
audio system to perform specific operations. Keep the direction of dependency clear.

---

## Timing

Song timing should be reliable enough for a musical play-along experience.

For v1.0:

- Use `Time.time` or `AudioSettings.dspTime` for timing, depending on what
  the project's audio configuration supports.
- Prefer `AudioSettings.dspTime` for audio-synchronized timing when low latency
  matters.
- Do not use `Update()` frame count as a timing source.
- Measure timing accuracy on real Android devices before finalizing the approach.

Avoid introducing complex synchronization infrastructure before it is needed.

---

## Song Selection

The song selection UI presents the song library.

The song system should expose:

- The list of available songs.
- Metadata for each song (title, duration, etc.).

The selection UI should not directly access progression data or playback internals.

When the player selects a song, the UI sends a selection intent to the song system.
The song system responds by loading the selected song.

```text
Song Selection UI
    | (OnSongSelected intent)
    v
Song System (loads song, updates state)
    | (OnSongLoaded event)
    v
UI / Presentation (updates display)
```

---

## Events and Communication

Use events when other systems need to react to song state changes.

Relevant events may include:

- Song selected / loaded.
- Playback started, paused, stopped.
- Progression position changed.
- Chord event reached (with chord data payload).
- Song ended.

Keep event payloads minimal. Pass only what the subscriber needs.

---

## SOLID Application

- **S:** The song system manages song data and playback state. It does not play
  audio, detect input, render the song list, or manage chord selection.
- **O:** Adding a new song should require authoring a data asset, not editing
  core playback logic.
- **L:** If multiple song sources share an interface (e.g., local vs future
  streaming), each must provide valid song data and consistent playback behavior.
- **I:** Expose only the contract each consumer needs. The song selection UI
  should not receive internal timing or playback APIs.
- **D:** The song system should provide chord events through a stable boundary.
  The chord system should not need to know about song data format internals.

---

## Content Rules

Only include songs and audio for which the project has the necessary rights.

- Do not use copyrighted recordings without appropriate licensing.
- Do not include songs based on technical accessibility alone.
- Keep a record of the license or rights basis for each included song.

A song must not be included in a build unless its content rights are confirmed.

---

## Performance

Song playback is not typically a hot-path CPU concern.

However:

- Avoid per-frame allocation in the progression-checking loop.
- Cache the current and next chord event rather than searching the full progression
  every frame.
- Progression data should be loaded before playback begins, not during it.

---

## Validation

Validate song data at load time.

Useful checks:

- Song metadata references are not null.
- Progression data is not empty.
- Chord references in progression events resolve to valid chord data.
- Song duration is consistent with progression length.

Prefer early, clear errors over mysterious playback failures mid-song.

---

## Integration Checklist

When implementing or modifying the song system, confirm:

- [ ] Song data is authored as ScriptableObjects, not hard-coded.
- [ ] Runtime playback state lives in a runtime component, not in assets.
- [ ] Adding a new song requires only new data assets, not code changes.
- [ ] Chord events are fired at the correct progression time.
- [ ] The chord system receives chord events and updates its state.
- [ ] The UI displays the song library from the song system, not from hard-coded data.
- [ ] All included song content is appropriately licensed.
- [ ] Song playback timing has been tested on a real Android device.

---

## Core Rule

**The song system owns song data, progression, and playback state. It fires events
at progression points. It does not play audio, manage chord selection, process input,
or render the UI. Adding a new song means authoring data, not editing code.**
