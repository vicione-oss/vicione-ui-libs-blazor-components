---
name: delete-orphaned-css
description: >-
  Find and delete orphaned Blazor scoped CSS files — `*.razor.css` files that
  have no adjacent `.razor` component next to them. Use this whenever the build
  reports BLAZOR102 "The scoped css file ... was defined but no associated razor
  component or view was found for it", or when a prompt asks to remove, clean up,
  or delete orphaned/dangling/unused scoped CSS files.
---

# Delete orphaned scoped CSS

Guidance for removing Blazor scoped CSS files that no longer have a matching
component. A scoped CSS file `Foo.razor.css` is expected to sit next to a
`Foo.razor` component. When it has none, the build emits **BLAZOR102**:

> The scoped css file '...' was defined but no associated razor component or
> view was found for it.

## How orphans arise

- **A component was renamed or deleted** and its `.razor.css` was left behind.
  The orphan sits in the working tree and every build reports it until the file
  is removed.
- **A checkout stranded generated CSS.** Scoped CSS is compiled from a
  `.razor.scss` sibling and is gitignored, so it outlives the commit it was
  built for. Moving to a commit where that component does not exist — a branch
  switch, a rebase, a bisect — leaves its CSS behind, and the build fails on a
  file the new commit never had.

Both raise the same error and take the same fix. The second is worth
recognizing while rebasing or bisecting: the error names components the commit
under test never touched, which reads as if that commit were broken. Run the
scan below before trusting a build that follows a checkout.

## Why not rely on the build output alone

The build caps the number of reported errors (for example 50 at a time), so a
single build run may surface only a subset of the orphaned files. Always do a
workspace-wide scan to find every orphan in one pass instead of removing them
one build at a time.

## Workflow

1. Scan the whole workspace for `*.razor.css` files whose sibling `.razor` file
   does not exist. From the workspace root, using pwsh:

   ```powershell
   Get-ChildItem -Recurse -Filter *.razor.css |
     Where-Object { -not (Test-Path ($_.FullName -replace '\.css$','')) } |
     ForEach-Object { $_.FullName }
   ```

   Each result is an orphaned scoped CSS file: the check strips the trailing
   `.css` and confirms the underlying `.razor` component is missing.

2. Delete each reported file using the workspace file-removal tool so project
   references are cleaned up too (not just the file on disk).

3. Build to validate that the BLAZOR102 errors are gone and nothing else broke.

## Scope discipline

- Only remove `.razor.css` files that have **no** adjacent `.razor` component.
  Never delete a scoped CSS file whose component still exists.
