# Runs EditMode tests in batch mode (Windows) through tools/run-unity.ps1. Close the Unity editor first.
# Usage: powershell -NoProfile -File tools/run-tests.ps1           Results: TestResults/editmode.xml
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Force -Path "$root/TestResults" | Out-Null
$unityArgs = @("-batchmode", "-nographics", "-projectPath", $root, "-runTests", "-testPlatform", "EditMode",
               "-testResults", "$root/TestResults/editmode.xml", "-logFile", "$root/TestResults/editmode.log")
& powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/run-unity.ps1" -- @unityArgs
if ($LASTEXITCODE -ne 0) { throw "Tests failed (exit $LASTEXITCODE). See TestResults/editmode.log" }
Write-Host "Tests passed. Results in TestResults/editmode.xml"
