#!/usr/bin/env bash
# Rebuilds the scripted history of this benchmark repository, deterministically.
#
# The history is part of the benchmark's input: churn, ownership, knowledge freshness, change coupling, sprint trends
# and history-only findings are read from it. Its 26 commits (2026-07-27 .. 2026-09-04, three two-week
# sprints) are stored as patches in ./patches (NNNN.patch, one per commit, in order, as written by
# `git format-patch -k`), each carrying its fictional author and date. This script applies them on top of the
# key-first commit with the committer set to the author and the committer date to the author date, and re-creates the
# sprint release tags. Every input of a commit object is fixed, so the result is byte-identical to the published
# history: the script checks the final commit id and every tag target.
#
# The authors are fictional (neutral invented names under the reserved .invalid domain). Commits after the scripted
# history (the answer key's maintenance, this script, the journal) are real benchmark maintenance.
#
# Usage: benchmark/history/build-history.sh <new-directory>
#   Run from any full (non-shallow) clone of this repository; <new-directory> must not exist. The rebuilt history is
#   left on branch `scripted` in <new-directory>.

set -euo pipefail

readonly BASE=da6e917fae400fda378d7791cf1d5e78591eca66
readonly EXPECTED_HEAD=fedfd1a1ebff7fe016f7c571f45ab519c98bb876
# tag, number of scripted commits it sits on, expected commit id
readonly TAGS=(
    "v0.1.0 10 9f4cb576ed894b7a1bc2f2ac29f3ca16f1689101"
    "v0.2.0 16 e91f36ac04db35511e5c159c080b1b92f86a2877"
    "v0.3.0 26 fedfd1a1ebff7fe016f7c571f45ab519c98bb876"
)

here="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source_repo="$(git -C "$here" rev-parse --show-toplevel)"
target="${1:?usage: build-history.sh <new-directory>}"
if [[ -e "$target" ]]; then
    echo "error: $target already exists" >&2
    exit 1
fi

work="$(mktemp -d)"
trap 'rm -rf "$work"' EXIT
git clone --quiet --no-checkout --no-tags "$source_repo" "$target"
cd "$target"
git checkout --quiet -b scripted "$BASE"

n=0
while IFS= read -r patch; do
    n=$((n + 1))
    author="$(sed -n 's/^From: //p' "$patch" | head -n 1)"
    date="$(sed -n 's/^Date: //p' "$patch" | head -n 1)"
    name="${author% <*}"
    email="${author##*<}"
    email="${email%>}"
    input="$patch"
    
    GIT_COMMITTER_NAME="$name" GIT_COMMITTER_EMAIL="$email" GIT_COMMITTER_DATE="$date" \
        git -c commit.gpgsign=false -c core.autocrlf=false \
        am --quiet --keep --keep-cr --whitespace=nowarn --committer-date-is-author-date "$input"
    for entry in "${TAGS[@]}"; do
        read -r tag at expected <<<"$entry"
        if [[ "$at" == "$n" ]]; then
            GIT_COMMITTER_NAME="$name" GIT_COMMITTER_EMAIL="$email" GIT_COMMITTER_DATE="$date" \
                git -c tag.gpgsign=false tag -f -a "$tag" -m "Release ${tag#v}" HEAD >/dev/null
            actual="$(git rev-parse "$tag^{commit}")"
            if [[ "$actual" != "$expected" ]]; then
                echo "error: $tag is $actual, expected $expected" >&2
                exit 1
            fi
        fi
    done
done < <(find "$here/patches" -name '*.patch' | LC_ALL=C sort)

head="$(git rev-parse HEAD)"
if [[ "$head" != "$EXPECTED_HEAD" ]]; then
    echo "error: rebuilt history ends at $head, expected $EXPECTED_HEAD" >&2
    exit 1
fi
echo "rebuilt $n scripted commits on top of ${BASE:0:7}; HEAD $head matches; ${#TAGS[@]} tags verified"
