# A reshape resets the keyboard position, except in the header row

## Status

Accepted (builds on [0003](./0003-the-keyboard-position-is-dom-focus.md))

## Context

A **reshape** is anything that changes which items the table shows or in what order: a sort, a filter, a reload, a
column becoming hidden. TypeScript addresses a body cell by the absolute row index its row carries and by its column
id. An absolute row index names a place in the current view, not an item, so after a reshape it points at whichever
item ended up there, and a gesture would act on that one.

## Decision

**Every reshape resets the position to the default cell.** One rule, no variation per kind of reshape.

**Except in the header row.** A position on a header cell survives a reshape, because the gestures that cause
reshapes live there.

A reset **moves the tab stop**. It moves DOM focus as well only if the table already held focus; otherwise the reset
is invisible, and the user meets it on their next <kbd>Tab</kbd> back in.

When the reshape is a column being hidden:

| Case | Result |
| --- | --- |
| The position is on the header of a surviving column | Unchanged |
| The position is on the header of the hidden column | The neighboring header — the column to the right, or to the left when the hidden one was last |
| Every column is hidden | The no-data placeholder cell, the same stop an empty table uses |
| The position is on a body cell | The default cell, like any other reshape |

## Consequences

- The position does not need to survive on item identity, so the design does not depend on `GetRowKey` or on a
  consumer having set `ItemIdSelector`.
- TypeScript's remembered coordinate exists for exactly one purpose: finding a row that virtualization derendered.
  Everything else is a reset. The .NET side keeps telling JavaScript when a reshape happened
  (`ResetFocusedCellAsync`), because JavaScript cannot tell a row that scrolled out of the render window from a row
  a filter removed.
- A reset moves the tab stop rather than clearing it, so the body never ends up with no tab stop at all.
- The no-data placeholder is also shown when every column is hidden: rows with no cells in them offer nothing else
  that can hold the stop.
- The header exception is why <kbd>Shift</kbd>+<kbd>Enter</kbd> multi-sort keeps working. Without it, each added
  sort level would throw the position out of the header row.
- An editing table that refreshes on commit resets the position after every edit. If that reads badly, the fix is
  to narrow what counts as a reshape for that table, not to make the rule vary per reshape.

## Alternatives considered

- **The position lives as long as its row does.** Rows are keyed by item, so a surviving item keeps its DOM row, and
  a promoted `tabindex` would travel with it across a sort or filter with no code at all. Rejected for three
  reasons. Its behavior depends on whether a consumer set `ItemIdSelector`: without one the row key is the item
  instance, and a reload of fresh instances rebuilds every row, so the same gesture behaves differently in two
  consumers for a reason neither can see. A sort that moves the focused row from the top of the list to the bottom
  drags focus and the viewport with it, because focus is real. And it cannot be stated in one sentence a consumer
  will read.
- **Reset on sort and on a column change, keep on filter.** Defensible — reordering moves everything, filtering
  only removes — but it needs the same header exception, and a consumer cannot predict which of two rules applies to
  which action.
- **Keep the position and hide only the marker.** With the marker driven by `:focus-visible`, a reshape does not
  change how focus arrived, so the marker persists, and hiding it needs an explicit blur. Two mechanisms painting one
  marker is what [0003](./0003-the-keyboard-position-is-dom-focus.md) removes.
