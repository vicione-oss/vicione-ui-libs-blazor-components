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
