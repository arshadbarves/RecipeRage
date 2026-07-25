#!/usr/bin/env bash
# Playcenter vendor-firewall gates. Exits non-zero on violation.
set -euo pipefail

CORE_DIR="Assets/Playcenter/MobileCore/Runtime/Core"
PATTERN='using (UnityEngine|VContainer|Unity\.Netcode|Epic|Firebase|Cysharp)'

if grep -rnE "$PATTERN" "$CORE_DIR" --include='*.cs'; then
    echo "GATE FAIL: vendor using found under $CORE_DIR" >&2
    exit 1
fi

echo "GATE PASS: $CORE_DIR is vendor-free"
