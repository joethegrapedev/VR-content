# Installer scripts

Double-click the file for your computer. Full instructions with screenshots-worth of
detail are in **[../INSTALL.md](../INSTALL.md)**.

| I want to... | Windows | Mac |
|---|---|---|
| Put the app on a headset | `install-on-quest-windows.bat` | `install-on-quest-mac.command` |
| Copy trainee results off a headset | `get-results-windows.bat` | `get-results-mac.command` |

## What the installer does

1. Looks for Google's Android device tools (`adb`). If they are not on the computer, it
   downloads them (about 10 MB) into a `platform-tools` folder here.
2. Looks for `RSAF_VRWarehouse.apk` next to the script, one folder up, or in Downloads.
   If it finds none, it downloads the published release (about 1 GB).
3. Waits for a headset to be connected and authorised.
4. Installs the app and reports what happened.

Nothing is installed system-wide. Deleting the `platform-tools` folder and the
downloaded `.apk` undoes everything on the computer's side.

## If a step fails

The scripts stop at the first problem and print what to do about it rather than a raw
error code. If the window closes too fast to read, open a terminal or command prompt,
drag the script into it and press Enter.

Common causes are listed in **[../docs/TROUBLESHOOTING.md](../docs/TROUBLESHOOTING.md)**.
The most frequent by far is a **USB-C cable that only carries power**, which looks
identical to the headset not being plugged in at all.

## Before you use these

Each headset needs **Developer Mode** turned on once, which requires a free Meta
developer account. That is [INSTALL.md Part 1](../INSTALL.md#part-1--unlock-the-headset-once-per-headset)
and it cannot be skipped or scripted — Meta requires it to be done by hand.
