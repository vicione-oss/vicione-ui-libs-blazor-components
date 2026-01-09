# CSS Guidelines

This document contains rules and recommendations on how to write CSS code for components.

## z-index

Before using the `z-index` property it should be made sure that no other way leads to the same result.
In most cases a reordering or restructuring of other DOM elements can lead to the same result without using it.
Using this property shall be **avoided whenever possible**.
