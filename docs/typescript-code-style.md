# TypeScript Code Style

This document contains rules and recommendations on how to write TypeScript code for components.

## Generated `.js` files

When working on `.ts` files living in a folder of a `.csproj`, you need to keep attention to the following files to get the orchestration of the folder / file structure for generated `.js` files right.

Only touch these files when you **add, rename, remove, or re-target** a `.ts` file or an import specifier. A pure in-file logic change requires no updates here.

| File      | What to do                                                                                                                                      |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| `.csproj` | Maintain MSBuild items named `GeneratedStaticWebAsset` to ensure generated `.js` files are included as static web assets in the Blazor project. |

## Values passed from .NET

Blazor passes a .NET `null` as `null`, never as `undefined`. When a function or
constructor called from .NET receives a value that can be `null` on the .NET
side, type it as `BlazorNullable<T>` from `Scripts/BlazorNullable.ts`, and
handle the missing value with `??` in the body. A default parameter only
replaces `undefined` and would let the `null` through. Classes generated from
`[GenerateTypeScriptClass]` already follow this.

Example:

```ts
// Wrong: Blazor passes null, which the default does not replace
setRevertValue(revertValue = '') { ... }

// Right
setRevertValue(revertValue: BlazorNullable<string>) {
    this.#revertValue = revertValue ?? '';
}
```

Plain TypeScript code is not affected. There, default parameters are fine and
ESLint asks for them.
