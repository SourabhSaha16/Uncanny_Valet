# Runs EditMode tests in batch mode (Windows). Close the Unity editor first.
# Usage: pwsh tools/run-tests.ps1           Results: TestResults/editmode.xml
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$version = (Select-String -Path "$root/ProjectSettings/ProjectVersion.txt" -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $version not found at $unity" }
New-Item -ItemType Directory -Force -Path "$root/TestResults" | Out-Null
$unityArgs = @("-batchmode", "-nographics", "-projectPath", "`"$root`"", "-runTests", "-testPlatform", "EditMode",
               "-testResults", "`"$root/TestResults/editmode.xml`"", "-logFile", "`"$root/TestResults/editmode.log`"")
$p = Start-Process -FilePath $unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
if ($p.ExitCode -ne 0) { throw "Tests failed (exit $($p.ExitCode)). See TestResults/editmode.log" }
Write-Host "Tests passed. Results in TestResults/editmode.xml"
