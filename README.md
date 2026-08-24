# RSAF VR Warehouse Safety

A virtual-reality warehouse safety training app for Meta Quest headsets. A trainee
walks through a warehouse, finds thirteen safety hazards, and is scored on how many
they identify and how quickly. Results are saved to a spreadsheet on the headset.

Runs entirely on the headset. No PC, no cables and no internet connection are needed
once it is installed.

---

## Install it on a headset

**You do not need to understand code, Unity, or git to do this.**

| | |
|---|---|
| **Never done this before?** | Follow **[INSTALL.md](INSTALL.md)** — a step-by-step guide with no assumed knowledge. Allow about 30 minutes for the first headset, 5 minutes for each one after that. |
| **Done it before?** | Download `RSAF_VRWarehouse.apk` from the **[latest release](../../releases/latest)** and run `adb install -r RSAF_VRWarehouse.apk`. |

The quick version:

1. Turn on Developer Mode for the headset, once, using the Meta Horizon phone app.
2. Plug the headset into a computer with a USB-C cable.
3. From the [latest release](../../releases/latest), download
   `install-on-quest-windows.bat` (Windows) or `Install-on-Quest-Mac.zip` (Mac).
4. Double-click it. The installer fetches the app and does the rest — you do not need to
   download the 1 GB `.apk` yourself.

## Collect trainee results

The app writes every completed run into an Excel file **stored on the headset**.
To copy those onto a computer, plug the headset in and double-click:

- Windows → `install/get-results-windows.bat`
- Mac → `install/get-results-mac.command`

The spreadsheets land on your Desktop in a dated folder. See
[docs/HOW_IT_WORKS.md](docs/HOW_IT_WORKS.md#where-the-results-go) for what the
columns mean.

## Something went wrong

See **[docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)**. It covers the common
problems in plain English: the headset not being detected, the app not appearing in
the library, running out of space, and audio issues.

---

## What the app does

A full walkthrough is in **[docs/HOW_IT_WORKS.md](docs/HOW_IT_WORKS.md)**. In short:

- The trainee types their name, picks a **3-minute** or **10-minute** session, and is
  placed in a warehouse.
- Thirteen safety hazards are hidden around the scene. Finding one opens a multiple-choice
  question about the correct response.
- Each hazard is worth **40 points**, less **10 points** for each wrong answer.
- The session ends when all thirteen are handled or the timer runs out. Unfinished runs
  are recorded as **DNF**.
- Scores are appended to a leaderboard spreadsheet on the headset.

## Requirements

| | |
|---|---|
| Headset | Meta Quest 2, Quest 3, Quest 3S or Quest Pro |
| Free space on headset | About 2 GB |
| Computer | Windows 10/11 or macOS — only for installing, not for running |
| Cable | USB-C, **data-capable** (many charge-only cables will not work) |
| Meta account | Needed once, to switch on Developer Mode |

## Build details

| | |
|---|---|
| Package name | `com.sp.fyp3a25.rsafvrwarehouse` |
| Version | 0.1.0 |
| Architecture | `arm64-v8a` |
| Android target | SDK 32, minimum SDK 29 |
| Engine | Unity 2021.3.12f1, URP, IL2CPP |
| Signing | Android debug certificate, SHA-256 `01d8a1a3…73b5a2` |

Because the app is signed with a debug certificate, an **update will only install over
an existing copy if it was signed with the same key**. If an update is refused, uninstall
the old version first — see [docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md).

## Status

The build has been verified by inspection — correct architecture, valid signature,
correct VR manifest entries, and all 41 audio clips present — but **it has not yet been
run on a physical headset since the audio work landed**. The first person to install it
should confirm sound plays and the warehouse scene loads, and report back.

The app declares support for `quest|quest2` in its manifest because the Unity Oculus
plugin bundled with this project predates Quest 3. Sideloaded apps are not restricted by
that field, so it installs and runs on newer headsets regardless.

## For developers

This project was reconstructed from a compiled Windows build; no original Unity source
existed. See:

- **[docs/BUILDING.md](docs/BUILDING.md)** — how to rebuild the APK, and what you need first
- **[docs/REBUILD_BRIEF.md](docs/REBUILD_BRIEF.md)** — how the project was recovered
- **[docs/AUDIO.md](docs/AUDIO.md)** — the music, ambience and sound-effect system
- **[STATUS.md](STATUS.md)** — what was broken, and what fixed it

> **Note:** cloning this repository is *not* enough to rebuild the app. About 2.8 GB of
> recovered art assets are deliberately not stored in git. `docs/BUILDING.md` explains
> what that means in practice.

## Repository layout

```
INSTALL.md               Step-by-step install guide for non-technical users
install/                 Double-click installer and results-collection scripts
docs/                    How it works, troubleshooting, building, audio, rebuild history
UnityProject/            The Unity project (source and settings; large assets excluded)
tools/audio/             Generator that produces every music and sound-effect file
RSAF_VRWarehouse.exe     Original Windows build, kept for reference (Git LFS)
RSAF_VRWarehouse_Data/   Original Windows build data (Git LFS)
```

Installable builds are attached to [Releases](../../releases) rather than committed,
which keeps the repository small and the downloads free.
