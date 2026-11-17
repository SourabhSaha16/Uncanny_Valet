#!/usr/bin/env bash
# Git Bash shim for tools/run-unity.ps1. Usage: bash tools/run-unity.sh [options] -- <unity args>
here="$(cd "$(dirname "$0")" && pwd)"
if command -v cygpath >/dev/null 2>&1; then ps1="$(cygpath -w "$here/run-unity.ps1")"; else ps1="$here/run-unity.ps1"; fi
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "$ps1" "$@"
exit $?
