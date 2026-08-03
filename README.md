# RSAF VR Warehouse Safety

A VR warehouse safety training application built in Unity (2021.3.12f1). Two ways to run it are provided below — a **standalone Meta Quest build** (recommended: no PC required at runtime, works from any host OS) and the original **Windows PC VR build** (Oculus Rift/Quest Link, Windows only).

## Option 1: Meta Quest standalone (recommended)

Once installed, this runs entirely on the headset — no PC, no Oculus/Meta Link software, no cables needed at runtime. Any computer (Windows, Mac, or Linux) can be used to do the one-time install via `adb`.

### Prerequisites

- A Meta Quest headset (2, 3, or Pro) with **Developer Mode** enabled on your Meta account and the headset (see [Meta's developer mode guide](https://developer.oculus.com/documentation/native/android/mobile-device-setup/))
- A USB-C cable to connect the headset to your computer
- [Android Platform Tools](https://developer.android.com/tools/releases/platform-tools) (`adb`) installed, or [SideQuest](https://sidequestvr.com/) if you prefer a GUI

### Install steps

1. Download `RSAF_VRWarehouse.apk` from the [latest release](../../releases/latest).
2. Connect the headset via USB-C and put it on — approve the "Allow USB Debugging" prompt that appears in the headset.
3. Install via command line:
   ```bash
   adb install RSAF_VRWarehouse.apk
   ```
   Or via SideQuest: drag-and-drop the `.apk` file onto the SideQuest window.
4. In the headset, go to **Library → Unknown Sources** (or **App Library** filtered to "Unknown Sources") and launch **RSAF_VRWarehouse**.

### Troubleshooting

- `adb devices` should list your headset before running `adb install` — if it shows nothing, re-check Developer Mode and USB debugging authorization on the headset.
- If install fails with `INSTALL_FAILED_INSUFFICIENT_STORAGE`, free up space on the headset.
- This build targets ARMv7 (32-bit) — this is intentional (matches the original app's Mono scripting backend) and works fine for sideloaded, non-Store use on all current Quest headsets.

## Option 2: Windows PC VR build (Oculus Rift / Quest Link)

The original Windows Unity Player build, for use with the Oculus PC app. **Windows only** — Meta's PC Link software has no macOS equivalent, so this option will not work on a Mac.

### Prerequisites

- Windows 10 or 11
- The Oculus PC app
- A compatible VR headset connected via USB/Link cable or Air Link
- Up-to-date GPU drivers

### Install steps

1. Download `RSAF_VRWarehouse_Windows.zip` from the [latest release](../../releases/latest).
2. Extract it anywhere on your PC.
3. Double-click `RSAF_VRWarehouse.exe`.
4. If the headset isn't detected, open the Oculus app first and confirm the headset shows as connected.

### Troubleshooting

- If the app won't start, confirm the Oculus PC app is running and the headset is connected there first.
- If you see missing-file errors, make sure the full zip was extracted (including the `RSAF_VRWarehouse_Data/` folder) rather than run in place inside the archive.
- If Windows Defender blocks the executable, choose "Run anyway" after confirming the download is from this repository's release page.

## About this repository

- `RSAF_VRWarehouse.exe` / `RSAF_VRWarehouse_Data/` in this repository are the original Windows build files (Git LFS), kept for reference and as the source used to produce the Windows release zip.
- The Quest APK was produced by reconstructing a working Unity project from this Windows build (via asset/script recovery) and rebuilding for the Android/Quest target, since no original Unity source project was available. Game logic and assets are preserved; some visual details (e.g. certain shaders) may differ slightly from the original PC build as a result.
- Unity Editor version: **2021.3.12f1**.

## Repository layout

```
RSAF_VRWarehouse.exe          Windows executable (Git LFS)
RSAF_VRWarehouse_Data/        Windows runtime data (Git LFS)
MonoBleedingEdge/, *.dll      Windows/Mono runtime support files
```

Downloadable builds (Quest `.apk` and Windows `.zip`) are attached to [Releases](../../releases) rather than committed to git, to keep the repository itself lightweight.
