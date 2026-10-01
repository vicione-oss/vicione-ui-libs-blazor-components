# The keyboard position is DOM focus

## Status

Accepted

## Context

A keyboard user of `AdvancedTable` / `SimpleTable` is always on one cell, the **focused cell**. The table used to
represent that position twice: the browser's focus sat on the scroll container, and TypeScript kept a separate
cell coordinate, the *lead*, next to it. The two were synchronized in one direction only, so every fix that picked
a behavior for one of them silently decided it for the other.

The marker for the position has to appear when the keyboard is used — by <kbd>Tab</kbd> into the table or by an
arrow move — and never for a mouse-only user.

Under Interactive Server, a keypress that needs a round trip is a keypress the user waits for.

## Decision

**The table has one keyboard position, and it is the browser's focus, managed by roving tabindex.** The focused
cell carries `tabindex="0"`, every other cell `tabindex="-1"`. Moving the position moves the `0` and calls
`focus()` on the new cell. Nothing else tracks a position.

The position is a **cell**, in the body or in the header row. Row gestures act on its row. An empty table's
position is the no-data placeholder cell, so the table stays reachable when it has no rows.

**Ownership of the attribute is split so that a re-render cannot disturb it.** The .NET side renders a constant
`tabindex="-1"` on every body and header cell. TypeScript promotes exactly one of them to `0`. Because the value
the .NET side renders never changes, Blazor's diff never writes the attribute, and the promotion survives any
re-render. The .NET side never renders a `tabindex` whose value depends on the position — it does not know it.

The scroll container is not a tab stop and carries no `tabindex`. The table's tab order is bounded by a pair of
elements either side of it instead (see [0004](./0004-the-table-body-is-one-tab-stop.md)), whose `tabindex="0"`
is constant too. **Exactly one `tabindex` value in the table is dynamic** — the promoted `0` — and TypeScript is
its only writer. TypeScript writes `tabindex` on the table's own cells and nowhere else; content a consumer put in
a cell keeps whatever the consumer gave it.

**Movement, the promotion and the `focus()` calls happen in TypeScript.** The .NET side is told about a position
only when a consumer-visible operation happens — a selection gesture, an activation — never per keypress.

**Focus is recorded in one direction only.** The `focusin` listener records the position and moves the `0`, but
never calls `focus()`. Every other path — arrow movement, a reset, the return of a row virtualization derendered —
only calls `focus()` and leaves the recording to the `focusin` that follows. With one writer per direction, the two
cannot chase each other.

**The marker is `td:focus-visible` and `th:focus-visible`, and nothing else.** `:focus` is never styled.

Reasons:

- **One position cannot disagree with itself.** Representing the position as focus removes the class of defect
  the lead caused, rather than one member of it.
- **It is what the field does.** Of five shipped grids with a keyboard model, four use roving tabindex: the
  WAI-ARIA APG grid pattern's own default, Telerik (*"a single tab stop component"* using *"roving tab index"*),
  AG Grid and Syncfusion.
- **`:focus-visible` is exactly the required rule, implemented by the browser.** It matches on keyboard focus,
  and on scripted focus when the user's most recent interaction was a keypress, which covers arrow movement. It
  does not match on a pointer click. A hand-maintained flag doing the same job would be a second mechanism that
  can disagree with it.
- **Real focus is required by the rest of the design.** Header cells carry their own sort gesture, the header
  region's action buttons are real `<button>`s, and a control inside an entered cell has to hand focus back to
  something.
- **Movement stays on the client**, for the round-trip reason above. DevExpress states the same choice and the
  same reason for its own grid.

## Consequences

- The word *lead* is retired. Keeping a separate noun for the position invites the split back; code says
  *focused cell*.
- A key that does not move the position does not show the marker. Pressing <kbd>Space</kbd> after clicking a cell
  selects the row without a marker; the row turning selected is that gesture's feedback.
- A row virtualization derenders takes DOM focus with it, and the browser drops it on `document.body`, where no
  key reaches the table. So when the focused row leaves the render window, the tab stop and the orphaned focus
  move to a rendered cell while the position keeps its coordinates. The next vertical move scrolls the row back
  and focus returns with it.
- The browser scrolls on focus as well, knowing nothing of the sticky header or of pinned columns. The table's own
  corrective scroll runs from `focusin`, after the browser's, and is what keeps the focused cell clear of both.
- The marker cannot be asserted in bUnit — `:focus-visible` is a browser heuristic. What bUnit can assert is that
  the .NET side renders a constant `-1` on every cell, which is what keeps the ownership split intact.
- A public API for the position would be a projection of something the browser already tracks. Whether one ships
  is a separate decision; if it does, it reports asynchronously and at row level.

## Alternatives considered

- **Active descendant** — focus stays on one container element, and `aria-activedescendant` names the current
  cell. Rejected: the one shipped product using it, Radzen, names a **row** as its active descendant and keeps a
  separate internal cell index to express a cell — two positions, which is the state being left. It would also
  have had to run roving tabindex alongside itself for the header cells, the header region's buttons and the
  controls inside cells, leaving a virtual/real seam in the middle of one component.
- **Movement handled on the .NET side.** Rejected: under Interactive Server every arrow press would cost a round
  trip and a body-wide render diff, and an auto-repeating arrow would flood the circuit. Radzen's .NET key handler
  is the counterexample.
