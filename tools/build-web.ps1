# Builds the Web player locally (Windows). Close the Unity editor first.
# Usage: pwsh tools/build-web.ps1            Output: build/WebGL/WebGL
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$version = (Select-String -Path "$root/ProjectSettings/ProjectVersion.txt" -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $version not found at $unity" }
$unityArgs = @("-batchmode", "-nographics", "-projectPath", "`"$root`"",
               "-executeMethod", "UncannyValet.EditorTools.WebBuild.BuildFromCommandLine",
               "-logFile", "`"$root/build/web-build.log`"")
New-Item -ItemType Directory -Force -Path "$root/build" | Out-Null
$p = Start-Process -FilePath $unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
if ($p.ExitCode -ne 0) { throw "Build failed (exit $($p.ExitCode)). See build/web-build.log" }
Write-Host "Built to build/WebGL/WebGL. Serve it with: npx http-server build/WebGL/WebGL"
