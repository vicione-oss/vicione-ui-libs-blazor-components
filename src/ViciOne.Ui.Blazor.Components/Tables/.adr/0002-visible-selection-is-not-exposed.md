# The visible selection is not exposed

## Status

Accepted

## Context

The **visible selection** is the set of selected rows that pass the active filter — not what is rendered, so a
selected row outside the loaded virtualization window still belongs to it. It exists as a separate set because
the selection survives filtering: the table never drops a selected row a filter hid. With a filter active,
deleting "the selection" and deleting "the part the filter leaves" are two different outcomes.

A consumer cannot compute the second set. It owns the selection and is told the applied filter state, but not
which rows pass that filter: `AdvancedTable` does not filter — its items provider does, possibly inside a query
whose full result the .NET side never sees. The provider returns one page plus a total count, so the table holds
the current window and a number; under `TableLoadingMode.All` that window is the whole matching set, under
`TableLoadingMode.Virtualize` it is not. Even where a consumer could evaluate the criteria itself, its
intersection would compare rows by default equality while the table uses the comparer from `ItemIdSelector`.

`SimpleTable` is unaffected: it owns its data and materializes the full matching set to filter, count and page it
anyway.

## Decision

**`AdvancedTable` exposes no visible selection.** Two rules stand behind it:

- **Nothing that works in only one loading mode.** A value present under `TableLoadingMode.All` and absent — or
  quietly different — under `TableLoadingMode.Virtualize` would let a performance setting change which rows an
  action hits.
- **The table does not guess.** The value drives destructive actions, so the permissive failure is the one that
  matters: a filter-hidden row reported as visible, then deleted. An absent readout costs work, a wrong one costs
  rows.

A capability, not a property of the design — revisit when a consumer actually has a provider-backed table and an
action that must respect the filter. The first alternative is the starting point for that conversation.

## Consequences

- Filter-scoped actions over provider-backed data are solved outside this component: use the in-memory table and
  hold every row on the .NET side, or scope the action in the backend, which knows the filter criteria already.
- With no filter active nothing is lost; the gap is exactly "a filter is active and an action must respect it".
- Adding it later is additive on the table's surface, but not on the provider's — the uniform version needs a
  further responsibility there, a breaking change, cheapest while that interface is not yet treated as stable.

## Alternatives considered

- **The items provider answers the question** — handed candidate row identities and the active filter, it returns
  which of them the filter leaves; the table intersects with its own comparer and pushes the result. The only
  uniform shape, and its cost scales with the selection rather than the source. Rejected on complexity (a second
  query path in every provider, plus an asynchronous side channel whose answer arrives *after* the notifications
  it belongs with, so read-together consistency becomes staleness handling here and a discard rule in every
  consumer), on cost (a round trip per settled change, parameterized by the selection itself, so chunking around
  request-size limits becomes the provider's problem), and on there being no consumer asking — speculative
  required surface on a public interface cannot be withdrawn.
- **Answer only where the whole matching set happens to be loaded.** Exact and free, but mode-dependent; see
  *Decision*.
- **Evaluate per-filter predicates on the .NET side against the selected rows.** No fetch, cost proportional to
  the selection. Rejected: all-or-nothing, and one predicate-less filter makes the answer too permissive in the
  case that drives deletions; it cannot express criteria that are not row-local, such as a ranking or a "top N";
  and it judges rows by the instances the table holds, which may be stale.
- **Leave it to the consumer.** Rejected on identity — default equality against the `ItemIdSelector` comparer,
  disagreeing silently once one is supplied — and because every consumer would re-derive one library-level rule.
