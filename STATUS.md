# Rebuild status

Reconstructing a working Meta Quest build of RSAF VR Warehouse Safety from the compiled
Windows player, which is the only surviving copy of the game.

## Done and verified

**The Unity project is recovered and renders correctly.** Headless validation passes:

```
## Render pipeline
- Graphics default pipeline: UniversalRenderPipelineAsset:URP-Balanced
- Quality[0] Performant     -> URP-Performant
- Quality[1] Balanced       -> URP-Balanced
- Quality[2] High Fidelity  -> URP-HighFidelity

## Materials
- Materials scanned: 348
- Broken (null or error shader): 0        (was 298)

## Scenes
- Scenes in build settings: 6
- Every scene: enabled cameras present, culling masks non-zero, 0 missing scripts

## RESULT: PASS
```

`Assembly-CSharp.dll` compiles clean at **727,552 bytes** vs the original shipped
**728,064** — a 512-byte delta across ~40,000 lines of recovered C#.

### Root causes found and fixed

| # | Cause | Evidence | Fix |
|---|---|---|---|
| F1 | All 14 persistence writes used `Application.dataPath`, read-only on Android. The throw happens **before** `SceneManager.LoadScene("5SD Map")`, so the app could never enter the warehouse scene | `KeyboardTypingArea.cs:318-328`, `TypingArea.cs:27-37` | New `SavePaths` helper; all sites moved to `persistentDataPath` |
| F2 | Decompiled sources were C# 12; Unity 2021.3 is C# 9 | primary constructors in `Myproject.cs` | Re-decompiled with `-lv CSharp9_0` + CI guard |
| F4 | All 51 extracted shaders were `//DummyShaderTextExporter` stubs | shader file headers | 36 package-provided stubs deleted; 298 materials + 51 URP renderer refs remapped to real shader GUIDs |
| F5 | No render pipeline asset assigned anywhere — a URP project with none renders nothing | validation report | Recovered `URP-Performant/Balanced/HighFidelity` assigned to their name-matched quality levels |
| F6 | XR configured for Standalone only; Android had no loader, so VR rendering never starts | `boot.config` pins `OculusXRPlugin` for Standalone only | Oculus loader enabled on the Android target |
| F7 | 4,616 dangling script references across 144 files after removing colliding package DLLs. `UnityEngine.UI` alone was 3,377 — all UI in all scenes was missing | GUID ownership scan | Unity's MD4 script-fileID derived to map every DLL reference back to its package source file |
| F3 | Previous APK was ARMv7 + Mono; a 32-bit address space cannot hold this scene's ~1.1 GB of assets | repo README | ARM64 + IL2CPP + ASTC configured |

Other preserved details: fixed timestep 1/72 s (Quest 72 Hz), 5 custom tags, layers 8-11
(`HandPlayer`/`Hand`/`Grabbable`/`Grabbing`, AutoHand's convention).

## Blocked: APK build — one `sudo` command needed

Everything is configured (IL2CPP, ARM64, Vulkan+GLES3, Linear, ASTC, Oculus loader, all six
scenes queued). The Android SDK, NDK r21d and Gradle are installed and **accepted by Unity**.
The build stops only at:

```
UnityException: JDK not found
```

This Unity is a manual-installer layout (`/Applications/Unity/Unity.app`), not Hub-managed, so
its Android module shipped without the bundled `OpenJDK` folder. Unity rejects every *external*
JDK path offered — three different valid JDK 11 installs, via `AndroidExternalToolsSettings`,
`EditorPrefs`, and the `-jdkPath` command-line argument — while accepting the SDK, NDK and Gradle
paths through the same API. It wants the JDK at its embedded location.

A correct JDK 11 (Temurin 11.0.32, aarch64) is already downloaded and verified at:
`.rebuild/tools/jdk/jdk-11.0.32+9/Contents/Home`

**To unblock, run:**

```bash
sudo cp -R "$(pwd)/.rebuild/tools/jdk/jdk-11.0.32+9/Contents/Home" \
  /Applications/Unity/PlaybackEngines/AndroidPlayer/OpenJDK
```

(run from the repository root). Then the build is:

```bash
cd .worktrees/rebuild
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -quit \
  -projectPath "$PWD/UnityProject" \
  -executeMethod BuildQuestApk.Build -logFile "$PWD/build.log"
```

The APK lands at `UnityProject/Builds/Android/RSAF_VRWarehouse.apk` with a size report in
`UnityProject/build-report.md`.

## Not yet done

- **Custom shader** `Outlined/Silhouette Only` is still a stub, used by `Highlight.mat` and
  `Highlight 2.mat` (object highlighting). Cosmetic; everything else renders.
- **Memory budget for `5SD Map` not yet measured** — needs a successful build's `BuildReport`.
  This is what confirms or refutes F3.
- **Recovered binary assets (2.8 GB) are not committed** pending a Git LFS budget decision.
  Only source-like files are in git, so the project is not yet reproducible from a fresh clone.
- **Nothing is verified on hardware.** No headset was available. Every claim above is either a
  headless Editor assertion or a static file check. Rendering correctness in-headset,
  hand tracking, grabbing, and the training flow all remain unverified.

## Reproducibility

`.rebuild/` holds the full method for every step: `EXTRACTION_STEPS.md`, `SHADER_REMAP.md`,
`SCRIPT_REF_REPAIR.md`, `ANDROID_TOOLCHAIN.md`, plus the scripts used.
