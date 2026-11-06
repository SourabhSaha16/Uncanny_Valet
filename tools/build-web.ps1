# Builds the Web player locally (Windows) and copies it into site/, which GitHub Pages publishes.
# Close the Unity editor first. Needs the WebGL Build Support module for the project's editor version.
# Usage: pwsh tools/build-web.ps1            Build: build/WebGL/WebGL    Publish folder: site/
# Then commit site/ (plus a docs/changelogs entry) and push main.
$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$version = (Select-String -Path "$root/ProjectSettings/ProjectVersion.txt" -Pattern '^m_EditorVersion: (.+)$').Matches[0].Groups[1].Value
$unity = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"
if (-not (Test-Path $unity)) { throw "Unity $version not found at $unity" }
if (-not (Test-Path "$(Split-Path -Parent $unity)\Data\PlaybackEngines\WebGLSupport")) {
    throw "WebGL Build Support is not installed for Unity $version."
}
$unityArgs = @("-batchmode", "-nographics", "-projectPath", "`"$root`"",
               "-executeMethod", "UncannyValet.EditorTools.WebBuild.BuildFromCommandLine",
               "-logFile", "`"$root/build/web-build.log`"")
New-Item -ItemType Directory -Force -Path "$root/build" | Out-Null
$p = Start-Process -FilePath $unity -ArgumentList $unityArgs -Wait -PassThru -NoNewWindow
if ($p.ExitCode -ne 0) { throw "Build failed (exit $($p.ExitCode)). See build/web-build.log" }

$built = "$root/build/WebGL/WebGL"
if (-not (Test-Path "$built/index.html")) { throw "No index.html in $built" }
$site = "$root/site"
if (Test-Path $site) { Remove-Item -Recurse -Force $site }
Copy-Item -Recurse $built $site
bash "$root/tools/write-build-info.sh" "$site"
Write-Host "Built and copied to site/. Test it with: npx http-server site"
Write-Host "Then commit site/ with a changelog entry and push main."
