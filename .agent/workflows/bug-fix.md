---
description: Use this workflow for bugs and unexpected behavior. Reproduce first, diagnose the root cause, propose the smallest safe fix, get approval when needed, provide regression tests, and commit only after developer confirmation.
---

# Content

## 1. Understand and Reproduce

Always obtain exact reproduction steps before diagnosing the bug.

If the developer has not provided enough information to reproduce the issue, ask for:

- What should happen
- What actually happens
- Exact reproduction steps
- Scene/state where it occurs
- Relevant input/actions
- Expected vs actual result
- Relevant Unity Console errors/warnings
- Whether it happens consistently or intermittently

Do not guess the reproduction conditions.

---

## 2. Inspect the Existing Implementation

Before changing anything, inspect the relevant:

- Scripts
- Components
- Prefabs
- Scenes
- ScriptableObjects
- Assets
- Input configuration
- Audio configuration
- Related systems
- Recent changes, when available

Understand how the affected system currently works.

Do not immediately rewrite the code.

---

## 3. Diagnose the Root Cause

Explain the suspected root cause **before proposing the fix**.

Use:

```text
Observed Behavior:
<what is happening>

Expected Behavior:
<what should happen>

Root Cause:
<why the problem is occurring>

Affected Files:
<relevant files>

Proposed Fix:
<smallest appropriate fix>
```

If there are multiple plausible causes:

1. Investigate them.
2. Determine which is most likely.
3. Explain the reasoning.
4. Recommend the most likely cause/fix.

Do not blindly modify code until a reasonable diagnosis exists.

---

## 4. Check for Architectural Problems

Determine whether the bug can be properly fixed within the existing architecture.

If the correct fix requires a significant architectural change:

**Stop before implementing it.**

Tell the developer:

```text
Architectural Problem:
<what is wrong>

Why the Current Architecture Causes It:
<explanation>

Why a Small Fix Is Not Appropriate:
<explanation>

Possible Architectural Change:
<proposed change>

Impact:
<systems/files/behavior affected>
```

Then ask:

> Should we change the architecture?

Do not make the architectural change without the developer's approval.

Do not hide an architectural change inside a bug fix.

---

## 5. Propose the Smallest Safe Fix

Bug fixes follow the same conservative scope rules as feature development.

Prefer:

- Small changes
- Existing systems
- Existing abstractions
- Minimal affected files
- Clear cause-and-effect

Avoid:

- Unrelated refactoring
- Rewriting working systems
- Adding unrelated features
- Premature optimization
- Large architectural changes without approval

The goal is:

> Fix the actual cause, not merely the visible symptom.

---

## 6. Get Approval When Required

Before implementing a significant fix, show:

```text
Fix Plan:
<steps>

CREATE
- <files>

MODIFY
- <files>

DELETE
- none

Expected Result:
<expected behavior>
```

If the fix is straightforward and already covered by the approved debugging direction, proceed.

If the fix changes architecture, scope, or behavior beyond the original bug, stop and ask for approval.

---

## 7. Implement the Fix

After approval:

- Implement only the approved fix.
- Follow `AGENTS.md`.
- Follow relevant skills.
- Keep functions generally ≤20 lines.
- Preserve unrelated behavior.
- Avoid unnecessary refactoring.
- Keep the change as small as reasonably possible.

---

## 8. Debugging Logs

Temporary or permanent debugging logs may be added when useful.

Useful logs may remain in the project when they provide legitimate ongoing diagnostic value.

Logs should:

- Clearly identify the relevant system.
- Provide useful state information.
- Avoid excessive spam.
- Avoid logging sensitive information.
- Be removable or disableable when appropriate.

Do not leave noisy or meaningless logs.

---

## 9. Developer Testing

The developer performs the functional verification.

The agent must never claim:

- "The bug is fixed."
- "It works now."
- "The issue is resolved."

unless the developer has actually tested and confirmed it.

Provide:

### Reproduction Test

Steps that previously caused the bug.

### Expected Fixed Behavior

What should happen now.

### Regression Tests

Existing behavior that should still work.

### Edge Cases

Relevant unusual conditions.

### Unity Console

Tell the developer what to check for:

- Errors
- Exceptions
- Warnings
- Missing references
- Input errors
- Audio errors
- Relevant diagnostic logs

---

## 10. Regression Testing

Always identify relevant existing behavior that should be retested.

Example:

```text
Regression Checks:
1. Normal string tap still plays audio.
2. Repeated taps still work.
3. Multiple strings can still be played.
4. Scene transition still works.
```

Do not assume that fixing one behavior cannot affect another.

---

## 11. If the Fix Fails

If the developer reports that the bug still exists:

1. Return to diagnosis.
2. Use the new test results as information.
3. Re-evaluate the root cause.
4. Do not blindly stack additional fixes.
5. Propose the next smallest appropriate investigation/fix.

If the evidence indicates an architectural problem, stop and discuss the architecture with the developer.

---

## 12. Definition of Done

A bug fix is complete when:

- The bug was reproduced or sufficiently characterized.
- Root cause was identified.
- The approved fix was implemented.
- Only necessary files were changed.
- Developer tested the original reproduction case.
- Developer confirmed the expected behavior.
- Relevant regression tests passed.
- No unresolved critical Unity errors remain.
- Useful diagnostic logs are retained where appropriate.
- The fix is ready to commit.

---

## 13. Git Commit

Only commit after the developer confirms the bug is fixed and relevant regression testing has passed.

Before committing:

1. Inspect changed files.
2. Confirm only intended changes are included.
3. Ensure unrelated developer work is not included.
4. Create a clear commit message.

Example:

```text
fix: prevent duplicate guitar string playback
```

Never:

- Rewrite Git history.
- Force push.
- Reset or discard developer work.
- Commit unrelated changes.

---

## 14. Final Report

After developer confirmation, report:

### Bug

What was wrong.

### Root Cause

Why it happened.

### Fix

What changed.

### Files

```text
Created:
- ...

Modified:
- ...
```

### Testing

What the developer tested and confirmed.

### Logs

Mention any useful diagnostic logs that were intentionally retained.

### Notes

Mention relevant limitations or architectural considerations.

---

## Workflow Summary

```text
Bug Report
    ↓
Get Exact Reproduction Steps
    ↓
Inspect Existing Implementation
    ↓
Reproduce / Characterize
    ↓
Diagnose Root Cause
    ↓
Check Architecture
    ↓
Propose Smallest Safe Fix
    ↓
Developer Approval if Required
    ↓
Implement
    ↓
Developer Tests
    ↓
Regression Testing
    ↓
Developer Confirms
    ↓
Commit
    ↓
Final Report
```

**Core rule:** Do not fix symptoms blindly. Reproduce the problem, understand the cause, fix the smallest appropriate thing, and stop if the real solution requires an architectural decision.
