---
description: Find and delete orphaned Blazor scoped CSS files (*.razor.css with no matching .razor component) that trigger BLAZOR102 build errors.
---

Delete all orphaned Blazor scoped CSS files in the workspace — `*.razor.css`
files that have no adjacent `.razor` component and therefore trigger the
BLAZOR102 "was defined but no associated razor component or view was found for
it" build error.

Apply the `delete-orphaned-css` skill at
`.claude/skills/delete-orphaned-css/SKILL.md`: read that file first and follow
its workflow exactly. The skill is the single source of truth for the scan,
removal, and validation steps — do not improvise beyond it.
