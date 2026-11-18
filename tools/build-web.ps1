# Builds the Web player locally (Windows) and copies it into site/, which GitHub Pages publishes.
# Close the Unity editor first. Needs the WebGL Build Support module for the project's editor version.
# Unity runs through tools/run-unity.ps1 (single-run lock, commit guard, timeout).
# Usage: powershell -NoProfile -File tools/build-web.ps1    Build: build/WebGL/WebGL    Publish folder: site/
# Then commit site/ (plus a docs/changelogs entry) and push main.
# With -Sandbox the Sandbox scene alone is built (an empty lot to test driving per world) into build/WebGL/Sandbox and copied to site-sandbox/,
# which the workflow publishes at /sandbox/ from the pivot/chaos branch. site/ is not touched.
param([switch]$Sandbox)
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$version = (Select-String -Path "$root/ProjectSettings/ProjectVersion.txt" -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $version not found at $unity" }
if (-not (Test-Path "$(Split-Path -Parent $unity)\Data\PlaybackEngines\WebGLSupport")) {
    throw "WebGL Build Support is not installed for Unity $version."
}
$unityArgs = @("-batchmode", "-nographics", "-projectPath", $root,
               "-executeMethod", "UncannyValet.EditorTools.WebBuild.BuildFromCommandLine",
               "-logFile", "$root/build/web-build.log")
$builtName = "WebGL"
$siteName = "site"
if ($Sandbox) {
    $builtName = "Sandbox"
    $siteName = "site-sandbox"
    $unityArgs += @("-sandboxBuild", "-customBuildPath", "$root/build/WebGL/Sandbox")
}
New-Item -ItemType Directory -Force -Path "$root/build" | Out-Null
& powershell -NoProfile -ExecutionPolicy Bypass -File "$PSScriptRoot/run-unity.ps1" -TimeoutMinutes 45 -- @unityArgs
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE). See build/web-build.log" }

$built = "$root/build/WebGL/$builtName"
if (-not (Test-Path "$built/index.html")) { throw "No index.html in $built" }
$site = "$root/$siteName"
if (Test-Path $site) { Remove-Item -Recurse -Force $site }
Copy-Item -Recurse $built $site
bash "$root/tools/write-build-info.sh" "$site"
Write-Host "Built and copied to $siteName/. Test it with: npx http-server $siteName"
if ($Sandbox) { Write-Host "Then commit $siteName/ with a changelog entry and push pivot/chaos." }
else { Write-Host "Then commit site/ with a changelog entry and push main." }
