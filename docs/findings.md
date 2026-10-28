# Chordyceps — Findings

> **Purpose:** A running log of bugs, technical issues, architectural problems,
> performance concerns, and notable observations discovered during development.
> This is not a task tracker. It is a shared memory — a place to record what
> was found, what was done about it, and what still needs attention.
>
> Add entries as issues are discovered. Update status when resolved. Never delete
> entries; mark them Resolved instead.

---

## Status Legend

| Status         | Meaning                                    |
| -------------- | ------------------------------------------ |
| 🔴 Open        | Known issue, not yet addressed             |
| 🟡 In Progress | Being investigated or fixed                |
| 🟢 Resolved    | Fixed and confirmed by developer testing   |
| ⚪ Deferred    | Acknowledged, intentionally left for later |
| ❌ Won't Fix   | Considered and decided not to address      |

---

## Severity Legend

| Severity     | Meaning                                                    |
| ------------ | ---------------------------------------------------------- |
| **Critical** | Prevents core gameplay; must be resolved before proceeding |
| **High**     | Significantly degrades experience or architecture          |
| **Medium**   | Meaningful problem, does not block immediate progress      |
| **Low**      | Minor issue, cosmetic, or edge case                        |

---

## Open Findings

### FIND-005 — Settings Menu ScrollView Not Responding to Mouse Scroll Wheel

**Date:** 2026-10-27  
**Status:** 🔴 Open  
**Severity:** Low  
**Area:** UI / Input

**Observed:**  
In the Unity Editor and PC builds, scrolling the mouse wheel does not scroll the `SettingsMenu` `ScrollView` while the options panel is open. Dragging the scrollbar works, but mouse wheel input does not scroll the content.

**Expected:**  
Spinning the mouse wheel while hovering or focusing the Settings Menu should smoothly scroll the menu vertically.

**Root Cause (if known):**  
UI Toolkit's `ScrollView` default mouse wheel handling may require explicit focus or wheel events dispatched via an `EventSystem` / `InputSystemUIInputModule`, or custom scroll values applied in `Update()` need event-based routing (`WheelEvent`) or proper scroll delta scaling.

**Affected Files:**

- `Assets/_Project/Scripts/UI/SettingsMenuController.cs`
- `Assets/_Project/UI/SettingsMenu.uxml`
- `Assets/_Project/UI/SettingsMenu.uss`

**Action Taken:**  
Documented as deferred for now since mobile touch scrolling is the primary target platform.

---

## Resolved Findings

### FIND-006 — Amp AudioMixer Cannot Be Authored From Scripting API

**Date:** 2026-10-28  
**Status:** � Resolved  
**Severity:** Low  
**Area:** Audio / Architecture

**Observed:**  
The electric-guitar amp feature requires an `AudioMixer` asset (`Assets/_Project/Audio/GuitarMixer.mixer`) with an `Electric` child group and seven exposed parameters. Unity exposes no public scripting API to author an `.mixer` asset — mixers can only be created through the Editor UI (`Create > Audio > Audio Mixer`). Because of this, `AmpMixerController` cannot create its own mixer and the mixer must be built by hand once.

**Expected:**  
Running `Tools/Chordyceps/Setup Amp Panel` should be the only manual setup step required.

**Root Cause (if known):**  
Unity's `AudioMixer` authoring is Editor-only; the relevant APIs (`AudioMixerController.CreateMixer`, etc.) are internal/undocumented. Hand-writing the `.mixer` YAML was rejected as fragile.

**Affected Files:**

- `Assets/_Project/Audio/GuitarMixer.mixer` (must be created manually)
- `Assets/_Project/Scripts/Audio/AmpMixerController.cs`
- `Assets/_Project/Scripts/Audio/AmpMixerParameters.cs`
- `Assets/_Project/Scripts/UI/Editor/AmpPanelSetup.cs`

**Action Taken:**  
Implemented a robust "contract-based" architecture where the code defines the required exposed parameters and provides a `Tools/Chordyceps/Validate Amp Mixer` utility to check them. `AmpMixerController` was updated to handle a dual-stage gain chain (`AmpDrive` pre-distortion boost via Compressor + `AmpGain` saturation via Distortion) and an S-curve mapping for `ParamEQ` bands to simulate a real amp tone stack.

**Notes:**  
The manual mixer setup instructions are documented in the final feature report and the findings entry. Once authored, all 43 DI notes can be dropped into `DefaultElectricBank.asset` with zero code change.

### FIND-004 — Chord Shape Redundancy & Generator File Inconsistencies

**Date:** 2026-10-18  
**Status:** 🟢 Resolved  
**Severity:** Medium  
**Area:** Chord System / Architecture

**Observed:**
Earlier batch generation created redundant duplicate chord assets per scale (72 files) and left orphaned assets with conflicting naming conventions (e.g., `Chord_Fs.asset` vs `Chord_F#.asset`), causing validation warnings (`Chords: 25/24`).

**Root Cause:**
Chords were tightly coupled to scale Roman numerals inside `ChordDefinition`, forcing each scale to own separate chord asset duplicates. The generator also lacked automated cleanup of extraneous unreferenced `.asset` files.

**Affected Files:**

- `Assets/_Project/Scripts/Chords/Data/ChordDefinition.cs`
- `Assets/_Project/Scripts/Chords/Data/ScaleDefinition.cs`
- `Assets/_Project/Scripts/Chords/ChordManager.cs`
- `Assets/_Project/Scripts/Chords/Editor/ChordAssetGenerator.cs`
- `Assets/_Project/Scripts/Chords/Editor/ChordSystemValidation.cs`

**Action Taken:**

- Decoupled `_romanNumeral` from `ChordDefinition` to `ScaleDefinition.GetRomanNumeral(slotIndex)`, allowing all 12 scales to reference 24 canonical, shared chord assets in `Assets/_Project/ScriptableObjects/Chords/Definitions/`.
- Implemented `CleanupExtraneousAssets()` with case-insensitive path matching in `ChordAssetGenerator.cs` to eliminate extraneous assets.
- Integrated automatic verification via `ChordSystemValidation.RunValidation()` and scene saving upon running `Tools > Chordyceps > Generate 12`.

---

### FIND-002 — String Y-Position Inversion in Prototype Scene

**Date:** 2026-10-15  
**Status:** 🟢 Resolved  
**Severity:** Medium  
**Area:** Input / Gameplay (Scene setup)

**Observed:**
In `GuitarPrototype.unity`, String_0_E2 (Low E) was placed at `y = 2.5` (top) and String_5_E4 (High e) was placed at `y = -2.5` (bottom).

**Expected:**
Per the primordial vision and real-world guitars: "A rectangle containing the 6 strings from bottom to top (low e to high e)". String 0 (Low E) should be at the bottom (`y = -2.5`) and String 5 (High e) at the top (`y = 2.5`).

**Root Cause (if known):**
Initial 2D test setup aligned strings from top to bottom (index 0 at top) before the bottom-up reference layout was formalized.

**Affected Files:**

- `Assets/_Project/Scenes/GuitarPrototype.unity`

**Action Taken:**
Inverted string Y-positions in `GuitarPrototype.unity`: String 0 at `y = -2.5` up to String 5 at `y = 2.5`.

---

### FIND-003 — Guitar Strings Missing Visual Renderer in Scene

**Date:** 2026-10-16  
**Status:** 🟢 Resolved  
**Severity:** Medium  
**Area:** Gameplay / UI (Visuals)

**Observed:**
The 6 guitar strings were invisible in the prototype scene; only colliders were active.

**Root Cause (if known):**
Scene GameObjects lacked visual renderer components; `GuitarString` relied on `SpriteRenderer` which was null.

**Affected Files:**

- `Assets/_Project/Scripts/Gameplay/GuitarString.cs`

**Action Taken:**
Upgraded `GuitarString.cs` with auto-initialized `LineRenderer`, realistic string gauge widths (Low E thick to High e thin), and decaying sine-wave pluck wobble animation.

---

### FIND-001 — Android SDK/NDK Tools Missing on Manual Unity 6 Install

**Date:** 2026-10-15  
**Status:** 🟢 Resolved  
**Severity:** High  
**Area:** Architecture / Other (Build pipeline)

**Observed:**
Unity Editor warning: `Failed getting available Android API levels. Could not find Android SDK Tools.`  
In Unity Hub, the `Add modules` option was missing because Unity 6000.3.7 was installed manually outside the Hub.

**Root Cause:**
Android SDK components (`build-tools`, `cmdline-tools`, `platform-tools`, `platforms`, `cmake`) were not populated in Unity's internal `PlaybackEngines\AndroidPlayer\SDK\` folder.

**Remediation:**
Extracted the official SDK components (`build-tools 36.0.0`, `cmdline-tools 16.0`, `platform-tools 36.0.0`, `platforms android-34 & android-35`, `cmake 3.22.1`) into `D:\UNITY VERSION\EDITORS\6000.3.7f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\`. Unity automatically detects the SDK via default settings.

---

## Finding Template

Copy this for each new finding:

```markdown
### FIND-NNN — Short Description

**Date:** YYYY-MM-DD
**Status:** 🔴 Open
**Severity:** Critical / High / Medium / Low
**Area:** Input / Audio / Chord System / Song System / UI / Performance / Architecture / Other

**Observed:**
What was seen. Be specific — include scene, action, and result.

**Expected:**
What should have happened.

**Root Cause (if known):**
What caused the problem.

**Affected Files:**

- path/to/file.cs

**Action Taken:**
What was done (leave blank if Open).

**Notes:**
Relevant context, related decisions, or follow-up concerns.
```

---

## Notes for Agents

- Before implementing any feature, scan this file for open findings in the
  relevant area. An open finding may affect the design of the new feature.
- When a bug fix resolves a finding, update its status to 🟢 Resolved and
  note the fix briefly.
- When implementation reveals a new problem that is out of scope for the current
  task, add it here and mention it in the task report. Do not silently ignore it.
- Do not use this file as a task list. Use it as a shared technical memory.
