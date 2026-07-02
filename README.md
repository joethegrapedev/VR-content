# VR Warehouse Safety

This repository contains the current Windows build of the VR Warehouse Safety application and a basic setup guide so it can be run on another machine without needing this laptop.

## What is included

- The executable: RSAF_VRWarehouse.exe
- The runtime data folder: RSAF_VRWarehouse_Data/
- Supporting runtime files such as the Oculus runtime components and Unity runtime files in the workspace root

> This repository is intended to preserve the current stable build as-is. No package updates or code changes have been applied.

## Prerequisites

On the target machine, install:

- Windows 10 or 11
- The Oculus PC app
- A compatible VR headset and USB/PC connection
- The latest graphics drivers for your GPU

## Running the application

1. Clone or download this repository.
2. Open the project folder.
3. Double-click RSAF_VRWarehouse.exe.
4. If the headset is not detected, open the Oculus app and make sure the headset is connected and authorized.

## Troubleshooting

- If the application does not start, make sure the Oculus runtime is installed and the headset is connected.
- If you see missing files, confirm that the full repository contents were downloaded, including RSAF_VRWarehouse_Data/.
- If the executable is blocked by Windows Defender, choose "Run anyway" after confirming the file is from a trusted source.

## Notes for future maintenance

- This repository currently contains the built application files rather than a full Unity source project export.
- If you later want to rebuild from source, add the Unity project files and record the Unity version used in this README.
- If the repository grows large, GitHub may require Git LFS for some binary files.

## GitHub upload commands

If you want to publish this folder to GitHub, run the following from the project folder:

```bash
git init
git add .
git commit -m "Initial import"
git branch -M main
git remote add origin https://github.com/<your-username>/<your-repo-name>.git
git push -u origin main
```

If you prefer, you can also create the repository on GitHub first and then point the remote to it.
