---
description: Review and rework code comments and documentation so they explain intent rather than restate code, without changing behavior.
---

Review and improve the clarity of code comments and documentation (XML doc
comments, `.cs`/`.ts` inline comments, JSDoc) so they explain the *why* rather
than restate the code. Rework only comments and documentation — DO NOT change
behavior.

Scope: `$ARGUMENTS` (files, folder, or changeset to review; leave empty to
review the currently open file).

Apply the `improve-comment-clarity` skill at
`.claude/skills/improve-comment-clarity/SKILL.md`: read that file first and
follow its principles and workflow exactly. The skill is the single source of
truth for what to edit, what to exclude, and how to validate — do not improvise
beyond it.
