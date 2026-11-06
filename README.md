# Uncanny Valet: start here

This kit turns a new Unity project into one that a team of Claude Code agents builds in parallel. Each
push to `main` publishes the game's web build (the committed `site/` folder) to GitHub Pages. The build
itself is made on your PC with `tools/build-web.ps1`; CI needs no Unity licence or secrets. You set things
up once, paste one prompt, and come back at two checkpoints.

The first goal is a **pick-up and drop-off prototype**: walk to a car, drive it into a spot, bring it back
when the patron asks, earn a tip. No real UI or art yet (see `docs/design.md`, section 10).

## Before you start

- A Claude Pro, Max, Team or Enterprise plan (Claude Code isn't on the free plan). Agent teams use much
  more of your allowance than one chat.
- A free GitHub account and a free Unity account.
- Windows 10+ or macOS 13+.

## Step 1. Install the tools

Windows (PowerShell):

```powershell
winget install Git.Git
winget install GitHub.cli
winget install Microsoft.PowerShell
winget install Unity.UnityHub
irm https://claude.ai/install.ps1 | iex
```

macOS: `brew install git gh` and `brew install --cask unity-hub`, then
`curl -fsSL https://claude.ai/install.sh | bash`.

Reopen the terminal, then check: `git --version`, `gh --version`, `claude --version`.

## Step 2. Create the Unity project

1. In Unity Hub, sign in and install **Unity 6000.2.5f1** with the **Web Build Support** module (the
   local build needs it; if the Hub's gear menu has no "Add modules", reinstall the editor from the Hub with
   WebGL ticked).
2. **New project** → **Universal 2D** → name `UncannyValet` → a folder such as `C:\Dev` → **Create**.
3. Open `Packages/manifest.json` and check it lists `com.unity.inputsystem` and `com.unity.test-framework`.
   (The template normally includes both. If not, add them in Window → Package Manager.)
4. Close the editor.

## Step 3. Add this kit

Copy everything in this folder into `C:\Dev\UncannyValet`, including the hidden `.claude` and `.github`
folders. Let `.gitignore` replace the existing one. Then open the project in Unity once so it creates
`.meta` files for the new scripts, check the Console has no errors, and close it again.

## Step 4. Check Unity is activated on this PC

Open the project once in Unity Hub with your signed-in account. No licence file or secrets are needed for
GitHub: Unity only runs on your PC.

## Step 5. Create the public GitHub repo (in the project folder)

```bash
gh auth login
git init -b main
git add .
git commit -m "lead: add Uncanny Valet starter kit"
gh repo create uncanny-valet --public --source=. --remote=origin
```

The commit subject matches the entry already in `docs/changelogs/lead.md`, so the CI changelog check
passes. Don't push yet.

## Step 6. Build the site and turn on Pages (human checkpoint 1)

1. Close the Unity editor, then run `pwsh tools/build-web.ps1` (about two minutes). It builds the Web player
   and copies it into `site/` with `build-info.txt` and `changelog.txt`.
2. Test it: `npx http-server site`, then open http://localhost:8080.
3. Commit `site/` with a `docs/changelogs/` entry (for example `lead: add first Web build in site/`).
4. On GitHub: repo **Settings → Pages → Build and deployment → Source: GitHub Actions**.
5. Push: `git push -u origin main`. Watch it with `gh run watch`. CI checks the changelog and publishes
   `site/`; it takes about half a minute. The game is then at `https://<your-user>.github.io/<repo>/`.

To publish an update later: close the editor, run `tools/build-web.ps1`, commit `site/` with a changelog
entry, push `main`. Each build adds roughly 58 MB to git history, so rebuild only when you want to publish.

## Step 7. Start the agents

In the project folder run `claude`, then paste prompt 1 from `KICKOFF.md`.

## How the agents work in parallel

| Phase | Who | Parallel |
| --- | --- | --- |
| P0 | Lead checks Core compiles and tests pass | — |
| P1 | cars, valet, lot, patrons, economy-night | All five at once |
| P2 | integration + qa | Both at once |
| P3 | qa reviews, lead merges, lead rebuilds `site/`, push deploys | — |

Every module depends only on `Core` (shared events and interfaces), so P1 agents never wait on each other.
Each agent works on its own branch in its own git worktree, and only touches its own folder.

## Changelogs

Every commit adds an entry to `docs/changelogs/<agent>.md` with a heading that matches the commit
subject. CI rejects commits without one. The live site also serves `changelog.txt` (all entries plus the
last 50 commits) and `build-info.txt` (which commit is live).

## The two times you're needed

1. **Step 6** above (Pages setting and first push).
2. **Play test:** when the lead says P1 is deployed, play it in the browser and say what feels wrong.
   Then paste prompt 3 from `KICKOFF.md`.

## What's in the kit

| Path | Purpose |
| --- | --- |
| `CLAUDE.md` | Rules every agent follows: ownership, modularity, file size, web rules, changelog, phases |
| `docs/design.md` | The single design document every agent works from |
| `docs/changelogs/` | One changelog per agent |
| `docs/status.md`, `docs/decisions.md` | Progress board and decisions log |
| `.claude/agents/` | Seven agent briefs |
| `.claude/settings.json` | Turns on agent teams; allows build/test/git commands; blocks force-push |
| `Assets/_Project/Scripts/Core/` | Shared events, ids, interfaces and the service registry |
| `Assets/_Project/Scripts/<Module>/` | One folder and assembly per module |
| `Assets/_Project/Editor/WebBuild.cs` | Web build used by the local script |
| `Assets/_Project/Tests/EditMode/` | Tests, starting with the event bus |
| `.github/workflows/ci-cd.yml` | Changelog check → publish the committed `site/` to GitHub Pages |
| `site/` | The built Web player that GitHub Pages serves (made by `tools/build-web.ps1`) |
| `tools/` | Local test and build scripts, changelog check |
