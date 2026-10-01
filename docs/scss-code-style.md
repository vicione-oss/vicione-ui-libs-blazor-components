# SCSS Code Style

This document contains rules and recommendations on how to write SCSS code for components.

## z-index

Before using the `z-index` property it should be made sure that no other way leads to the same result.
In most cases a reordering or restructuring of other DOM elements can lead to the same result without using it.
Using this property shall be **avoided whenever possible**.

## Nested structuring

The SCSS code shall depict the DOM structure through nesting.
Selectors should be nested to mirror the hierarchy of the elements they style rather than being declared as flat, sibling rules.

Example:

Before:

```scss
.demo { ... }
.layer { ... }
```

After:

```scss
.demo {
    .layer {
        ...
    }
}
```

## Specificity

CSS isolation narrows a selector before you do.
The scope identifier the compiler appends confines a rule to the markup of the component it was written for, so a selector that stays within that markup is already unique.
A child combinator added there earns nothing and shall be left out.

Narrow a selector by hand only where isolation does not reach, which is the right-hand side of `::deep`.
Everything past `::deep` carries no scope identifier and matches at any depth below the scoped element — markup a consumer passes into the component, and any other component that happens to use the same class name.
Use the child combinator `>` there wherever the element being styled is a direct child.
A combinator costs no specificity; it narrows *what* a rule matches, which is what keeps the rule off markup that merely reuses a class name.

Example:

Before:

```scss
.sidebar {
    // Also matches a table's column resize handles when a table is placed in the sidebar.
    ::deep .resize-handle { ... }
}
```

After:

```scss
.sidebar {
    > ::deep .resize-handle { ... }
}
```

Nesting depth shall not be relied on to out-specify another rule.
The scope identifier is appended to the last element of a selector only — or, where `::deep` is used, to the last element before it — so `.a .b .c` compiles to `.a .b .c[b-abc123]`.
A rule that wins only because it sits deep in a nesting chain loses that advantage the moment its markup moves into a component of its own, and nothing fails at build time when it does.
Give the rule its own component's scope instead, and narrow the competing selector.
