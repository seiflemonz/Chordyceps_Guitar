---
description: Use this workflow for new features. Inspect first, create a moderate plan, show exact file changes, get approval, implement only the approved scope, provide manual testing instructions, and commit only after developer confirmation.
---

# Content

## 1. Understand the Request

Before changing anything:

- Determine exactly what the developer wants.
- Identify the systems affected.
- Determine whether the feature is small, medium, or large.
- Do not start coding immediately.

If the request is ambiguous:

1. Make reasonable assumptions.
2. Clearly list those assumptions.
3. Ask the developer to confirm them before implementation.

---

## 2. Inspect the Existing Project

Always inspect relevant existing implementation before creating the plan.

Check, when applicable:

- Scripts
- Components
- Prefabs
- Scenes
- ScriptableObjects
- Assets
- Interfaces
- Related systems
- Relevant documentation
- Relevant skills

Do not assume a system does not exist.

Do not create duplicate functionality when an existing system can reasonably be extended.

---

## 3. Identify Existing Problems

If existing code makes the requested feature difficult, explicitly tell the developer.

Use:

```text
Existing Problem:
<problem>

Required Change:
<necessary change>

Feature:
<how the change enables the feature>
```

Do not hide architectural changes inside implementation.

---

## 4. Break Down Large Features

If the requested feature is large, divide it into logical implementation stages.

Example:

```text
Song System
1. Song data
2. Song playback
3. Chord timeline
4. Song selection
5. Song UI
```

Do not implement every stage automatically.

Only implement the approved stage.

---

## 5. Create the Plan

Before implementation, provide a moderate-detail plan containing:

### Feature

What will be implemented.

### Current Architecture

Relevant existing systems and how they currently work.

### Proposed Changes

What systems, classes, components, or data will change.

### Implementation Steps

The logical sequence of implementation.

### Files to Create

Every new file and its exact path.

### Files to Modify

Every existing file and its exact path.

### Integration

How the feature connects to existing systems.

### Risks

Relevant risks such as:

- Serialized references
- Input conflicts
- Performance
- Audio behavior
- Mobile behavior
- Coupling
- Existing functionality

### Assumptions

Any assumptions that require developer confirmation.

### Definition of Done

Specific conditions that must be true for the feature to be considered complete.

---

## 6. Show Exact File Scope

Before implementation, show:

```text
CREATE
- path/to/file
- path/to/file

MODIFY
- path/to/file
- path/to/file

DELETE
- none
```

Do not create, modify, or delete files outside the approved scope.

If a new file becomes necessary during implementation:

- If the change is meaningful, stop and ask for approval.
- If it is tiny, clearly necessary, and low-risk, it may be made and must be reported afterward.

---

## 7. Get Approval

Do not implement until the developer approves the plan.

Approval includes:

- Feature behavior
- Architecture
- File scope
- Assumptions
- Implementation approach

If requirements change, update the plan and get approval again.

---

## 8. Implement

After approval:

- Implement only the approved scope.
- Follow `AGENTS.md`.
- Follow relevant skills.
- Follow existing project conventions.
- Keep functions generally ≤20 lines.
- Prefer clean reusable systems.
- Keep responsibilities separated.
- Preserve existing behavior.
- Avoid unrelated refactoring.
- Do not add hypothetical future systems.

---

## 9. Handle Scope Changes

If implementation reveals something not included in the approved plan:

### Tiny and clearly necessary

Continue only if the change is:

- Small
- Directly required
- Low-risk
- Clearly within the approved feature's intent

Report it afterward.

### Meaningful change

Stop and inform the developer.

Explain:

```text
New Finding:
<what was discovered>

Why It Matters:
<why the approved plan is insufficient>

Required Change:
<what needs to change>

Impact:
<affected files, systems, architecture, or behavior>
```

Wait for approval before continuing.

---

## 10. Testing

The developer performs the functional testing.

The agent must **not** claim that the feature works, is fixed, or passes testing unless the developer has confirmed it.

After implementation, provide manual testing instructions containing:

### Test Setup

What scene, object, device, or state to use.

### Steps

Clear numbered actions.

### Expected Result

Exactly what should happen.

### Edge Cases

Relevant boundary or unusual cases.

### Unity Console

Tell the developer to check for:

- Errors
- Exceptions
- Unexpected warnings
- Missing references
- `NullReferenceException`
- Input System errors
- Audio errors
- Other relevant issues

---

## 11. Definition of Done

A feature is complete when:

- Approved implementation is finished.
- Only approved scope was changed.
- Code follows project conventions.
- Testing instructions were provided.
- Developer manually tested the feature.
- Developer confirmed the expected behavior.
- Relevant regressions were checked.
- No unresolved critical Unity errors remain.
- Necessary documentation is updated.
- The feature is ready to commit.

---

## 12. Git Commit

Only commit after the developer confirms the feature passes testing.

Before committing:

1. Inspect changed files.
2. Confirm only intended changes are included.
3. Ensure unrelated developer work is not included.
4. Create a clear commit message.

Example:

```text
feat: add guitar string touch interaction
```

Never:

- Rewrite Git history.
- Force push.
- Reset or discard developer work.
- Commit unrelated changes.

---

## 13. Final Report

After implementation and developer confirmation, report:

### Implemented

What was added.

### Files

```text
Created:
- ...

Modified:
- ...
```

### Testing

What the developer tested and confirmed.

Only report confirmed results.

### Notes

Important architectural decisions, necessary deviations, limitations, or relevant follow-up work.

---

## Workflow Summary

```text
Developer Request
        ↓
Inspect Existing Project
        ↓
Determine Scope
        ↓
Identify Existing Problems
        ↓
Make Assumptions Explicit
        ↓
Create Plan
        ↓
Show Exact Files
        ↓
Developer Approval
        ↓
Implement
        ↓
Handle Scope Changes
        ↓
Give Testing Instructions
        ↓
Developer Tests
        ↓
Developer Confirms
        ↓
Commit
        ↓
Final Report
```

**Core rule:** Do not code first and explain afterward. Understand the project, plan the change, show the exact scope, get approval, implement only what was approved, and let the developer verify the result.
