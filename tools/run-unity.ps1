# Runs Unity (or a stand-in) with a machine-wide single-run lock, a commit-memory guard,
# a timeout that kills the process tree it started, and memory-crash diagnosis.
# Usage: powershell -NoProfile -File tools/run-unity.ps1 [options] -- <unity args>
# Options: -UnityPath, -TimeoutMinutes (25), -MinFreeCommitGB (8), -LockWaitMinutes (30),
#          -CommitWaitSeconds (300), -DryRun. Works on Windows PowerShell 5.1.
# Exit codes: the Unity exit code, or 2 timed out, 3 gave up waiting for the lock, 4 no commit headroom.
# Options are parsed by hand because PowerShell -File rejects a bare "--" as an ambiguous parameter.
$UnityPath = ''
$TimeoutMinutes = 25.0
$MinFreeCommitGB = 8.0
$LockWaitMinutes = 30.0
$CommitWaitSeconds = 300
$LockName = 'Global\UncannyValetUnityRun'
$DryRun = $false
$UnityArgs = @()
for ($i = 0; $i -lt $args.Count; $i++) {
    switch -CaseSensitive ($args[$i]) {
        '--'                 { if ($i + 1 -lt $args.Count) { $UnityArgs = @($args[($i + 1)..($args.Count - 1)]) }; $i = $args.Count }
        '-UnityPath'         { $UnityPath = $args[++$i] }
        '-TimeoutMinutes'    { $TimeoutMinutes = [double]$args[++$i] }
        '-MinFreeCommitGB'   { $MinFreeCommitGB = [double]$args[++$i] }
        '-LockWaitMinutes'   { $LockWaitMinutes = [double]$args[++$i] }
        '-CommitWaitSeconds' { $CommitWaitSeconds = [int]$args[++$i] }
        '-LockName'          { $LockName = $args[++$i] }
        '-DryRun'            { $DryRun = $true }
        default              { Write-Host "run-unity: unknown option $($args[$i]) (put Unity arguments after --)"; exit 64 }
    }
}
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$GB = 1GB
$memoryPattern = 'Could not allocate|paging file|OutOfMemory|1455'
$infoFile = Join-Path $env:TEMP 'UncannyValetUnityRun.info'

function Get-Commit {
    $os = Get-CimInstance Win32_OperatingSystem
    $limit = [double]$os.TotalVirtualMemorySize * 1KB
    $free = [double]$os.FreeVirtualMemory * 1KB
    [pscustomobject]@{ Limit = $limit; Free = $free; Committed = $limit - $free }
}
function Format-Commit($c) {
    '{0:N1} of {1:N1} GB committed, {2:N1} GB free' -f ($c.Committed / $GB), ($c.Limit / $GB), ($c.Free / $GB)
}
function Resolve-UnityPath {
    if ($UnityPath) { return $UnityPath }
    $v = (Select-String -Path "$root/ProjectSettings/ProjectVersion.txt" -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
    $p = "C:\Program Files\Unity\Hub\Editor\$v\Editor\Unity.exe"
    if (-not (Test-Path $p)) { throw "Unity $v not found at $p" }
    $p
}
function Quote-Arg([string]$a) {
    if ($a -match '[\s"]' -and $a -notmatch '^".*"$') { return '"' + ($a -replace '"', '\"') + '"' }
    $a
}
function Get-LogPath([string[]]$a) {
    for ($i = 0; $i -lt $a.Count - 1; $i++) { if ($a[$i] -eq '-logFile') { return $a[$i + 1].Trim('"') } }
    $null
}

$exe = Resolve-UnityPath
$argLine = ($UnityArgs | ForEach-Object { Quote-Arg $_ }) -join ' '
if ($DryRun) { Write-Host "DRYRUN: `"$exe`" $argLine"; exit 0 }

$mutex = New-Object System.Threading.Mutex($false, $LockName)
$haveLock = $false
$proc = $null
$exitCode = 1
try {
    # 1. machine-wide single-run lock
    $deadline = (Get-Date).AddMinutes($LockWaitMinutes)
    $lastNote = [datetime]::MinValue
    while (-not $haveLock) {
        try { $haveLock = $mutex.WaitOne(1000) } catch [System.Threading.AbandonedMutexException] { $haveLock = $true }
        if ($haveLock) { break }
        if ((Get-Date) -gt $deadline) {
            Write-Host "run-unity: gave up waiting for the other Unity run after $LockWaitMinutes min."
            $exitCode = 3
            exit $exitCode
        }
        if (((Get-Date) - $lastNote).TotalSeconds -ge 30) {
            $info = 'unknown'
            if (Test-Path $infoFile) { $info = Get-Content $infoFile -ErrorAction SilentlyContinue | Select-Object -First 1 }
            Write-Host "run-unity: waiting for the other Unity run ($info)"
            $lastNote = Get-Date
        }
    }
    "PID $PID, started $((Get-Date).ToString('HH:mm'))" | Set-Content $infoFile

    # 2. commit headroom guard
    $c = Get-Commit
    Write-Host "run-unity: commit $(Format-Commit $c)"
    $gate = (Get-Date).AddSeconds($CommitWaitSeconds)
    while ($c.Free -lt $MinFreeCommitGB * $GB) {
        if ((Get-Date) -gt $gate) {
            Write-Host ("run-unity: refusing to start, only {0:N1} GB commit free (need {1} GB): {2}. Close programs or restart Windows." -f ($c.Free / $GB), $MinFreeCommitGB, (Format-Commit $c))
            $exitCode = 4
            exit $exitCode
        }
        Write-Host "run-unity: waiting for commit headroom ($(Format-Commit $c))"
        Start-Sleep -Seconds ([Math]::Min(10, [Math]::Max(1, $CommitWaitSeconds)))
        $c = Get-Commit
    }

    # 3. launch with timeout
    Write-Host "run-unity: starting $exe $argLine"
    $sp = @{ FilePath = $exe; PassThru = $true; NoNewWindow = $true }
    if ($argLine) { $sp.ArgumentList = $argLine }
    $proc = Start-Process @sp
    $null = $proc.Handle
    if (-not $proc.WaitForExit([int]([Math]::Min($TimeoutMinutes * 60000, [int]::MaxValue)))) {
        Write-Host "run-unity: timed out after $TimeoutMinutes min, killing process tree $($proc.Id)"
        & taskkill.exe /T /F /PID $proc.Id | Out-Null
        $proc.WaitForExit(10000) | Out-Null
        Write-Host 'run-unity: timed out'
        $exitCode = 2
    } else {
        $proc.WaitForExit()
        $exitCode = $proc.ExitCode
        # 4. crash diagnosis
        $log = Get-LogPath $UnityArgs
        if ($exitCode -ne 0 -and $log -and (Test-Path $log) -and (Select-String -Path $log -Pattern $memoryPattern -Quiet)) {
            $c2 = Get-Commit
            Write-Host ("run-unity: crashed on memory allocation, commit was {0:N1} of {1:N1} GB. Restart Windows if it repeats." -f ($c2.Committed / $GB), ($c2.Limit / $GB))
        }
    }
}
finally {
    # 5. clean up anything we started, then release the lock
    if ($proc -and -not $proc.HasExited) { & taskkill.exe /T /F /PID $proc.Id 2>&1 | Out-Null }
    if ($haveLock) { try { $mutex.ReleaseMutex() } catch { Write-Host "run-unity: could not release lock: $_" } }
    $mutex.Dispose()
}
exit $exitCode
