# Running Unity safely: tools/run-unity.ps1

Windows runs out of commit memory (paging file, error 1455) when Unity runs pile up. Every batch-mode
Unity launch goes through the wrapper, which:

- takes a machine-wide lock (`Global\UncannyValetUnityRun`), so only one Unity run happens at a time across
  all worktrees; a second call waits (default 30 min) and prints who holds the lock every 30 s;
- checks free commit memory (needs `-MinFreeCommitGB`, default 8), waits up to 5 min, then refuses (exit 4);
- kills the Unity process tree it started after `-TimeoutMinutes` (default 25, exit 2), and on Ctrl+C or error;
- passes the Unity exit code through, and says "crashed on memory allocation" when the log shows a memory error.

```
powershell -NoProfile -File tools/run-unity.ps1 -- -batchmode -nographics -projectPath <path> -logFile <log> ...
bash tools/run-unity.sh -- <unity args>            # Git Bash
powershell -NoProfile -File tools/run-unity.ps1 -DryRun -- <unity args>   # print the command line only
```

Options go before `--`: `-UnityPath`, `-TimeoutMinutes`, `-MinFreeCommitGB`, `-LockWaitMinutes`, `-CommitWaitSeconds`.
Exit codes: Unity's own, or 2 timed out, 3 gave up waiting for the lock, 4 not enough commit memory.
`tools/run-tests.ps1` and `tools/build-web.ps1` already use it. The editor opened by hand (not through the
wrapper) is not covered by the lock. Self-test (no Unity): `powershell -NoProfile -File tools/tests/test-run-unity.ps1`.

## The sandbox site (issue #180)

A third site beside stable (`site/`, root) and beta (`site/` of `pivot/chaos`, `/beta/`): an empty lot for testing how the car drives in each
world, with a world picker (keys 1 to 9), R to reset the car and the F2 tuning panel. No shift, patrons or scoring.

- Scene: `Assets/_Project/Scenes/Sandbox.unity`, built by `Uncanny Valet/Setup/Build Everything` (not in Build Settings).
- Worlds: the lot definitions listed in `Assets/_Project/Data/Sandbox/SandboxWorldsConfig.asset`. To add a world, add its lot definition to
  that list and run Build Everything.
- Build (Unity editor closed): `powershell -NoProfile -File tools/build-web.ps1 -Sandbox` builds only that scene into `site-sandbox/`
  (committed, with `build-info.txt`). Without the switch the script builds the game into `site/` as before.
- Publish: commit `site-sandbox/` on `pivot/chaos` (with a `docs/changelogs/` entry) and push; the workflow copies it to `/sandbox/` and
  fails with a clear error when `site-sandbox/index.html` is missing.
