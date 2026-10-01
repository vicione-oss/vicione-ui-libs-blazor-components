# The table body is one tab stop, and Enter activates and enters a cell

## Status

Accepted (builds on [0003](./0003-the-keyboard-position-is-dom-focus.md))

## Context

The table used to have a tab stop per column header, one for the scroll container, and one for every control a
cell held, so the number of stops grew with both the column count and the row count. In a small `SimpleTable` it took
13 <kbd>Tab</kbd> presses to reach the select-all checkbox, and the arrow keys did nothing at any of those stops,
because the key handler only ran while the scroll container itself was the event target.

<kbd>Enter</kbd> did nothing on most cells: `CellActivated` is per-column and opt-in. A consumer that swaps an
editor into a cell from `CellActivated` was already implementing "go into the cell" on its own, because the table
did not offer it. There was no <kbd>Escape</kbd> handling.

## Decision

### The tab order

| Gesture | Result |
| --- | --- |
| <kbd>Tab</kbd> from before the table | The header region's own controls, each a natural tab stop (they are real `<button>`s) |
| <kbd>Tab</kbd> again | The table body, on the cell the position was last on, else the default cell. The marker shows, because the arrival is by keyboard |
| <kbd>Tab</kbd> again | Out of the table |
| <kbd>Shift</kbd>+<kbd>Tab</kbd> back in | The same cell the position was on |

The **default cell** is the first cell in DOM order of the first data row, which is pin order — the leftmost cell
the user actually sees.

**Column headers, rows and the controls inside cells are not tab stops.**

### How the one stop is held

A **tab boundary** sits either side of the scroll container: two invisible one-pixel elements, each carrying a
constant `tabindex="0"`. They are the first and last things the browser can put focus on in the table's region,
so every arrival by <kbd>Tab</kbd> crosses one. Focus landing on a boundary never stays. Arriving from outside the
table, it is sent to the focused cell; arriving from inside, it is let through, and the browser's own
<kbd>Tab</kbd> carries on past everything the table contains. Which of the two happened is read from the
arrival's `relatedTarget`.

Inside the table, <kbd>Tab</kbd> on a cell is claimed and hands focus to the trailing boundary. Between the two,
nothing the browser does can land focus on a control inside a cell.

**The table writes no `tabindex` onto content it did not render.** A consumer's button in a cell stays exactly as
they wrote it, and is simply never reached by <kbd>Tab</kbd>, because focus is past it before the browser looks.
What a consumer's `tabindex` still decides is the walk inside an entered cell, below: `-1` means "skip me" there as
well.

The boundary sends focus to the focused cell, else the default cell, else the first header cell. A table with no
columns registered has no cell at all, and a boundary then holds focus until the next press; so does one reached
before the module has attached. Both are accepted.

### Movement

Arrow keys move the position one cell at a time. ↑ from the first data row reaches the matching **header cell**
of that column; a further ↑ does not move. The other three edges behave the same way: movement never takes focus
out of the table, and <kbd>Tab</kbd> is the only way out.

### A body cell

Two terms describe what happens to a body cell, because they are two steps that do not always come together:

- **Activating** a cell is pressing <kbd>Enter</kbd> while the cell has the focus. It raises the column's
  `CellActivated`, and nothing else raises it.
- An **entered cell** is a cell the focus is inside, and that is the whole definition. Nothing else is tracked, so
  there is no second answer to keep in step with the first.

A cell is entered in one of two ways, both deliberate. Activating it is one: the table responds by moving focus
into the cell's first focusable content. Clicking its content is the other, and it enters the cell without
activating it. A cell holding nothing focusable is activated but never entered: focus stays on the cell, and the
next <kbd>Tab</kbd> leaves the table — the same place a cell with controls ends up once the last of them is walked
past.

| Gesture | Result |
| --- | --- |
| <kbd>Enter</kbd> | Activates the cell, then enters it if it holds focusable content |
| <kbd>Tab</kbd> inside an entered cell | Walks that cell's controls; leaving the last one returns to the cell, and <kbd>Shift</kbd>+<kbd>Tab</kbd> off the first does the same |
| <kbd>Escape</kbd> inside an entered cell | Returns to the cell |
| <kbd>Escape</kbd> with the cell itself focused | Nothing |

`CellActivated` stays per-column and opt-in, and entering after an activation is **not cancelable**. Focus moves
in after the render the callback caused, so content a consumer creates in response to the activation is focusable
by the time focus arrives. A column with no `CellActivated` handler causes no render, so focus moves into what the
cell already holds straight away.

The walk skips a control with its own `tabindex="-1"`, so a `SpinEdit` in a cell walks to its input and back
rather than through its spin buttons. A <kbd>Tab</kbd> from somewhere in the cell the walk cannot place — a `-1`
control reached by clicking it — is claimed all the same and returns focus to the cell, so nothing leaves a cell
sideways.

<kbd>Escape</kbd> is handled at the cell, not at the table, and on key-up rather than key-down. At the cell, the
table only sees what bubbles past the popups, context menus and filter editors that already answer the key. On
key-up, because an editor inside the cell reverts what was typed on its own key-up, which moving focus out on
key-down would take away from it.

### A header cell

<kbd>Enter</kbd> sorts by the column and <kbd>Shift</kbd>+<kbd>Enter</kbd> adds it as a sort level, unchanged.
<kbd>Space</kbd> does nothing. The column's filter panel opens with <kbd>Alt</kbd>+<kbd>↓</kbd>. **<kbd>Enter</kbd>'s
body meaning does not extend to the header row** — on a header it is the sort gesture, so no key takes the keyboard
into a header's content.

A header cell is still entered by the same rule as any other cell, because that rule is only about where focus is:
clicking its content puts focus inside it, and <kbd>Tab</kbd> then walks that header's controls. One keyboard route
in exists: <kbd>Alt</kbd>+<kbd>↓</kbd> opens the filter panel, and the panel counts as part of its header cell, so
the walk carries the user through the editor and its buttons and back to the header. `Popup` and `ContextMenu`
render through `PopupRoot` and are never inside a cell, so nothing else is affected.

**Other focusable content inside a header cell has no keyboard route in, and that is deliberate for now.** The known
instance is the select-all `CheckBox` of `SimpleTableSelectColumn`, rendered when `HasSelectAllHeader` is true, which
used to be reachable by <kbd>Tab</kbd>. It is only enabled under `SelectionMode.Multiple`, with no
`ItemSelectionAllowed`, not `DisplayOnly`, and with rows left by the filter; `AdvancedTableSelectColumn` renders no
select-all at all. Where it is enabled, it is now **mouse-only**, and since <kbd>Ctrl</kbd>+<kbd>A</kbd> is not part of the key
set, there is no keyboard route to select-all at all. This is accepted as not a required feature at this point. It is
reopened by a requirement for it, not by a sighting. The candidates for that day:

- A dedicated entry key, <kbd>F2</kbd> — which the WAI-ARIA pattern pairs with <kbd>Enter</kbd> for exactly this —
  applied to every cell, header and body alike.
- <kbd>Space</kbd> acting on a header cell's control the way it acts on a row in the body.
- A single tab stop kept for the checkbox as an exception.

The problem is wider than the checkbox: a column's header is a consumer-supplied fragment, so any column can put
focusable content there.

### The no-data placeholder

It holds the tab stop and nothing else: no marker, and arrows, <kbd>Space</kbd> and <kbd>Enter</kbd> do nothing on
it. ↑ still reaches the header row, so sorting and filtering stay available on an empty table.

Reasons:

- **One stop is unanimous in the field.** The WAI-ARIA APG: *"Only one of the focusable elements contained by the
  grid is included in the page tab sequence."* Telerik makes the body one stop and keeps toolbar, group panel and
  pager as separate page-level stops, which is the split adopted here. DxGrid reaches the same result with three
  *navigation areas*.
- **The header row is a row, not a toolbar.** Reaching header cells by arrow puts them in the same coordinate space
  as the data at no cost, because a header cell under roving tabindex is genuinely focused and its existing sort
  handler keeps working. It also keeps <kbd>Shift</kbd>+<kbd>Enter</kbd> multi-sort usable, which needs the position
  to stay in the header row across a sort.
- **The header region needs no work.** Its action buttons are already real `<button>`s, so they are already a
  bounded set of natural tab stops.
- **Entering is the table's response to an activation, so the existing callback is reused.** `CellActivated` was
  documented as *"Left unset, Enter on a cell of this column does nothing"*, so giving <kbd>Enter</kbd> a meaning
  there is additive. A consumer that swaps an editor into a cell from `CellActivated` already treats activating as
  the way into the cell. Reusing the callback rather than adding a second event for the same key avoids two hooks
  with an ordering question between them.
- **Walking the controls of an entered cell is one rule for every cell.** A cell with one control returns to the
  cell on the first press; a cell with two does not strand the second. DxGrid states the same rule: *"press Enter
  to focus the first object, then press Tab/Shift+Tab to navigate between objects. When leaving the last nested
  object, navigation automatically returns to the previous level."* This library already marks secondary buttons
  `tabindex="-1"` — `SpinEdit`'s spin buttons, `SearchBox`'s clear button — so most cells return straight away.

## Consequences

- The marker appears on <kbd>Tab</kbd> arrival, not only after the first arrow press. A mouse click still shows
  nothing.
- The row select checkbox is no longer a tab stop. It is reached by activating its cell: the select column wires no
  `CellActivated`, so nothing is raised and entering the cell lands focus on the checkbox, and <kbd>Space</kbd> is then the checkbox's own key.
- The boundaries are two focusable elements the table did not have before. Every tab-order test walks through them,
  so what those tests assert is that focus never comes to rest on one.
- A consumer who wants a cell's control back in the page tab sequence has no way to ask for it. The field does not
  offer one either — Telerik's answer to the same request is a hand-written template column — so one can be added
  the day a consumer asks.
- Inside a dialog none of this is observable until the dialog gives focus to something. That fix belongs to
  `Popup`; on ordinary pages this works on its own.
- An editing table that wants <kbd>Tab</kbd> to commit and move to the next cell gets it as an opt-in on top of this
  rule, not instead of it.
- <kbd>Home</kbd>, <kbd>End</kbd>, <kbd>PageUp</kbd>, <kbd>PageDown</kbd> and <kbd>Ctrl</kbd>+<kbd>A</kbd> stay out
  of the key set. Adding them is a stakeholder question, not a consequence of this.

## Alternatives considered

- **A tab stop per column and per row.** The previous state; a table you cannot get past with the keyboard.
- **<kbd>Tab</kbd> as a movement key between cells**, as AG Grid does. Rejected: AG Grid had to add an API to let
  people leave its own grid.
- **<kbd>Tab</kbd> inside a cell commits and moves to the next cell**, as Telerik and AG Grid do in edit mode.
  Rejected as the default: it makes <kbd>Tab</kbd> mean two different things depending on where focus is.
- **One roving group for the header region's buttons.** Rejected: it would make every button but one unreachable
  until arrow movement within the region exists.
- **Renaming `CellActivated` to `CellEntered` or `CellEntering`.** Rejected: the callback is not raised on every
  entry, because a click into a cell enters it without activating it, and a consumer reading either name would
  expect it there. It is also raised on cells that are never entered, because they hold nothing focusable. An `-ing`
  name, including `CellActivating`, reads as cancelable in .NET, as `FormClosing`, `Validating` and
  `LocationChanging` are, and entering is not. And a rename breaks every consumer binding without a compile error,
  because a Razor attribute naming a parameter that no longer exists only fails at render. `CellEntered` becomes
  the right name only if activating and entering are made one gesture.
- **A cancelable entry**, through a `Handled` flag. Rejected: it needs a round trip before focus moves, which is a
  visible delay and a movement round trip [0003](./0003-the-keyboard-position-is-dom-focus.md) rules out. A consumer
  who must keep the keyboard out of a cell renders nothing focusable in it, or moves focus themselves from the
  handler.
- **A table-level <kbd>Escape</kbd>** that clears the position or leaves the table. Rejected: it competes with every
  popup, dialog and context menu on the page, and "<kbd>Escape</kbd> closes the nearest thing" is the behavior users
  already have.
- **The table writes `tabindex="-1"` onto cell content**, or `CheckBox` gains a `TabStop` parameter. Rejected: the
  boundaries make it unnecessary, and it would mean the table changing markup a consumer wrote.
