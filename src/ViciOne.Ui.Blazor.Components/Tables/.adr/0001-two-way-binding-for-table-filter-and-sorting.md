# Two-way binding for table filter and sorting

## Status

Accepted

## Context

`AdvancedTable` and `SimpleTable` render filter panels and sort indicators, so the user can change how rows
are narrowed and ordered. Consumers need three things from that state, in any combination:

- **Observe it** — mirror it into a URL, a settings store, telemetry.
- **Drive it** — restore a saved view, apply a preset, clear from a toolbar button, push from a debounce timer
  or a server message.
- **Ignore it** — drop the table on a page and let the user filter, with no consumer code at all.

Two properties constrain any shape serving all three:

- **The state types compare by identity only.** `FilterState` and `SortingState` have no content equality;
  their `With…`/`Without…` helpers return the same instance when nothing changes and a shared `Empty` when the
  last entry goes. Identity is the only available "did anything change?" signal.
- **The table must work unbound.** Since ignoring the state is a first-class case, the applied state cannot
  depend on being handed back by a consumer.

## Decision

Expose filter and sorting as ordinary two-way bindings — `FilterState` / `FilterStateChanged` and
`SortingState` / `SortingStateChanged` — each with an imperative setter alongside (`SetFilterStateAsync`,
`SetSortingStateAsync`). No separate member for a starting value.

The table **owns** its applied state; the binding is a channel into and out of it:

- an inbound parameter value is a *command*, applied when its instance is one the table has not already seen
  and is not its own value coming back;
- a stable instance therefore behaves as a starting value. No member carries an `Initial` prefix: a name
  promising a starting value would misdescribe a parameter honored for the table's whole lifetime;
- no parameter means the empty state;
- the event reports every change of the applied state, so a consumer who sets no parameter can still observe;
- the setter serves what a binding expresses badly — a "clear all" button, a preset reset — and callers not in
  a render at all.

Binding and setter are peers. A consumer may use either, both, or neither.

## Consequences

- **Driving the table needs no `@ref`.** `@bind-FilterState` covers the common cases.
- **"Initial value" is a usage pattern, not a member** — hand over an instance that never changes. Sound only
  because the state types compare by identity.
- **Correctness depends on the consumer's instance discipline.** Building a content-equal but distinct value
  on every render makes the table react on every render; with a binding that is a non-terminating loop.
  Guarding it would require the content equality the state types lack, so it is documented as misuse.
- **The table may disagree with a bound parent, by design.** A parent that ignores the event holds a stale
  value; the table keeps working. This is the price of working unbound.
- **The table can rewrite the parent's bound field.** A value naming a column the table does not show is
  reduced, and the reduced value is reported — so a binding writes it back, editing a persisted view.
- **`SimpleTable` has no state to maintain.** It forwards the parameter instance and re-raises the event.

## Seams left open (deliberately non-breaking)

- **Content comparison instead of identity.** An optional `IEqualityComparer<FilterState>` parameter would let
  a consumer decide what "changed" means, defaulting to identity. It would close the fresh-instance-per-render
  footgun for consumers who opt in, at their own cost. Additive: the guards move from `ReferenceEquals` to the
  comparer, nothing else shifts. `ItemIdSelector` / `ItemIdComparer` on this component are the precedent for
  handing identity semantics to the consumer.
- **`UpdateKey` for forcing a re-apply.** The setter is idempotent, so a consumer cannot re-push a value the
  table already holds — re-applying `Empty` when the state is already `Empty` does nothing. An `UpdateKey`
  parameter, ORed into the guard, forces the command through when the key changes even though the value did
  not. The library already has this pattern: [`IHasUpdateKey`](../../Interfaces/IHasUpdateKey.cs), implemented
  by `ComboBox` and the `PropertyGrid` editors, compared in the same `OnParametersSet` guard position. Not
  built now — no consumer has needed it, and a forced re-apply is close enough to "reload the data" that the
  two should be designed together.

## Alternatives considered

- **A seed parameter read once, plus a change event, plus an `@ref`-only setter.** Serves observe and ignore
  cleanly, and sidesteps identity entirely by reading the parameter once. Rejected on the driving case: a
  parameter that accepts a value then silently refuses further ones has no honest name, and requiring an
  `@ref` for the most ordinary operation taxes every consumer who wants to drive the table.
- **Binding only, no setter.** Rejected: a caller outside a render — debounce timer, server push — has no
  parameter to set, and the setter is the only channel that can re-deliver a value the parameter channel has
  already seen.
- **Fully controlled: the consumer owns the state, the table renders what it is given.** Cleanest in the
  abstract, but ignoring the state is a first-class case — a fully controlled table shows filter UI that does
  nothing until both halves are wired up, so every consumer pays for the feature.
- **Content equality on the state types themselves.** Removes the footgun, but puts a walk over a filter list
  of unknown size on every render and changes shared value types for one consumer's parameter-passing style.
  The comparer seam above is the opt-in form of this.
