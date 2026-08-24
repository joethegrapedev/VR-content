#!/bin/bash
#
# RSAF VR Warehouse Safety — copy trainee results off a Quest headset (macOS).
#
# The app records every run into an Excel spreadsheet stored on the headset.
# Double-click this file to copy those spreadsheets to your Desktop.

set -uo pipefail

readonly PACKAGE_ID="com.sp.fyp3a25.rsafvrwarehouse"
readonly REMOTE_DIR="/storage/emulated/0/Android/data/${PACKAGE_ID}/files/Output"
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

readonly BOLD=$'\033[1m'; readonly DIM=$'\033[2m'
readonly RED=$'\033[31m'; readonly GREEN=$'\033[32m'; readonly RESET=$'\033[0m'

die() {
    printf '\n%s  PROBLEM%s\n    %s\n' "$BOLD$RED" "$RESET" "$1"
    if [ $# -gt 1 ]; then
        printf '\n    %sWhat to do:%s\n' "$BOLD" "$RESET"
        shift
        for line in "$@"; do printf '      • %s\n' "$line"; done
    fi
    printf '\n%sPress Return to close this window.%s\n' "$DIM" "$RESET"
    read -r _
    exit 1
}

locate_adb() {
    local candidate
    for candidate in \
        "$(command -v adb 2>/dev/null || true)" \
        "$SCRIPT_DIR/platform-tools/adb" \
        "$HOME/Library/Android/sdk/platform-tools/adb" ; do
        if [ -n "$candidate" ] && [ -x "$candidate" ]; then
            printf '%s' "$candidate"; return 0
        fi
    done
    return 1
}

clear
printf '%s\n  Collect trainee results from a Quest headset\n%s\n' "$BOLD" "$RESET"

ADB="$(locate_adb)" || die "The Android tools are not set up on this Mac." \
    "Run 'install-on-quest-mac.command' first — it downloads them automatically."

printf '\n%s==> Connecting to the headset%s\n' "$BOLD" "$RESET"
"$ADB" start-server >/dev/null 2>&1
if ! "$ADB" wait-for-device shell true 2>/dev/null; then
    die "No headset is connected." \
        "Plug the headset into this Mac with a USB-C cable." \
        "Put it on and approve the 'Allow USB debugging' prompt."
fi
printf '    Connected.\n'

printf '\n%s==> Looking for results%s\n' "$BOLD" "$RESET"
listing="$("$ADB" shell "ls $REMOTE_DIR 2>/dev/null" | tr -d '\r')"
if [ -z "$listing" ]; then
    die "No results were found on this headset." \
        "Results only appear after somebody completes a training run." \
        "Check the app has been used on this particular headset."
fi

# Each run is stamped into a dated folder so repeated collections never
# overwrite a previous batch of results.
destination="$HOME/Desktop/RSAF Results $(date '+%Y-%m-%d %H%M')"
mkdir -p "$destination" || die "Could not create a folder on the Desktop."

copied=0
while IFS= read -r name; do
    [ -z "$name" ] && continue
    printf '    %s\n' "$name"
    if "$ADB" pull "$REMOTE_DIR/$name" "$destination/$name" >/dev/null 2>&1; then
        copied=$((copied + 1))
    else
        printf '      (could not copy this one)\n'
    fi
done <<< "$listing"

if [ "$copied" -eq 0 ]; then
    rmdir "$destination" 2>/dev/null
    die "Results were listed but none could be copied." \
        "Make sure the headset stayed connected, then try again."
fi

printf '\n%s  ✓  %s file(s) copied%s\n\n' "$BOLD$GREEN" "$copied" "$RESET"
printf '  Saved to:\n    %s\n' "$destination"
printf '\n  Open the .xls files with Excel, Numbers or Google Sheets.\n'
printf '\n%sPress Return to close this window.%s\n' "$DIM" "$RESET"
read -r _
open "$destination" 2>/dev/null || true
