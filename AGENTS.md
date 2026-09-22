# AGENTS.md

## Code style
- Refer to `docs/csharp-code-style.md` for C# coding conventions
- Refer to `docs/scss-code-style.md` for SCSS coding conventions

## Documentation style
- Refer to `docs/documentation-style.md` for comment and documentation conventions, including consistent terminology

## Localization
- Refer to `docs/localization-guide.md` for localized text and resource file conventions

## Commit messages
- Refer to `docs/commit-message-style.md` for commit message conventions

## Skills

Reusable skill definitions located in `.claude/skills/` following the [Agent Skills](https://agentskills.io/) open standard.
Supported by GitHub Copilot and Claude Code.

| Skill                                                                      | Description                                             | Diagnostic  |
|----------------------------------------------------------------------------|---------------------------------------------------------|-------------|
| [delete-orphaned-css](.claude/skills/delete-orphaned-css/SKILL.md)         | Delete orphaned Blazor scoped CSS files (`*.razor.css`) | `BLAZOR102` |
| [improve-comment-clarity](.claude/skills/improve-comment-clarity/SKILL.md) | Improve the clarity of code comments                    |             |

When a build reports a diagnostic listed above, apply the matching skill.

Each skill can have an associated slash command (`.claude/commands/`) and custom
prompt (`.github/prompts/`) that delegate to it as the single source of truth.

When you **add, remove, or rename** any skill, slash command, or custom prompt
file, update the solution file (`ViciOne.Ui.Blazor.Components.slnx`) accordingly
so these files stay listed as solution items.

## Rebasing

A rebase leaves every commit building, not only the last one. A conflict
resolved wrongly part-way through is masked when a later commit overwrites the
same lines, and the damage surfaces later, in a `git bisect` or an intermediate
checkout.

Replay with a build after each commit: `git rebase --exec "<build>" <upstream>`
stops at the first commit that fails, where it is fixed with `git commit
--amend` before continuing. Clear orphaned scoped CSS inside that command
rather than once beforehand: every checkout strands more of it, and those
errors bury the ones that matter.

This is worth its cost after conflicts were resolved by hand, or before
force-pushing a rewritten branch. It is not worth it where the upstream delta
touches no code at all: a docs, skill or tooling merge cannot break a build that
already passed, so rebase plainly. Confirm that rather than assuming it, with
`git diff --stat <old-base> <upstream>`.

## Working on `.ts` files

When working on `.ts` files living in a folder of a `.csproj`, you need to keep attention to the following files to get the orchestration of the folder / file structure for generated `.js` files right.

Only touch these files when you **add, rename, remove, or re-target** a `.ts` file or an import specifier. A pure in-file logic change requires no updates here.

File | What to do
-|-
`.csproj` | Maintain MSBuild items named `GeneratedStaticWebAsset` to ensure generated `.js` files are included as static web assets in the Blazor project.

After making changes to the `.ts` files, make sure the code style is consistent by running the following command from solution root:

`npm run lint-with-fix`
