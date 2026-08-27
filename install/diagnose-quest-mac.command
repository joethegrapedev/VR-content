#!/bin/bash
#
# RSAF VR Warehouse Safety — collect diagnostics from a Quest headset (macOS).
#
# Run this if the app will not install or will not launch. It gathers what is
# needed to work out why and saves it to a single file on your Desktop, which
# you can send on. It changes nothing on the headset.

set -uo pipefail

readonly PACKAGE_ID="com.sp.fyp3a25.rsafvrwarehouse"
readonly CAPTURE_SECONDS=25
readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

readonly BOLD=$'\033[1m'; readonly DIM=$'\033[2m'
readonly RED=$'\033[31m'; readonly GREEN=$'\033[32m'; readonly RESET=$'\033[0m'

REPORT="$HOME/Desktop/RSAF-diagnostics-$(date '+%Y-%m-%d-%H%M').txt"

die() {
    printf '\n%s  PROBLEM%s\n    %s\n' "$BOLD$RED" "$RESET" "$1"
    if [ $# -gt 1 ]; then
        printf '\n    %sWhat to do:%s\n' "$BOLD" "$RESET"; shift
        for l in "$@"; do printf '      • %s\n' "$l"; done
    fi
    printf '\n%sPress Return to close.%s\n' "$DIM" "$RESET"; read -r _; exit 1
}

locate_adb() {
    local c
    for c in "$(command -v adb 2>/dev/null || true)" \
             "$SCRIPT_DIR/platform-tools/adb" \
             "$HOME/Library/Android/sdk/platform-tools/adb"; do
        [ -n "$c" ] && [ -x "$c" ] && { printf '%s' "$c"; return 0; }
    done
    return 1
}

# Everything printed to the report is also echoed, so the operator can see
# progress instead of staring at a still window.
say() { printf '%s\n' "$1"; printf '%s\n' "$1" >> "$REPORT"; }
rule() { say ""; say "──────────────────────────────────────────────────────────"; }

clear
printf '%s\n  RSAF VR — headset diagnostics\n%s\n' "$BOLD" "$RESET"
printf '  This reads information from the headset. It changes nothing.\n'

ADB="$(locate_adb)" || die "The Android tools are not set up on this Mac." \
    "Run the installer first — it downloads them automatically."

: > "$REPORT"
say "RSAF VR Warehouse — diagnostic report"
say "Generated: $(date)"
say "Host: macOS $(sw_vers -productVersion 2>/dev/null)"

rule
printf '\n%s==> Checking the headset connection%s\n' "$BOLD" "$RESET"
"$ADB" start-server >/dev/null 2>&1
state="$("$ADB" devices | awk 'NR>1 && NF {print $2; exit}')"

case "$state" in
    device) say "Headset connection: OK" ;;
    unauthorized) die "The headset is connected but USB debugging is not approved." \
        "Put the headset on and tap Allow on the prompt inside it, then run this again." ;;
    *) die "No headset detected." \
        "Plug the headset into this Mac with a USB-C cable that carries data." \
        "Many charge-only cables give exactly this result." ;;
esac

rule
say "── Headset ──"
for prop in ro.product.model ro.product.device ro.build.version.release ro.build.version.sdk; do
    say "$prop = $("$ADB" shell getprop $prop 2>/dev/null | tr -d '\r')"
done

# Free space matters: the app needs about 2GB and a short install can look
# like a launch failure rather than a storage failure.
say ""
say "── Storage ──"
"$ADB" shell df -h /storage/emulated/0 2>/dev/null | tr -d '\r' | while IFS= read -r l; do say "$l"; done

rule
printf '\n%s==> Checking the app%s\n' "$BOLD" "$RESET"
say "── Installed app ──"
if "$ADB" shell pm list packages 2>/dev/null | tr -d '\r' | grep -q "$PACKAGE_ID"; then
    say "Installed: YES"
    "$ADB" shell dumpsys package "$PACKAGE_ID" 2>/dev/null | tr -d '\r' \
      | grep -E "versionName|versionCode|primaryCpuAbi|firstInstallTime|lastUpdateTime|codePath" \
      | while IFS= read -r l; do say "  ${l##*( )}"; done
else
    say "Installed: NO  <-- the app is not on this headset"
    say ""
    say "Nothing further can be checked until it is installed."
    printf '\n%s  The app is not installed on this headset.%s\n' "$BOLD$RED" "$RESET"
    printf '  Run the installer first, then run this again.\n'
    printf '\n  Report saved to:\n    %s\n' "$REPORT"
    printf '\n%sPress Return to close.%s\n' "$DIM" "$RESET"; read -r _; exit 0
fi

rule
printf '\n%s==> Launching the app and recording what happens%s\n' "$BOLD" "$RESET"
printf '    Put the headset ON now and watch it.\n'
printf '    Recording for %s seconds...\n' "$CAPTURE_SECONDS"

say "── Launch attempt ──"
"$ADB" shell am force-stop "$PACKAGE_ID" >/dev/null 2>&1
"$ADB" logcat -c >/dev/null 2>&1

# Starting through the activity manager bypasses the Oculus launcher. If the app
# runs this way but not from the library, the fault is in the launcher metadata
# rather than the app itself — which is the single most useful thing to know.
launch_out="$("$ADB" shell monkey -p "$PACKAGE_ID" -c android.intent.category.LAUNCHER 1 2>&1 | tr -d '\r')"
say "Launch command output:"
say "$launch_out"

"$ADB" logcat -v time > "$HOME/.rsaf_logcat.tmp" 2>/dev/null &
LOG_PID=$!
sleep "$CAPTURE_SECONDS"
kill "$LOG_PID" 2>/dev/null; wait "$LOG_PID" 2>/dev/null

say ""
say "── Is it running now? ──"
running="$("$ADB" shell pidof "$PACKAGE_ID" 2>/dev/null | tr -d '\r')"
if [ -n "$running" ]; then
    say "RUNNING (pid $running) — the app started via the activity manager."
    say "If it will not start from the headset's library, the launcher is refusing it,"
    say "not the app crashing."
else
    say "NOT RUNNING — the app did not stay up."
fi

say ""
say "── Relevant log lines ──"
grep -iE "rsaf|unity|il2cpp|oculus|vr|FATAL|AndroidRuntime|ActivityManager|crash|abort" \
    "$HOME/.rsaf_logcat.tmp" 2>/dev/null | tail -250 >> "$REPORT"
rm -f "$HOME/.rsaf_logcat.tmp"

printf '\n%s  ✓  Report saved%s\n\n' "$BOLD$GREEN" "$RESET"
printf '  %s\n' "$REPORT"
printf '\n  Send that file back. It contains no personal information —\n'
printf '  just the headset model, the app version and the launch log.\n'
printf '\n%sPress Return to close.%s\n' "$DIM" "$RESET"
read -r _
open -R "$REPORT" 2>/dev/null || true
