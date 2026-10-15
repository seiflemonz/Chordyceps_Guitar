# UI Toolkit Skill

## Purpose

Define safe and maintainable practices for using Unity's UI Toolkit in Chordyceps,
targeting 2D interface screens on Android while keeping UI presentation separate
from gameplay rules and audio.

---

## Technology Choice

Chordyceps uses **UI Toolkit** for 2D interface screens where it is a good fit.

Use UI Toolkit for:

- Song selection and song library screens.
- Settings screen.
- Chord/key selector panels.
- Capo controls.
- Tutorial/onboarding screens.
- Any primarily 2D, non-world-space UI.

**Layout Reference:** `Cozy Guitar Practice Interface.png` and Primordial Vision
The main play interface anchors UI Toolkit elements to peripheral quadrants around the central 3D character:
- **Upper-Right (Circle of Chords):** Circle divided into 3 major chords on top and 3 minor chords below relative to key; selected chord name glows and scales up its slice.
- **Top-Center (Capo Stepper):** Two capo increment/decrement buttons (`-` / `+`) with current fret value.
- **Middle-Left (Key Stepper):** Two key increment/decrement buttons (`<` / `>`) to cycle between scales and chord shapes; tuning preset selector.
- **Bottom-Left (Thumbstick):** Analog stick to cycle between the 6 chord shapes on the selected scale.
- **Bottom-Right (Strings Rectangle):** Rectangle containing 6 horizontal strings from bottom to top (Low E at bottom to High e at top); strings wobble upon pluck/strum.
- **Bottom-Center:** Status pill (Key / Shape / BPM).

Use Unity scene GameObjects (not UI Toolkit) for:

- The 3D main character sitting on a chair holding the guitar (medium shot, full guitar view).
- The isometric room environment (2 walls and a floor).
- Any visual overlay tied directly to 3D world-space elements.

Use **TextMeshPro** where it remains appropriate, especially for existing or
world-space text. Do not replace TextMeshPro with UI Toolkit Label elements
unnecessarily.

---

## Architecture Boundaries

The UI layer presents state and sends player intent. It does not own rules.

UI should:

- Read state from gameplay systems (chord system, song system, audio system).
- Send player intent to gameplay systems (chord selected, song selected, etc.).
- Update its own visual presentation in response to events.

UI should **not**:

- Own chord selection logic.
- Directly manage audio playback.
- Control song progression timing.
- Store authoritative game state.

---

## UXML and USS

Structure UI documents cleanly.

- Use **UXML** for layout and structure.
- Use **USS** for visual styling.
- Do not embed inline styles in UXML when a USS class can be reused.
- Prefer USS classes over per-element style overrides.
- Keep UXML files focused on one screen or component.

Do not put game logic inside UXML or USS.

---

## Code-Behind (C#)

Each significant UI screen should have a corresponding C# class that:

- Queries and caches UI element references in `Awake` or equivalent initialization.
- Registers callbacks on relevant UI elements.
- Updates element state in response to game events.
- Sends player intent to gameplay systems through methods or events.

Avoid putting business logic, chord calculations, or audio calls directly
inside UI code-behind classes.

```csharp
// Good: UI informs the system; system owns the logic.
private void OnChordButtonClicked(ClickEvent evt)
{
    _chordSystem.SelectChord(_chordData);
}

// Bad: UI performs the chord logic itself.
private void OnChordButtonClicked(ClickEvent evt)
{
    _currentChord = CalculateChordFromKey(_selectedKey, _shapeIndex);
    _audioPlayer.Play(_currentChord);
}
```

---

## Element References

Query UI elements once at initialization; do not repeatedly search the document.

```csharp
private Button _settingsButton;

private void Awake()
{
    var root = GetComponent<UIDocument>().rootVisualElement;
    _settingsButton = root.Q<Button>("settings-button");
    Debug.Assert(_settingsButton != null, "Missing settings-button in UI document.");
}
```

Prefer `Q<T>("element-name")` with an explicit name over broad type-only queries
when element identity matters.

---

## Event Registration and Cleanup

Register callbacks at initialization.

Clean up callbacks when the UI screen is disabled or destroyed to avoid stale
references and unexpected behavior.

```csharp
private void OnEnable()
{
    _settingsButton.RegisterCallback<ClickEvent>(OnSettingsClicked);
}

private void OnDisable()
{
    _settingsButton.UnregisterCallback<ClickEvent>(OnSettingsClicked);
}
```

---

## Responding to Game State

Update UI elements in response to game events rather than polling every frame.

Prefer:

```text
Game system fires event
    |
    v
UI updates specific element(s)
```

Avoid:

```text
UI polls game state every frame
    |
    v
Rebuilds or refreshes entire layout
```

Only update what changed. Avoid triggering large layout rebuilds for small state
changes.

---

## Touch and Mobile Layout

All UI Toolkit screens must be designed for touch on Android phones.

- Buttons and interactive elements must meet minimum touch target sizes
  (44 x 44 dp or equivalent is a reasonable baseline).
- Do not rely on hover states for primary interactions.
- Use relative/percentage-based sizing and flex layouts rather than fixed pixel
  sizes where possible.
- Test on common Android aspect ratios (tall portrait 19.5:9, wide 16:9).
- Avoid fixed-width layouts that break on unusual screen widths.

Do not design UI in the Editor at a desktop resolution and assume it will look
correct on a phone. Test on a real device or with representative resolutions.

---

## Performance

UI Toolkit has its own rendering pipeline. Avoid unnecessary layout work.

- Do not recreate or reparent large numbers of VisualElements during normal gameplay.
- Cache frequently accessed elements rather than querying the document repeatedly.
- For list-based UI (e.g., song library), avoid creating one VisualElement per
  item if the list is large. Use virtualization (ListView) when appropriate.
- Avoid animating a large number of elements simultaneously if it causes frame
  drops on target hardware.
- Do not rebuild entire screens when only a small state value changed.

---

## Separation of Concerns

Do not merge UI Toolkit screens with gameplay MonoBehaviours into one large class.

Prefer:

- A focused UIDocument-owning component per screen.
- Separate code-behind classes per logical UI screen or component.
- Clear method names that describe intent (e.g., `UpdateChordDisplay`, `ShowSongSelected`).

Avoid a single MonoBehaviour that owns the guitar string area, the chord wheel,
the song library, and the settings screen simultaneously.

---

## USS Naming

Use consistent, readable class names in USS.

Suggestions:

- Use lowercase with hyphens (`chord-wheel__button--selected`).
- Use BEM-style naming when it aids clarity (`block__element--modifier`).
- Avoid cryptic abbreviations.

Do not introduce a naming convention that conflicts with the existing project convention.

---

## Debugging UI Issues

Common UI Toolkit issues to check:

- Missing element names causing `Q<T>()` to return null.
- Callbacks not registered or cleaned up correctly.
- Layout issues on portrait vs. landscape aspect ratios.
- Font assets not assigned to Label elements.
- Missing USS class causing elements to appear unstyled.

Add `Debug.Assert` checks for critical element references during development.

---

## Integration with TextMeshPro

TextMeshPro may coexist with UI Toolkit.

Do not replace working TextMeshPro text with UI Toolkit Label elements solely
for consistency. Replace only when there is a concrete benefit or when the UI
is being rebuilt anyway.

---

## Core Rule

**Use UI Toolkit for 2D screens. Keep UI presentation separate from gameplay rules.
Query elements once, register callbacks cleanly, update only what changed, and design
every screen for portrait touch on Android phones.**
