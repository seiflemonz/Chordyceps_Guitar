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

| Status | Meaning |
|---|---|
| 🔴 Open | Known issue, not yet addressed |
| 🟡 In Progress | Being investigated or fixed |
| 🟢 Resolved | Fixed and confirmed by developer testing |
| ⚪ Deferred | Acknowledged, intentionally left for later |
| ❌ Won't Fix | Considered and decided not to address |

---

## Severity Legend

| Severity | Meaning |
|---|---|
| **Critical** | Prevents core gameplay; must be resolved before proceeding |
| **High** | Significantly degrades experience or architecture |
| **Medium** | Meaningful problem, does not block immediate progress |
| **Low** | Minor issue, cosmetic, or edge case |

---

## Open Findings

### FIND-002 — String Y-Position Inversion in Prototype Scene

**Date:** 2026-10-15  
**Status:** 🔴 Open  
**Severity:** Medium  
**Area:** Input / Gameplay (Scene setup)  

**Observed:**
In `GuitarPrototype.unity`, String_0_E2 (Low E) is placed at `y = 2.5` (top) and String_5_E4 (High e) is placed at `y = -2.5` (bottom).

**Expected:**
Per the primordial vision and real-world guitars: "A rectangle containing the 6 strings from bottom to top (low e to high e)". String 0 (Low E) should be at the bottom (`y = -2.5`) and String 5 (High e) at the top (`y = 2.5`).

**Root Cause (if known):**
Initial 2D test setup aligned strings from top to bottom (index 0 at top) before the bottom-up reference layout was formalized.

**Affected Files:**
- `Assets/_Project/Scenes/GuitarPrototype.unity`

**Action Taken:**
Logged finding; will invert string Y-positions in the scene during Phase 2 layout alignment.

---

## Resolved Findings

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
