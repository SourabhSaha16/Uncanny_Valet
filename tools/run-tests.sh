#!/usr/bin/env bash
# Runs EditMode tests in batch mode (macOS). Close the Unity editor first.
# Usage: bash tools/run-tests.sh            Results: TestResults/editmode.xml
set -euo pipefail
cd "$(dirname "$0")/.."
VERSION=$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt)
UNITY="/Applications/Unity/Hub/Editor/$VERSION/Unity.app/Contents/MacOS/Unity"
[ -x "$UNITY" ] || { echo "Unity $VERSION not found at $UNITY"; exit 1; }
mkdir -p TestResults
"$UNITY" -batchmode -nographics -projectPath "$PWD" -runTests -testPlatform EditMode \
  -testResults "$PWD/TestResults/editmode.xml" -logFile "$PWD/TestResults/editmode.log"
echo "Tests passed. Results in TestResults/editmode.xml"
