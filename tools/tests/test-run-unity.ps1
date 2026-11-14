# Tests tools/run-unity.ps1 with fake executables. Never launches Unity.
# Usage: powershell -NoProfile -File tools/tests/test-run-unity.ps1
$ErrorActionPreference = 'Stop'
$wrapper = Join-Path (Split-Path -Parent $PSScriptRoot) 'run-unity.ps1'
$work = Join-Path $env:TEMP ("run-unity-test-" + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$lock = 'Global\UncannyValetUnityRunTest' + [guid]::NewGuid().ToString('N')
$fails = 0

# fake exit-code program, and a fake that spawns a long-running child (ping -n 77) under cmd
Set-Content "$work\fake-exit.cmd" "@echo off`r`nexit /b %1"
Set-Content "$work\fake-hang.cmd" "@echo off`r`nping -n 77 127.0.0.1 >nul"
Set-Content "$work\fake-short.cmd" "@echo off`r`nping -n 6 127.0.0.1 >nul`r`necho short-done"
Set-Content "$work\fake-oom.cmd" "@echo off`r`necho Could not allocate memory > %2`r`nexit /b 1"

function Run-Wrapper([string[]]$extra) {
    $ErrorActionPreference = 'Continue'
    $out = & powershell -NoProfile -ExecutionPolicy Bypass -File $wrapper -LockName $lock @extra 2>&1 | Out-String
    [pscustomobject]@{ Code = $LASTEXITCODE; Out = $out }
}
function Check([string]$name, [bool]$ok, $detail) {
    if ($ok) { Write-Host "PASS $name" } else { Write-Host "FAIL $name  $detail"; $script:fails++ }
}
function Hang-Count { @(Get-CimInstance Win32_Process -Filter "Name='PING.EXE'" | Where-Object { $_.CommandLine -match 'ping\s+-n\s+77' }).Count }
function Lock-Free {
    $m = New-Object System.Threading.Mutex($false, $lock)
    try { $got = $m.WaitOne(2000) } catch [System.Threading.AbandonedMutexException] { $got = $true }
    if ($got) { $m.ReleaseMutex() }
    $m.Dispose(); $got
}

try {
    # DryRun prints the command line and quotes spaces, strips the leading --
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '-DryRun', '--', '-batchmode', '-projectPath', 'D:\a b\c')
    Check 'dryrun prints command' ($r.Code -eq 0 -and $r.Out -match 'DRYRUN' -and $r.Out -match '-batchmode -projectPath "D:\\a b\\c"' -and $r.Out -notmatch ' -- ') $r.Out

    # Exit code passthrough, and the commit line is printed
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '--', '7')
    Check 'exit code 7 passes through' ($r.Code -eq 7) "code $($r.Code) $($r.Out)"
    Check 'prints commit at start' ($r.Out -match 'run-unity: commit .* GB committed') $r.Out
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '--', '0')
    Check 'exit code 0 passes through' ($r.Code -eq 0) "code $($r.Code)"

    # Memory crash diagnosis from the log
    $log = "$work\unity.log"
    $r = Run-Wrapper @('-UnityPath', "$work\fake-oom.cmd", '--', '-logFile', $log)
    Check 'memory crash diagnosed' ($r.Code -eq 1 -and $r.Out -match 'crashed on memory allocation, commit was') "code $($r.Code) $($r.Out)"

    # Headroom guard refuses
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '-MinFreeCommitGB', '100000', '-CommitWaitSeconds', '2', '--', '0')
    Check 'headroom guard refuses' ($r.Code -eq 4 -and $r.Out -match 'refusing to start') "code $($r.Code) $($r.Out)"
    Check 'lock released after refusal' (Lock-Free) ''

    # Timeout kills the child tree and releases the lock
    $r = Run-Wrapper @('-UnityPath', "$work\fake-hang.cmd", '-TimeoutMinutes', '0.05', '--', 'x')
    Check 'timeout exits 2 with message' ($r.Code -eq 2 -and $r.Out -match 'timed out') "code $($r.Code) $($r.Out)"
    Start-Sleep -Seconds 1
    Check 'child tree killed' ((Hang-Count) -eq 0) "ping still running"
    Check 'lock released after kill' (Lock-Free) ''

    # Lock contention: second invocation waits, then runs
    $first = Start-Process powershell -PassThru -WindowStyle Hidden -RedirectStandardOutput "$work\first.out" -ArgumentList @(
        '-NoProfile', '-File', "`"$wrapper`"", '-LockName', $lock, '-UnityPath', "`"$work\fake-short.cmd`"", '--', 'x')
    Start-Sleep -Seconds 2
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '--', '0')
    $sw.Stop()
    $first.WaitForExit()
    Check 'second run waited for the first' ($r.Code -eq 0 -and $r.Out -match 'waiting for the other Unity run' -and $sw.Elapsed.TotalSeconds -gt 2) "code $($r.Code) $([int]$sw.Elapsed.TotalSeconds)s $($r.Out)"

    # Lock wait timeout gives up with code 3
    $holder = Start-Process powershell -PassThru -WindowStyle Hidden -ArgumentList @(
        '-NoProfile', '-File', "`"$wrapper`"", '-LockName', $lock, '-UnityPath', "`"$work\fake-short.cmd`"", '--', 'x')
    Start-Sleep -Seconds 2
    $r = Run-Wrapper @('-UnityPath', "$work\fake-exit.cmd", '-LockWaitMinutes', '0.03', '--', '0')
    $holder.WaitForExit()
    Check 'lock wait gives up with 3' ($r.Code -eq 3) "code $($r.Code) $($r.Out)"
}
finally {
    Get-CimInstance Win32_Process -Filter "Name='PING.EXE'" | Where-Object { $_.CommandLine -match 'ping\s+-n\s+77' } |
        ForEach-Object { & taskkill.exe /T /F /PID $_.ProcessId | Out-Null }
    Remove-Item -Recurse -Force $work -ErrorAction SilentlyContinue
}
if ($fails -gt 0) { Write-Host "$fails test(s) FAILED"; exit 1 }
Write-Host 'All run-unity tests passed'
