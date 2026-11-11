#!/usr/bin/env bash
# Fails if any commit changes project files without also changing a file in docs/changelogs/.
# Run by CI; you can also run it locally: bash tools/check-changelog.sh <base-sha> <head-sha>
set -euo pipefail

WATCHED='^(Assets/|Packages/|ProjectSettings/|\.github/|tools/)'
LOGS='^docs/changelogs/'
ZERO='0000000000000000000000000000000000000000'

if [ "$#" -eq 2 ]; then
  BASE="$1"; HEAD="$2"
elif [ "${EVENT_NAME:-}" = "pull_request" ]; then
  BASE="$PR_BASE_SHA"; HEAD="$PR_HEAD_SHA"
else
  BASE="${BEFORE_SHA:-}"; HEAD="${HEAD_SHA:-HEAD}"
fi

if [ -z "$BASE" ] || [ "$BASE" = "$ZERO" ]; then
  # First push of a branch: check only commits that are not already on origin/main.
  if git rev-parse --verify -q origin/main >/dev/null; then
    COMMITS=$(git rev-list --no-merges "$HEAD" ^origin/main)
  else
    COMMITS=$(git rev-list --no-merges "$HEAD")
  fi
else
  COMMITS=$(git rev-list --no-merges "$BASE..$HEAD")
fi

EXEMPT_FILE="$(dirname "$0")/changelog-exempt.txt"
FAILED=0
for c in $COMMITS; do
  # Commits already pushed without an entry; history is never rewritten, so they are listed here with a reason.
  if [ -f "$EXEMPT_FILE" ] && grep -q "^$c" "$EXEMPT_FILE"; then continue; fi
  FILES=$(git diff-tree --no-commit-id --name-only -r "$c")
  if echo "$FILES" | grep -Eq "$WATCHED" && ! echo "$FILES" | grep -Eq "$LOGS"; then
    echo "::error::$(git log -1 --format='%h %s' "$c") changes project files but no docs/changelogs/ entry."
    FAILED=1
  fi
done

if [ "$FAILED" -eq 1 ]; then
  echo "Add an entry to docs/changelogs/<agent>.md in the same commit (see CLAUDE.md, Changelog rule)."
  exit 1
fi
echo "Changelog check passed for $(echo "$COMMITS" | grep -c . || true) commit(s)."
