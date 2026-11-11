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
| `Assets/_Project/Scripts/Audio/` | audio (new module, assembly `UncannyValet.Audio`) |
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

## Engineering standards (apply to every line of code)

Design
- Single responsibility: one class, one reason to change. Prefer composition over inheritance and small
  interfaces over big ones. Mark classes `sealed` unless inheritance is designed for.
- Dependencies are passed in: plain classes take them in the constructor; MonoBehaviours get them from
  `Services` in `Start` (never `Awake`) or from `[SerializeField]` references. Only Bootstrap meets concrete types.
- Open for extension: new rules are new classes behind an interface, not new `if` branches in old ones.
- No magic numbers, no strings as identifiers. Tunables live in `*Config` / `*Definition` assets with
  `[Min]`, `[Range]` and `[Tooltip]`. Treat ScriptableObjects as read-only at runtime.
- State has one owner. Everyone else reads it or reacts to its event.

Unity correctness
- Subscribe to events in `OnEnable`, unsubscribe in `OnDisable`. Never leave a subscription behind.
- Never allocate in `Update`/`FixedUpdate`: no LINQ, `new` collections, string concatenation, boxing or
  closures. Reuse buffers (`NonAlloc` physics calls). Cache components in `Awake`; no `GetComponent`,
  `Find*` or `Camera.main` per frame.
- Physics: move Rigidbody2D bodies in `FixedUpdate` through the body (never `transform.position`);
  read input in `Update`; use `Time.deltaTime` or `fixedDeltaTime`, never frame counts.
- Rendering: never `renderer.material` (it leaks a copy); use shared materials or `MaterialPropertyBlock`;
  cache generated sprites and textures once.
- Use `[RequireComponent]` and `[DisallowMultipleComponent]` where a component needs a sibling. Validate
  setup in `OnValidate` / `Awake` and fail loudly with `Debug.LogError` naming the object; never silently
  skip a missing reference.
- Destroying or despawning an object must release everything it holds: spots, timers, subscriptions, labels.

Code quality
- Pure rules are plain C# with EditMode tests written with the rule. Every bug fix adds a regression test.
- Public API gets an XML summary. No dead code, no commented-out code, no TODO without an owner in status.md.
- `TuningKey` (and any enum stored as an integer in an asset) is **append-only**: never remove, reorder or insert in the middle, because `DifficultyDialsConfig.asset` stores raw key numbers. A removal once broke start-up in the web build (issue found 2026-11-11); `BootstrapSceneSmokeTests` guards it.
- Handle expected failures with return values (`TryGet`), not exceptions. Never swallow exceptions.
- Prefer enums, readonly structs and small types over loose bools, ints and tuples.
- Before committing, an agent re-reads its diff against this list and the file-size limits.
- qa reviews every branch against this section and rejects violations.

## Web build rules

The target is Unity Web (WebGL) on GitHub Pages. No threads or `Task.Run`, no `System.IO` file access,
no blocking waits. Keep build compression disabled. Use the Input System package for input.

The Web player is built **locally** with `tools/build-web.ps1`, which writes it to the committed `site/` folder.
CI does not run Unity and has no secrets; it checks changelogs and publishes `site/` to GitHub Pages.
Only the lead rebuilds and commits `site/` (about 58 MB per build, so only at publish points), with a
`docs/changelogs/lead.md` entry. The Unity editor must be closed first.

## Parallel work and git

- Each agent works on its own branch `agent/<name>` in its own git worktree, so agents never write the
  same files. The lead merges into `main` after qa reviews.
- Never force-push, rebase `main`, or rewrite history. Never commit `Library/`, `Temp/` or builds.
- Unity's batch mode can't run while the editor has the project open; close the editor before agents run
  builds or tests (`tools/run-tests.sh` or `tools/run-tests.ps1`).

## Changelog rule (every commit)

1. Commit subject: `<agent>: <what changed, under 72 chars> [Claude]`, e.g. `cars: add CarDriver top-down steering [Claude]`. Every commit mentions `[Claude]` so the human can tell Claude's commits from their own.
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
| P3 | qa reviews every module; lead merges, rebuilds `site/` locally, pushes `main` to deploy to GitHub Pages | No |

**Human checkpoints:** (1) GitHub Pages source set to "GitHub Actions", before the first push;
(2) play the P1 build in the browser and say what feels wrong.

## Release routine (adopted 2026-11-11, from `docs/redesign/test_review.md`)

The slow parts are Unity itself (start-up 13 to 30 s per launch, web build about 500 s), not the tests. So:

- **Every merge:** run the EditMode suite (about 25 s). Always.
- **RunBatch** (`ProjectSetup.RunBatch`) only when something under `Assets/_Project/Editor/`, `Data/` or `Scenes/` changed, or an agent added a ScriptableObject field or wiring.
- **PlayMode** only before a site deploy, when Cars, Lot, Traffic, Valet or Pedestrians physics code changed, or on request. Module agents run PlayMode for their own area.
- **Web build and `site/`** only when something player-visible changed (runtime code, art, audio, scenes or data), and then once for a batch of merges, not per merge. Docs-only, tests-only and tooling-only merges never rebuild `site/`; pushing them redeploys the committed `site/`.
- **Changelog:** every commit that touches `Assets/`, `Packages/`, `ProjectSettings/`, `.github/` or `tools/` carries a `docs/changelogs/` entry, including the lead's own regenerate-and-rebuild commits (CI failed once on this).

## Stable and beta sites (adopted 2026-11-11)

- `main` is **stable**: its committed `site/` is served at https://sourabhsaha16.github.io/Uncanny_Valet/ (the link given to playtesters).
- `pivot/chaos` is **beta**: its committed `site/` is served at https://sourabhsaha16.github.io/Uncanny_Valet/beta/. All new work lands and is built here first.
- **Promote** (beta to stable) only when the human says so: merge `pivot/chaos` into `main` (fast-forward when possible) and push `main`. Never push to `main` otherwise.
- One workflow (`.github/workflows/ci-cd.yml`) assembles both and deploys on a push to either branch. The changelog check runs first; commits that were pushed without an entry are listed in `tools/changelog-exempt.txt` (history is never rewritten).

## Tracking work (adopted 2026-11-11)

- Pending work lives in **GitHub issues** of this repo, shown on the **Uncanny_Valet_Kanban** GitHub Project, the one main project (https://github.com/users/SourabhSaha16/projects/5; columns: Backlog, Ready, In progress, In review, Done). The older project 4 "Uncanny Valet" is retired: do not add to it or move cards on it. Priority is the `priority:1` to `priority:4` label on the issue. `docs/pending_tasks.md` only points there.
- Issue titles are plain, human-readable sentences (no task ID prefix; the old IDs are mapped in `docs/pending_tasks.md`). Every issue body is a two-line description, a blank line, then one line `**Done when:** <acceptance criterion>`. Labels: `area:*` (module), `type:*` (bug, feature, chore, playtest, decision), `priority:1` to `priority:4` (1 do first, 2 do next, 3 do later, 4 someday; set by the human) and `needs decision` (waiting on the human). Milestones group a feature (Car handling, Valet powers, Patron Director step 4, ...).
- The lead creates and moves issues; agent briefs name the issue number and agent reports end with it. Commits and merges reference it (`Refs #n`). Work lands on `pivot/chaos`, which is not the default branch, so `Closes #n` only closes an issue when `main` is promoted: the lead closes issues with `gh issue close` after merging to beta and notes it in the changelog.
- `gh` is at `C:\Program Files\GitHub CLI\gh.exe` (not on PATH in the lead's shell).

## Ready queue workflow (adopted 2026-11-11)

- The human puts issues in the **Ready** column of the Kanban project in the order they want them done (top first) and then asks the lead to pick them up one by one.
- Pick up the top Ready card: move it to **In progress**, work it, move it to **In review** when merged to `pivot/chaos`, and to **Done** (and close the issue) only after the human confirms or the issue's own acceptance line is met and the human has been told.
- Every status update goes on the issue as a **comment**, not only in chat: started, plan or blocker, what was merged (commit hash), tests, and what is left. Keep them short.
- **Every comment and every commit Claude writes starts or ends with `[Claude]`** (comments start with it, commit subjects end with it) so the human can tell them from their own.
- Work one card at a time unless the human says otherwise. If a card needs a decision, comment the question, add `needs decision` and ask in chat.
