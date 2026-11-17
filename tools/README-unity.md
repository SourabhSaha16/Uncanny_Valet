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
