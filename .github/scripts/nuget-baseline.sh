#!/usr/bin/env bash
# Feature 017: prints the package-validation baseline of a package, taken from nuget.org (what users have today),
# not from git tags (a tag can exist before or without its package being published).
#
#   nuget-baseline.sh <PackageId> [<ExclusiveUpperBound>]
#
# - Prints the highest stable (no prerelease) version published on nuget.org, optionally only versions lower than
#   <ExclusiveUpperBound> (the version being released, so a re-run never compares a package with itself).
# - Prints nothing when the package is not on nuget.org (404) or no version qualifies: no baseline.
# - Fails on any other error, so a nuget.org outage never silently skips package validation.
set -euo pipefail

ID="$1"
BELOW="${2:-}"
LOWER_ID=$(printf '%s' "$ID" | tr '[:upper:]' '[:lower:]')
INDEX=$(mktemp)
trap 'rm -f "$INDEX"' EXIT

STATUS=$(curl -sS -o "$INDEX" -w '%{http_code}' "https://api.nuget.org/v3-flatcontainer/${LOWER_ID}/index.json")
case "$STATUS" in
  200) ;;
  404) exit 0 ;;
  *) echo "nuget.org returned HTTP $STATUS for $ID" >&2; exit 1 ;;
esac

# Parsing errors fail the script (no `|| true` here); only "no version qualifies" yields an empty result.
python3 - "$INDEX" "$BELOW" <<'PY'
import json, sys

def key(version):
    return tuple(int(part) for part in version.split('.'))

versions = json.load(open(sys.argv[1], encoding='utf-8'))['versions']
stable = [v for v in versions if '-' not in v]
below = sys.argv[2]
if below:
    stable = [v for v in stable if key(v) < key(below)]
if stable:
    print(max(stable, key=key))
PY
