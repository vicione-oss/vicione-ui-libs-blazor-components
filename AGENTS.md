# AGENTS.md

## Code style
- Refer to `docs/csharp-code-style.md` for C# coding conventions
- Refer to `docs/scss-code-style.md` for SCSS coding conventions

## Documentation style
- Refer to `docs/documentation-style.md` for comment and documentation conventions, including consistent terminology

## Skills

Reusable skill definitions located in `.claude/skills/` following the [Agent Skills](https://agentskills.io/) open standard.
Supported by GitHub Copilot and Claude Code.

| Skill                                                                      | Description                                              |
|----------------------------------------------------------------------------|---------------------------------------------------------|
| [delete-orphaned-css](.claude/skills/delete-orphaned-css/SKILL.md)         | Delete orphaned Blazor scoped CSS files (`*.razor.css`) |
| [improve-comment-clarity](.claude/skills/improve-comment-clarity/SKILL.md) | Improve the clarity of code comments                    |

Each skill can have an associated slash command (`.claude/commands/`) and custom
prompt (`.github/prompts/`) that delegate to it as the single source of truth.

When you **add, remove, or rename** any skill, slash command, or custom prompt
file, update the solution file (`ViciOne.Ui.Blazor.Components.slnx`) accordingly
so these files stay listed as solution items.

## Working on `.ts` files

When working on `.ts` files living in a folder of a `.csproj`, you need to keep attention to the following files to get the orchestration of the folder / file structure for generated `.js` files right.

Only touch these files when you **add, rename, remove, or re-target** a `.ts` file or an import specifier. A pure in-file logic change requires no updates here.

File | What to do
-|-
`tsconfig.json` | Maintain `compilerOptions.paths` to reflect file structure and ensure correct module resolution.
`.csproj` | Maintain MSBuild items named `GeneratedStaticWebAsset` to ensure generated `.js` files are included as static web assets in the Blazor project.

After making changes to the `.ts` files, make sure the code style is consistent by running the following command from solution root:

`npm run lint-with-fix`
