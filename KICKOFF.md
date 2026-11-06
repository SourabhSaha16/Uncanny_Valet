# Kickoff prompts

Paste these into Claude Code, run from the project folder.

## 1. First start

```text
You are the lead for Uncanny Valet. Read CLAUDE.md, docs/design.md and docs/status.md.

Run P0: confirm the Unity editor is closed, run the EditMode tests in batch mode with the script in
tools/ for this OS, and fix only Core or project setup if they fail. Commit with a changelog entry.

Then run P1 in parallel: spawn the cars, valet, lot, patrons and economy-night agents at once, each in its
own git worktree on its own agent/<name> branch, briefed from .claude/agents/<name>.md. Answer their
questions from docs/design.md and log every answer in docs/decisions.md. When all five are done, spawn
integration and qa for P2 in parallel, then run P3: qa reviews every branch, you merge into main,
run the tests, and push main so CI deploys to GitHub Pages.

Keep docs/status.md current. Make sure every commit has its changelog entry. Stop and ask me only at the
play-test checkpoint or if a locked design decision would need to change.
```

## 2. Resume after closing Claude Code

```text
Read CLAUDE.md, docs/status.md and docs/decisions.md, then resume from where status.md leaves off.
Agents from the last session are gone: respawn the ones the current phase needs, with the same names,
in their existing worktrees, and brief each one from status.md. Continue until the next checkpoint.
```

## 3. After the play test

```text
Play-test notes: <what felt wrong, what felt good>.
Record them in docs/decisions.md, turn them into tasks in docs/status.md, assign the right agents, and
continue. If a note changes docs/design.md, propose the edit to me first.
```

## 4. Start the next milestone (later)

```text
Next milestone: <e.g. mafia jobs: drugs in boot and fuel drain>. Update docs/design.md with a new
"Milestone scope" section (ask me anything unclear first), add the Core contracts it needs, write any new
agent briefs, then run it in parallel the same way as P1-P3.
```
