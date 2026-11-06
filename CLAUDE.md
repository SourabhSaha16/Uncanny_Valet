# Uncanny Valet: rules for every Claude Code session

The main session is the **lead**: it plans, assigns work to agents, merges, and talks to the human.
It does not write gameplay code itself unless an agent is stuck.

## Source of truth

- `docs/design.md` is the only design doc. Every agent reads it before starting and follows it.
- Only the lead edits `docs/design.md`. An agent that finds a gap or contradiction asks the lead.
- Locked decisions (anything already in design.md) change only with the human's approval.
- `docs/status.md` is the progress board. `docs/decisions.md` logs every question and answer.

## Folders and ownership

| Path | Owner |
| --- | --- |
| `Assets/_Project/Scripts/Core/` | lead (contracts: changes by request only) |
| `Assets/_Project/Scripts/Cars/` | cars |
| `Assets/_Project/Scripts/Valet/` | valet |
| `Assets/_Project/Scripts/Lot/` | lot |
| `Assets/_Project/Scripts/Patrons/` | patrons |
| `Assets/_Project/Scripts/Economy/`, `Assets/_Project/Scripts/Night/` | economy-night |
| `Assets/_Project/Scripts/Bootstrap/`, `Assets/_Project/Scripts/UI/`, `Assets/_Project/Scenes/` | integration |
| `Assets/_Project/Editor/`, `.github/`, `tools/` | integration |
| `Assets/_Project/Tests/` | qa (module agents may add tests for their own module) |
| `Assets/_Project/Data/`, `Assets/_Project/Art/` | the module that defines the asset type |
| `docs/` | lead; `docs/changelogs/<agent>.md` belongs to that agent |

An agent edits only its own folders and its own changelog. Anything else is a request to the lead.

## Modularity rules

- Every module is an assembly definition that references **only** `UncannyValet.Core`
  (plus Unity packages). Bootstrap and Tests may reference everything. No module references another.
- Modules talk through events in `Core/Events` and interfaces in `Core/Contracts`, resolved through
  `Services`. No other singletons, no `FindObjectOfType` for game logic, no static mutable state.
- One public type per file; file name = type name. Exception: event structs grouped by domain.
- Namespaces: `UncannyValet.<Module>` (sub-folders add a segment only if the module has 8+ files).
- Files stay small: aim under 120 lines, hard limit 200. Methods under 30 lines.
- Naming: PascalCase types and methods, `_camelCase` private fields, `[SerializeField] private` over
  public fields, interfaces start with `I`, ScriptableObjects end in `Definition` or `Config`.
- Pure rules (tips, timing, ticket state) live in plain C# classes with EditMode tests.
- Data and behaviour are separate: tunable numbers go in ScriptableObjects or serialized fields,
  never hard-coded in logic.

## Web build rules

The target is Unity Web (WebGL) on GitHub Pages. No threads or `Task.Run`, no `System.IO` file access,
no blocking waits. Keep build compression disabled. Use the Input System package for input.

## Parallel work and git

- Each agent works on its own branch `agent/<name>` in its own git worktree, so agents never write the
  same files. The lead merges into `main` after qa reviews.
- Never force-push, rebase `main`, or rewrite history. Never commit `Library/`, `Temp/` or builds.
- Unity's batch mode can't run while the editor has the project open; close the editor before agents run
  builds or tests (`tools/run-tests.sh` or `tools/run-tests.ps1`).

## Changelog rule (every commit)

1. Commit subject: `<agent>: <what changed, under 72 chars>`, e.g. `cars: add CarDriver top-down steering`.
2. In the **same commit**, append an entry to `docs/changelogs/<agent>.md`:

```
## <commit subject, exactly as above>
Date: YYYY-MM-DD
Files: <list of files added or changed>
Why: <one or two sentences, linking the design.md section>
Tests: <tests added or run, and the result>
```

3. CI fails any push or pull request where a commit changes `Assets/`, `Packages/`, `ProjectSettings/`,
   `.github/` or `tools/` without also changing a file in `docs/changelogs/`.
4. The lead's merge commits use `lead: merge agent/<name>` with an entry in `docs/changelogs/lead.md`.

## Asking protocol

Agents ask the lead, never the human. The lead answers from design.md when it can; otherwise it asks
the human once, records the answer in `docs/decisions.md`, and updates design.md.

## Phases and checkpoints

| Phase | Work | Parallel? |
| --- | --- | --- |
| P0 | Lead checks Core compiles and the sample test passes in batch mode | No |
| P1 | cars, valet, lot, patrons, economy-night build their modules against Core | Yes, all five |
| P2 | integration wires the Bootstrap scene, debug overlay and Web build; qa writes tests | Yes, two |
| P3 | qa reviews every module; lead merges; push to `main` deploys to GitHub Pages | No |

**Human checkpoints:** (1) GitHub secrets and Pages settings, before the first push;
(2) play the P1 build in the browser and say what feels wrong.
