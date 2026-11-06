#!/usr/bin/env bash
# Writes build-info.txt and changelog.txt next to index.html, so the live site shows what it contains:
#   https://<user>.github.io/<repo>/build-info.txt
#   https://<user>.github.io/<repo>/changelog.txt
set -euo pipefail
SITE_DIR="$1"

{
  echo "Uncanny Valet web build"
  echo "Commit:  $(git rev-parse --short HEAD)"
  echo "Subject: $(git log -1 --format=%s)"
  echo "Date:    $(git log -1 --format=%cd --date=iso)"
} > "$SITE_DIR/build-info.txt"

{
  echo "=== Last 50 commits ==="
  git log -50 --format='%h  %ad  %s' --date=short
  for f in docs/changelogs/*.md; do
    [ "$(basename "$f")" = "README.md" ] && continue
    echo
    echo "=== $f ==="
    cat "$f"
  done
} > "$SITE_DIR/changelog.txt"

echo "Wrote build-info.txt and changelog.txt to $SITE_DIR"
