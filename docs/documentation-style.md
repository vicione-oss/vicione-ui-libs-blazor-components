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
