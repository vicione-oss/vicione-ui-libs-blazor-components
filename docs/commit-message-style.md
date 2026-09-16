# Commit Message Style

This document contains rules and recommendations on how to write commit messages in this repository.

Commit messages created by tools, such as GitLab's `Resolve "..."` merge commits or Renovate's `Update ...` commits, keep their generated form.

## Title

When a change belongs to one component or area, start the title with its name followed by a comma, then describe the change in imperative mood.
Otherwise use the imperative sentence on its own.

Examples:

```text
Button, add Busy state with selectable indication
Navigation, fix selected entry color
Set release date for 6.1.0
```

## Description

Every commit needs a description. Separate it from the title by a blank line and explain in a few lines **why** the change was made and what it means.
Leave out a list of touched files, the diff already shows them.

Example:

```text
Sweep animation, make the ring mixin private

The ring only exists as the surface the sweep paints its arc on, so it has no
use outside the sweep animation. Prefixing it with an underscore makes Sass
reject any access from another module, the same way the inputs mixins keep
their helpers private.
```

## Trailers

Trailers such as `Co-Authored-By` go last, separated from the description by a blank line.
