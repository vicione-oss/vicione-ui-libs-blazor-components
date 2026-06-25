# AGENTS.md

## Working on `.ts` files

When working on `.ts` files, you need to keep attention to the following files to get the orchestration of the folder / file structure for generated `.js` files right:

File | What to do
-|-
`tsconfig.json` | Maintain `compilerOptions.paths` to reflect file structure and ensure correct module resolution.
`ViciOne.Ui.Blazor.Components.csproj` | Maintain MSBuild items named `GeneratedStaticWebAsset` to ensure generated `.js` files are included as static web assets in the Blazor project.

After making changes to the `.ts` files, make sure the code style is consistent by running the following command from solution root:

`npm run lint-with-fix`
