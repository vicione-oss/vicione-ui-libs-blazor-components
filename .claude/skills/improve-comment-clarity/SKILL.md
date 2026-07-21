---
name: improve-comment-clarity
description: >-
  Write and rework code comments and documentation (XML doc comments, `.cs`/`.ts`
  inline comments, JSDoc) so they explain intent rather than restate code. Use
  this whenever adding new comments, or when a prompt asks to review, clean up,
  clarify, simplify, or improve comments/documentation — and automatically
  whenever you change code and its existing comments need updating, because a
  comment that no longer matches the code it describes must be corrected. Also
  use it when a comment reads as too technical, jargon-heavy, vague, or stale.
---

# Improve comment clarity

Guidance for writing new comments and reworking existing ones, in any language
(`.cs`, `.ts`, XML doc comments, JSDoc, etc.). Reworking comments is expected —
and necessary — when you change the code they describe.

## Principles

- **Explain the *why*, not the *what*.** The code already shows what it does. A
  good comment captures intent, rationale, and non-obvious consequences. Do not
  restate the code in prose.
- **Make it understandable from the start.** A reader should grasp the point
  without already knowing the implementation. Introduce a concept before using
  its jargon.
- **Add a concrete example for non-obvious concepts.** A tiny example (e.g. "a
  fraction `0..1`, `0.5` = center") makes an abstract explanation click far
  faster than a formula restatement.
- **Do not comment the obvious.** Self-explanatory code, trivial DTOs, and simple
  getters/setters need no comment. Silence is better than noise.
- **Keep comments truthful and current.** When you change code, update or delete
  any comment it invalidates. A stale comment is worse than none.
- **Do not duplicate documentation that lives elsewhere.** If a decision or
  pattern is already captured in an ADR (`.adr/*.md`), a `docs/*` guide, or
  another authoritative source, reference it instead of re-explaining it inline.
  Prefer a short pointer over a long duplicated rationale.
- **Preserve accuracy of public API docs.** When improving public XML doc
  comments for clarity, keep the described contract exact.
- **Match the surrounding style.** Follow the tone, format, and density of nearby
  comments rather than introducing a new style.
- **Follow the repository documentation style.** Use consistent, correct
  terminology, matching the wording already used by the surrounding code base.
  The preferred terms live in
  [`docs/documentation-style.md`](/docs/documentation-style.md) — follow it
  rather than re-deciding wording here.

## Workflow for a review pass

When asked to review comments across a file, folder, or changeset:

1. Enumerate the in-scope source files (exclude generated files such as
   `*.cs.js` / `*.cs.ts`, and — unless asked otherwise — samples and tests).
2. Read each file and judge every comment against the principles above.
3. Edit only where clarity is genuinely lacking; leave already-clear comments
   untouched. Note (do not silently skip) anything odd you find, e.g. empty or
   stray files, or genuine `todo` markers.
4. Do not treat a genuine `todo`/decision note as a clarity problem; leave it
   unless explicitly told to remove it.
5. Build to validate that comment/doc-only edits did not break anything.

## Sweeping a wording change

When applying a find-and-replace across comments and docs (for example to align
with the terminology in
[`docs/documentation-style.md`](/docs/documentation-style.md)), change **prose
only**. Never rewrite anything that is not human-readable text:

- code identifiers (for example `GetJsModule`, `IJSRuntime`, `JSInvokable`,
  `IJSObjectReference`),
- import paths and `.js`/`.ts` file references,
- external URLs,
- package identifiers (for example `@types/blazor__javascript-interop`).

Changing those breaks links or code; only the surrounding text should change.
Build afterward to validate that the edits did not break anything.
