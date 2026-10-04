---
description: Use this workflow for code and system reviews. Inspect the requested system and its related dependencies, identify evidence-based issues, recommend concise fixes, and never modify code during the review.
---

# Content

## 1. Define Review Scope

Determine exactly what the developer wants reviewed.

Inspect:

- The requested system.
- Its directly related dependencies.
- Relevant interfaces.
- Relevant data structures.
- Relevant prefabs, scenes, or ScriptableObjects when applicable.
- Relevant configuration.

Do not review unrelated parts of the project.

---

## 2. Inspect Before Judging

Read the actual implementation before making recommendations.

Do not assume:

- How a system works.
- Why a design decision was made.
- That an unfamiliar pattern is incorrect.
- That a theoretically possible problem is currently relevant.

Base findings on the existing implementation.

---

## 3. Review Areas

Evaluate the relevant areas below:

### Bugs

Look for:

- Incorrect behavior.
- Incorrect state handling.
- Null/reference risks.
- Edge cases.
- Incorrect Unity lifecycle usage.
- Incorrect assumptions about dependencies.

### Architecture

Look for:

- Excessive coupling.
- Poor separation of responsibilities.
- Difficult-to-maintain dependencies.
- Duplicate systems.
- Unnecessary complexity.
- Poor boundaries between systems.

### Maintainability

Look for:

- Difficult-to-understand code.
- Excessively large methods/classes.
- Repeated logic.
- Unclear responsibilities.
- Fragile implementations.

### Performance

Report performance concerns when there is a reasonable technical basis.

Pay particular attention to:

- Unnecessary repeated work.
- Excessive allocations.
- Unnecessary Update loops.
- Uncontrolled object creation.
- Audio/polyphony issues.
- Mobile-specific complexity.

Do not report performance concerns merely because something _could theoretically_ be slower.

If complexity is likely to become a real performance problem, report it.

---

## 4. Avoid Hypothetical Problems

Do not report issues simply because they might become problematic someday.

A finding should have evidence in the current implementation or a clear technical reason that it already matters.

Avoid statements such as:

> "This might cause problems later."

Prefer:

> "This currently creates X because Y."

If a future concern is worth mentioning, clearly label it as a future consideration rather than a current issue.

---

## 5. Categorize Findings

Use:

```text id="7g4m2p"
Must Fix
Should Fix
Optional
```

### Must Fix

Issues that can cause:

- Incorrect behavior.
- Data loss.
- Serious instability.
- Significant architectural problems.
- Serious performance problems.
- High-risk maintenance problems.

### Should Fix

Problems that meaningfully reduce:

- Maintainability.
- Reliability.
- Clarity.
- Performance.
- Extensibility.

but do not currently threaten the project.

### Optional

Improvements that are genuinely useful but not necessary.

Do not turn personal style preferences into required fixes.

---

## 6. Explain Significant Findings

For each significant finding, provide a concise explanation:

```text id="5b8d1a"
Finding:
<what is wrong>

Evidence:
<what in the implementation demonstrates the issue>

Recommendation:
<what should be changed>

Impact:
<brief implementation impact>
```

Keep recommendations concise.

Do not provide a massive rewrite unless explicitly requested.

---

## 7. Do Not Modify Code

A review is analysis only.

Do not:

- Modify code.
- Create files.
- Delete files.
- Refactor the project.
- Apply fixes automatically.
- Commit changes.

If the developer wants a fix, that becomes a separate implementation task using the appropriate workflow.

---

## 8. Architectural Concerns

If a finding indicates that the current architecture is fundamentally unsuitable:

Explain:

- What the architectural problem is.
- Why the current implementation causes it.
- What alternatives exist.
- What the likely impact of changing it would be.

Do not modify the architecture during the review.

The developer decides whether an architectural change should happen.

---

## 9. Review Verdict

End every review with a concise verdict:

```text id="l1zv6w"
Verdict: Healthy
```

or:

```text id="jv8x4k"
Verdict: Needs Attention
```

or:

```text id="8u2xqk"
Verdict: Significant Issues
```

Use:

### Healthy

No meaningful issues were found.

### Needs Attention

The system works but contains issues worth addressing.

### Significant Issues

There are important problems that should be addressed before continuing to build heavily on the system.

---

## 10. Recommended Review Format

Use this structure:

```text id="w0m4c6"
# Review: <System>

## Scope
<what was inspected>

## Must Fix

### <Finding>
Finding:
...

Evidence:
...

Recommendation:
...

Impact:
...

## Should Fix

### <Finding>
...

## Optional

### <Finding>
...

## Verdict
<Healthy / Needs Attention / Significant Issues>
```

Omit empty categories.

---

## Core Rules

- Inspect the actual implementation.
- Inspect related dependencies.
- Base findings on evidence.
- Report real complexity/performance concerns.
- Avoid hypothetical problems.
- Keep recommendations concise.
- Never modify code during a review.
- Do not turn style preferences into mandatory fixes.
- End with a clear verdict.
