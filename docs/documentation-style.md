# Documentation Style

This document contains rules and recommendations on how to write comments and
documentation (XML doc comments, `.cs`/`.ts` inline comments, JSDoc, README and
ADR files) for this repository.

## Consistent terminology

Pick one term per concept and use it everywhere — comments, XML docs,
identifiers, README and ADR files. Favor consistency and precision across the code base over
locally shorter wording, and prefer spelling a term out over an abbreviation
when the scope is broad or the reader may not have the local context to decode
it.

For example:

| Use            | Instead of        | Notes                                                       |
|----------------|-------------------|-------------------------------------------------------------|
| drag ghost     | ghost             | Use precise wording.                                        |
| .NET side      | server-side       | The application must not necessarily be Blazor server-side. |
| JavaScript     | JS                | Spell it out in prose for cross-scope clarity.              |

These are only examples to illustrate the principle, not a maintained list of
approved terms; when introducing a recurring term, match the wording already
used by the surrounding code base.

## Terminology to avoid

Do not use the following terms — in this repository they collide with other meanings and become ambiguous:

| Avoid | Use instead | Notes                                                |
|-------|-------------|------------------------------------------------------|
| BCL   | .NET        | Possible confusion with "Blazor Components Library". |

## Public API documentation

Applies to `///` on the shipped-package surface — the public and protected
members a consumer sees. A `public` member in an assembly nobody consumes
counts as internal.

Treat the implementation as a blackbox: document the contract, never the
mechanism.

- The **contract** is external even where it looks like implementation: thrown
  exceptions, thread safety, disposal requirements, ordering and timing
  guarantees, observable side effects.
- The **mechanism** is not: data structures, locks, private helpers, caching,
  algorithm names.
- Never restate the signature. No "Gets or sets", no `<param>` echoing the
  parameter name, no `<summary>Disposes.</summary>`.
- For every doc and every part of one, ask: does an external user need this?
  Does it leak internals? Delete what fails either question.
- Where documentation is mandatory and enforced by diagnostic `CS1591`, make
  use of `<inheritdoc/>` where possible / reasonable, otherwise add documentation
  instead.

## Code comments

Covers `//` comments in `.cs`, `.ts` and `.scss`. Not README, ADR or other
markdown.

Default to none. Every comment is a potential code smell — challenge each one
before writing it.

- A comment explaining **what or how** is refactorable: rename, extract,
  restructure until the comment is redundant, then drop it.
- A comment explaining **why** — external constraint, non-obvious ordering,
  workaround — is not exempt from the same test. Ask what could carry it
  instead: a name, a type, a guard, an enum member, a test. `grep` the test
  suite before keeping it; where a test asserts the constraint, the test
  carries it and the comment goes. Drop it too if it is trivial, or if it
  explains a decision no reader would question. Only a reason that no code
  structure can express **and no test can reach** survives — that a browser
  returns a stale width on the first read after an element is unhidden, and
  reports no error for it, is one.
- **Trimming is deletion first.** When a comment fails a check, the question is
  "does this go?", not "can this be shorter?". Rewriting a comment that should
  not exist launders it — it survives the review it should not have survived,
  now harder to spot.
- **A surviving why does not carry its neighbours.** Split it at sentence and
  clause boundaries and run every piece through the checks again on its own. A
  clause naming an identifier, call or value that appears in the lines the
  comment sits on is a restatement and goes, however sound the clause next to
  it is; so is a clause that reads as the next lines read aloud. Shrinking to a
  single clause is the normal outcome. Watch the shapes that smuggle a
  restatement in behind a real reason: "*X, not Y*", "*done before Z*", "*this
  is a K*" — each names the construct directly below it.
- **One name survives: the one that says what the reason is about.** Where the
  line below holds several constructs, the reader has to be told which of them
  the reason belongs to, and only the name can tell them — that is precision,
  not restatement. "*…, using `InvokeAsync()`*" keeps a reason about the
  synchronization context from being read onto the `await` next to it or the
  method it wraps. Keep the name only where dropping it leaves the subject in
  doubt, and keep it inside the reason — "*…, hence we use `InvokeAsync()`
  here*" is a conclusion of its own and falls back under the check above.
- **No history.** State what the code does and why, never what it used to do or
  what a change replaced. "Used to be translucent", "no longer reports this",
  "moved here from X" — git carries all of it, and the comment is wrong the
  next time the code is touched.
- **Every claim is checked.** A comment asserting something about another file,
  another project, or a consuming application gets verified before it is
  written, or it is not written. Unverifiable and wrong cost the same.
- **Say it once, where the decision lives.** A reason that spans files is
  stated at the place that owns it. Elsewhere, restate it in a clause rather
  than sending the reader away. Past roughly three lines, the reason is
  architectural, not local — it belongs in the feature's `README.md`, with
  nothing left behind above the code.
- **A reference is a last resort.** A path, a section name or an ADR number
  drifts silently and nothing fails when it does. In order: state the reason
  inline; failing that, name a symbol a rename would carry with it; only then
  name a file or an ADR, and only where the reader genuinely has to go there —
  a cross-language boundary, or a rule that deliberately lives elsewhere.
  Never as a substitute for a reason that fits in a clause, and never point at
  a place that answers a different question than the one the reader arrived
  with. A reference is the *address* of a reason, never the reason: state what
  this code cannot answer on its own, with the reference as the pointer, not
  instead of one.

## Wording

Applies to comments and XML docs alike. Which term to pick is
`## Consistent terminology` above.

- Full sentences, capitalised and terminated.
- **Plainest word that is still precise.** Covers, not occludes. Pointless,
  not futile. "Able to fail", not "discriminating". A word the reader stops at
  costs more than the syllables it saves. Precision still wins — keep the exact
  word where the plain one is vaguer.
- **Name only things the code has.** A noun for a domain thing is a type,
  member, parameter or CSS class you can `grep`. Where one class lands on two
  kinds of element, name the class and the element separately — an element *of*
  the thing, never a compound noun the code never defines. If `grep` finds the
  term nowhere, you invented it.
- No convention that exists nowhere else in the repository — no ASCII section
  dividers, no banner comments. `grep` first; if it is not already there, it is
  not a convention.
- Shorter and simpler wins — but never at the cost of precision.
- A comment you touch, you own: fix or delete a stale comment in code you edit.
  A wrong comment is worse than none.
