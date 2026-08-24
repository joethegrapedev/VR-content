#!/bin/bash
#
# RSAF VR Warehouse Safety — one-click installer for macOS.
#
# Double-click this file in Finder. It finds (or downloads) the Android tools,
# finds (or downloads) the app, then installs it onto a connected Quest headset.
#
# Nothing is installed system-wide: any tools it downloads land in a
# "platform-tools" folder next to this script and can be deleted afterwards.

set -uo pipefail

# ---------------------------------------------------------------- configuration
readonly APP_NAME="RSAF VR Warehouse Safety"
readonly APK_FILENAME="RSAF_VRWarehouse.apk"
readonly PACKAGE_ID="com.sp.fyp3a25.rsafvrwarehouse"
readonly RELEASE_REPO="joethegrapedev/VR-content"
readonly APK_URL="https://github.com/${RELEASE_REPO}/releases/latest/download/${APK_FILENAME}"
readonly TOOLS_URL="https://dl.google.com/android/repository/platform-tools-latest-darwin.zip"
readonly DEVICE_WAIT_SECONDS=120

readonly SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR" || exit 1

# ---------------------------------------------------------------- presentation
readonly BOLD=$'\033[1m'; readonly DIM=$'\033[2m'
readonly RED=$'\033[31m'; readonly GREEN=$'\033[32m'
readonly YELLOW=$'\033[33m'; readonly RESET=$'\033[0m'

step()  { printf '\n%s==> %s%s\n' "$BOLD" "$1" "$RESET"; }
info()  { printf '    %s\n' "$1"; }
warn()  { printf '    %s%s%s\n' "$YELLOW" "$1" "$RESET"; }
ok()    { printf '    %s%s%s\n' "$GREEN" "$1" "$RESET"; }

# Prints a failure with what to do about it, then holds the window open so a
# double-clicked script does not vanish before the message can be read.
die() {
    printf '\n%s%s  PROBLEM%s\n' "$BOLD$RED" "──────" "$RESET"
    printf '    %s\n' "$1"
    if [ $# -gt 1 ]; then
        printf '\n    %sWhat to do:%s\n' "$BOLD" "$RESET"
        shift
        for line in "$@"; do printf '      • %s\n' "$line"; done
    fi
    printf '\n%sPress Return to close this window.%s\n' "$DIM" "$RESET"
    read -r _
    exit 1
}

# ---------------------------------------------------------------- android tools
# Returns the path to a usable adb, downloading Google's platform-tools if no
# copy is already present. Preference order keeps an existing developer install
# ahead of anything we fetch, so nobody ends up with two divergent toolchains.
locate_adb() {
    local candidate
    for candidate in \
        "$(command -v adb 2>/dev/null || true)" \
        "$SCRIPT_DIR/platform-tools/adb" \
        "$HOME/Library/Android/sdk/platform-tools/adb" ; do
        if [ -n "$candidate" ] && [ -x "$candidate" ]; then
            printf '%s' "$candidate"
            return 0
        fi
    done
    return 1
}

download_android_tools() {
    step "Downloading Android device tools (about 10 MB, one time only)"
    local zip="$SCRIPT_DIR/platform-tools.zip"

    if ! curl --fail --location --progress-bar --output "$zip" "$TOOLS_URL"; then
        rm -f "$zip"
        die "Could not download the Android tools from Google." \
            "Check that this Mac is connected to the internet." \
            "If you are on a restricted work network, try a home or mobile connection."
    fi

    if ! unzip -q -o "$zip" -d "$SCRIPT_DIR"; then
        rm -f "$zip"
        die "The downloaded tools archive could not be opened." \
            "Delete 'platform-tools.zip' and 'platform-tools' next to this script, then run it again."
    fi

    rm -f "$zip"
    chmod +x "$SCRIPT_DIR/platform-tools/adb" 2>/dev/null || true
    ok "Tools ready."
}

# ---------------------------------------------------------------- the app file
# Finds the APK beside the script or in Downloads; fetches the published release
# if neither has it. Downloading is last so an already-vetted local copy always wins.
locate_apk() {
    local candidate
    for candidate in \
        "$SCRIPT_DIR/$APK_FILENAME" \
        "$SCRIPT_DIR/../$APK_FILENAME" \
        "$HOME/Downloads/$APK_FILENAME" ; do
        if [ -f "$candidate" ]; then
            printf '%s' "$candidate"
            return 0
        fi
    done
    return 1
}

download_apk() {
    step "Downloading $APP_NAME (about 1 GB — this can take a while)"
    info "Source: $APK_URL"
    local target="$SCRIPT_DIR/$APK_FILENAME"

    if ! curl --fail --location --progress-bar --output "$target.part" "$APK_URL"; then
        rm -f "$target.part"
        die "Could not download the app." \
            "Check this Mac's internet connection." \
            "Or download '$APK_FILENAME' manually from:" \
            "https://github.com/${RELEASE_REPO}/releases/latest" \
            "then put it in the same folder as this script and run it again."
    fi

    mv "$target.part" "$target"
    ok "Download complete."
}

# ---------------------------------------------------------------- headset
# Blocks until exactly one authorised device appears. Distinguishes "no device"
# from "device present but debugging not yet approved", because the fix differs.
wait_for_headset() {
    local waited=0 state
    while [ "$waited" -lt "$DEVICE_WAIT_SECONDS" ]; do
        state="$("$ADB" devices | awk 'NR>1 && NF {print $2; exit}')"

        case "$state" in
            device)
                return 0
                ;;
            unauthorized)
                if [ "$waited" -eq 0 ] || [ $((waited % 10)) -eq 0 ]; then
                    warn "Headset found, but debugging has not been approved yet."
                    info "Put the headset ON and tap 'Allow' on the prompt inside it."
                fi
                ;;
            "")
                if [ "$waited" -eq 0 ]; then
                    info "Waiting for a headset... plug it into this Mac with a USB-C cable."
                fi
                ;;
        esac

        sleep 2
        waited=$((waited + 2))
    done
    return 1
}

# ---------------------------------------------------------------- main
clear
printf '%s\n' "$BOLD"
printf '  %s\n' "$APP_NAME"
printf '  Installer for Meta Quest — macOS\n'
printf '%s\n' "$RESET"
printf '  %sBefore you start:%s\n' "$BOLD" "$RESET"
printf '    1. Developer Mode must be turned on for the headset\n'
printf '       (done once, in the Meta Horizon phone app)\n'
printf '    2. The headset must be plugged into this Mac with a USB-C cable\n'
printf '    3. Put the headset on when prompted, to approve the connection\n'

step "Checking for Android device tools"
if ADB="$(locate_adb)"; then
    ok "Found: $ADB"
else
    info "Not found on this Mac."
    download_android_tools
    ADB="$(locate_adb)" || die "Android tools installed but 'adb' still cannot be found." \
        "Delete the 'platform-tools' folder next to this script and run it again."
    ok "Found: $ADB"
fi
readonly ADB

step "Locating the app"
if APK="$(locate_apk)"; then
    ok "Found: $APK"
else
    info "No local copy of $APK_FILENAME found."
    download_apk
    APK="$(locate_apk)" || die "The app was downloaded but cannot be found."
fi
readonly APK

apk_size_mb=$(( $(stat -f%z "$APK") / 1048576 ))
info "Size: ${apk_size_mb} MB"
if [ "$apk_size_mb" -lt 100 ]; then
    die "'$APK_FILENAME' is only ${apk_size_mb} MB, which is far too small to be the real app." \
        "The download was probably interrupted." \
        "Delete that file and run this script again."
fi

step "Connecting to the headset"
"$ADB" start-server >/dev/null 2>&1
if ! wait_for_headset; then
    die "No headset became ready within ${DEVICE_WAIT_SECONDS} seconds." \
        "Check the USB-C cable is firmly connected to both the Mac and the headset." \
        "Use a cable that supports data — some charge-only cables will not work." \
        "Confirm Developer Mode is on for this headset in the Meta Horizon phone app." \
        "Put the headset on and look for an 'Allow USB debugging' prompt."
fi
ok "Headset connected and authorised."

step "Installing $APP_NAME"
info "This takes several minutes for a file this size. Do not unplug the cable."
install_log="$(mktemp -t rsaf_install)"
if "$ADB" install -r -g "$APK" 2>&1 | tee "$install_log"; then
    if grep -qi "Success" "$install_log"; then
        rm -f "$install_log"
    else
        failure="$(grep -io 'INSTALL_FAILED_[A-Z_]*' "$install_log" | head -1)"
        rm -f "$install_log"
        case "$failure" in
            INSTALL_FAILED_INSUFFICIENT_STORAGE)
                die "The headset does not have enough free space." \
                    "The app needs about 2 GB free. Delete some apps or videos on the headset and try again." ;;
            INSTALL_FAILED_UPDATE_INCOMPATIBLE|INSTALL_FAILED_VERSION_DOWNGRADE)
                die "A different build of this app is already installed." \
                    "Remove the old one first, then run this script again:" \
                    "$ADB uninstall $PACKAGE_ID" ;;
            *)
                die "The installation did not complete." \
                    "Check the messages above for the reason." \
                    "Make sure the headset stayed awake and connected throughout." ;;
        esac
    fi
else
    rm -f "$install_log"
    die "The installation command failed." \
        "Make sure the headset stayed connected and did not go to sleep." \
        "Then run this script again."
fi

printf '\n%s  ✓  INSTALLED SUCCESSFULLY%s\n\n' "$BOLD$GREEN" "$RESET"
printf '  To open it in the headset:\n'
printf '    1. Put the headset on\n'
printf '    2. Open the %sApp Library%s\n' "$BOLD" "$RESET"
printf '    3. Change the filter at the top right to %sUnknown Sources%s\n' "$BOLD" "$RESET"
printf '    4. Launch %sRSAF_VRWarehouse%s\n' "$BOLD" "$RESET"
printf '\n  You can unplug the cable now — the app runs entirely on the headset.\n'
printf '\n%sPress Return to close this window.%s\n' "$DIM" "$RESET"
read -r _
