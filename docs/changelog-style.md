# Changelog Style

This document contains rules on how to write entries in `CHANGELOG.md`.

## Structure

Each version has one section per package, and each change is one list item in the section of the package it belongs to.

## Entries

Start an entry with the name of the component or area it belongs to, followed by a comma and a short description of the change.

```markdown
- `SpinEdit`, fixed unintended value changes by the mouse wheel
```

When a component has more than one entry in the same version, list its name once and put each change in a nested list item, starting with a capital letter.

```markdown
- `CheckBox`
  - Prevent clicks from bubbling up to parent component
  - Fixed the check mark and indeterminate icons overflowing the box
```
