# Mission: Rebuild RSAF VR Warehouse Safety into a working Meta Quest APK — entirely in the cloud

You are taking over a broken VR training application. Work autonomously for as long as it takes.
Do not stop at the first plausible fix — drive to a reproducible, evidence-backed build.

**Repository:** `https://github.com/joethegrapedev/VR-content` (PUBLIC)
**Work branch:** branch off `add-quest-build`

## CRITICAL EXECUTION CONSTRAINT

**You have no access to the user's machine, and you must never ask for it.** Everything runs in
the cloud. Your sandbox is where you *author* code; **GitHub Actions is where Unity actually
runs.** You do not install Unity in your own sandbox — you write workflows, push them, and read
the results with `gh run list` / `gh run view --log` / `gh run download`.

Your iteration loop is: **edit → commit → push → watch the Actions run → read logs → fix → repeat.**
Treat CI logs and CI artifacts as your only source of truth about whether something works.

The only things you may ask the user for are browser-only actions they alone can perform
(Unity licensing, paying for a quota). Batch those into a single clear request as early as
possible so you are not blocked twice.

---

## 1. The symptom

A previously-built Quest `.apk` starts, plays its opening audio narration, then renders **pure
black forever**. It does not obviously crash — audio is heard, then nothing renders. Your job is
to find the true cause and ship a Quest APK where every scene renders and every feature works.

---

## 2. Ground truth about this repository — verified, do not re-litigate

**The repo contains NO source code.** It is a compiled Unity **Windows** player and nothing else.
No `Assets/`, no `ProjectSettings/`, no `.cs`, no `.unity`.

Verified contents:

```
RSAF_VRWarehouse.exe                       640 KB   Windows player executable
UnityPlayer.dll                             28 MB   Unity 2021.3.12f1 runtime
MonoBleedingEdge/                          8.7 MB   Mono runtime + configs
RSAF_VRWarehouse_Data/                     1.7 GB   ALL GAME CONTENT LIVES HERE
  ├── globalgamemanagers(.assets/.resS)             build settings, scene list, quality settings
  ├── level0 .. level5                              the six scenes, in build order
  ├── sharedassets0..5.assets(+.resS)               meshes, textures, audio (sharedassets5.resS = 858 MB)
  ├── resources.assets(+.resS)
  ├── Managed/                                      153 managed DLLs incl. Assembly-CSharp.dll (728 KB)
  ├── Plugins/x86_64/                               OVRPlugin.dll, OculusXRPlugin.dll, UnityMockHMD.dll  (WINDOWS ONLY)
  ├── UnitySubsystems/                              OculusXRPlugin, UnityOpenXR, UnityMockHMD manifests
  ├── Resources/                                    unity default resources, unity_builtin_extra
  ├── Output/                                       RSAF_Leaderboard.xls, RSAF_VIP_Leaderboard.xls
  ├── StreamingAssets/UnityServicesProjectConfiguration.json
  ├── _MyProject/saves/                             AudioSettings.json, PlayerInfo.json  (runtime save format)
  ├── boot.config                                   xrsdk-pre-init-library=OculusXRPlugin
  └── app.info                                      "SP_FYP_3A25" / "RSAF_VRWarehouse"
```

**All of this is on GitHub via Git LFS and is downloadable in CI** — verified against the LFS
batch API. Use `actions/checkout` with `lfs: true`.

**Scene list, read from `globalgamemanagers` — authoritative build order:**

| Index | Data file | Scene path |
|---|---|---|
| 0 | `level0` | `Assets/_MyProject/Scenes/Menu.unity` |
| 1 | `level1` | `Assets/_MyProject/Scenes/Audio Settings.unity` |
| 2 | `level2` | `Assets/_MyProject/Scenes/Leaderboard.unity` |
| 3 | `level3` | `Assets/_MyProject/Scenes/Tutorial.unity` |
| 4 | `level4` | `Assets/_MyProject/Scenes/Keyboard.unity` |
| 5 | `level5` | `Assets/_MyProject/Scenes/5SD Map.unity` |

`5SD Map` is the main warehouse: 130 MB of scene data over ~1.1 GB of shared assets. **This is
almost certainly the scene that goes black.**

**Stack, identified from `Managed/` and `ScriptingAssemblies.json`:**

- Unity **2021.3.12f1**
- **Universal Render Pipeline (URP)** — `globalgamemanagers` references the full `Hidden/Universal Render Pipeline/*` shader set
- **Oculus XR Plugin** + XR Management; OpenXR and MockHMD subsystems also present
- **AutoHand** (paid Asset Store VR interaction framework) — `AutoHandPlayer`, `AutoHandExtensions`,
  `AutoHandSettings`, `AutoHandPlayerForceArea`, `AutoInputModule`, `AutoPose`, `Autohand` namespace.
  This is the core interaction layer: hands, grabbing, locomotion.
- Cinemachine, Unity Input System, Post-processing, Unity Services Core, TextMeshPro
- **NPOI** (+OOXML/OpenXml4Net/OpenXmlFormats) — writes the `.xls` leaderboards in `Output/`
- Newtonsoft.Json — the `_MyProject/saves/*.json` persistence
- NaughtyAttributes, ICSharpCode.SharpZipLib

**What does not exist anywhere:** the original Unity project, and the "reconstructed" project the
README claims produced the previous APK. Both are gone — confirmed absent from the repo and from
the user's machine. **The previous rebuild is lost; you are redoing the reconstruction from the
binaries.** Do not spend time hunting for it.

---

## 3. Cloud architecture you are building

```
  Your sandbox            GitHub Actions (ubuntu-latest, free/unlimited — repo is public)
  ────────────            ──────────────────────────────────────────────────────────────
  author workflows  ───►  Job A: recover    AssetRipper + ilspycmd → Unity project → commit
  author C# tools   ───►  Job B: validate   GameCI editor image → headless Editor tests
  read CI logs      ◄───  Job C: build      game-ci/unity-builder → Android APK
  iterate                 Job D: verify     static APK checks → attach to GitHub Release
```

Key references: **GameCI** (`game-ci/unity-builder`, `game-ci/unity-test-runner`,
`game-ci/unity-request-activation-file`) and the `unityci/editor` Docker images. Verify that a
`2021.3.12f1` Android image tag actually exists; if not, use the nearest `2021.3.x` and
**document the deviation prominently**.

### Constraints you must engineer around

1. **Runner disk is ~14 GB free by default.** Use `jlumbroso/free-disk-space` (frees ~30 GB) at
   the top of every heavy job. Still tight — see #2 for the fix.
2. **Extract ONCE.** The 1.7 GB original build is only needed for the initial recovery. Once the
   Unity project is committed, no later job should ever check out the original build data again.
   Design the workflows so build/validate jobs use a sparse or filtered checkout that excludes
   `RSAF_VRWarehouse_Data/`. This is the single most important decision for cost and runtime.
3. **LFS bandwidth is metered and small on the free tier.** Cache LFS objects with `actions/cache`
   keyed on the output of `git lfs ls-files -l`. Combined with #2, the 1.7 GB should be pulled
   essentially once, ever.
4. **6-hour limit per job.** Cache `Library/` with `actions/cache` — the first Unity import of
   this project will be very slow, subsequent ones fast. Split recovery, validation, and build
   into separate jobs so a failure late doesn't redo everything.
5. **Committing the recovered project will add several GB of LFS.** Warn the user before you push
   it; they may need a GitHub LFS data pack. Ask once, early, together with the license request.

---

## 4. Ranked hypotheses for the black screen — investigate in this order

Grounded in what is actually in the repo. Prove or disprove each with CI evidence; assume nothing.

**H1 — 32-bit/Mono out-of-memory (strongest suspect).**
The README states the previous APK targeted **ARMv7 (32-bit) + Mono**. A 32-bit process has ~3 GB
usable address space; the main scene is backed by ~1.1 GB of assets and texture memory blows
through that fast. The signature matches exactly: launches, audio thread runs so you hear
narration, allocation fails, black. **Fix: ARM64 + IL2CPP, ASTC compression, texture size caps,
mipmap streaming, compressed/streamed audio.** ARMv7 was simply the wrong target — every current
Quest is ARM64. **This is headlessly verifiable** via `BuildReport` per-asset sizes.

**H2 — Shaders/materials did not survive extraction.**
Extraction tools cannot recover *compiled* shaders; the repo's README half-admits this. Materials
pointing at missing shaders resolve to `Hidden/InternalErrorShader` or render black. **Headlessly
verifiable:** write an Editor script that walks every material and fails the build on any null or
error shader, then systematically remap to URP equivalents (`Universal Render Pipeline/Lit`,
`/Simple Lit`, `/Unlit`, `/Particles/Unlit`).

**H3 — URP asset not assigned for Android.** No pipeline asset in `GraphicsSettings` or in the
per-level `QualitySettings` for Android ⇒ black frame while audio and logic keep running.
Assert this in an Editor test.

**H4 — Stereo mode vs. non-instanced shaders.** Quest defaults to Multiview (single-pass
instanced); shaders that aren't instancing-aware render to nothing. If Multi-pass works and
Multiview is black, that confirms H2.

**H5 — Color space / graphics API.** Linear + GLES2 in the API list = black on Android. Vulkan
and/or GLES3 only; remove GLES2 entirely.

**H6 — Shader variant stripping** removing every variant the scene needs. Disable stripping while
debugging, use Always-Included Shaders, re-tighten at the end.

**H7 — Camera / XR rig wiring.** Missing `TrackedPoseDriver`, wrong clear flags, culling mask set
to Nothing, bad near/far clip, XR Origin mis-parented after reconstruction.

**H8 — Scene load failure.** `5SD Map` throws on load, leaving nothing to draw.

H1 and H2 can both be true. Fix both.

---

## 5. Method — phased, with gates. Report at every gate.

### Phase 0 — Bootstrap and unblock
- Clone the repo in your sandbox. Confirm `gh` works and you can read Actions.
- **Immediately produce the single consolidated ask to the user** (see §7): Unity license secrets,
  and a heads-up on LFS storage cost. Everything else you do yourself.
- While waiting, write and land the workflow scaffolding and the recovery job.

**Gate 0:** Secrets present; a trivial "hello Unity" Actions job activates the license and exits 0.

### Phase 1 — Recover the project (Job A, runs once)
- Check out with `lfs: true`, free disk, install .NET.
- **AssetRipper** (https://github.com/AssetRipper/AssetRipper) over the whole
  `RSAF_VRWarehouse_Data` folder; export as a Unity project.
- **`ilspycmd`** (`dotnet tool install -g ilspycmd`) to decompile `Managed/Assembly-CSharp.dll`
  to C#. All game logic lives here: training flow, scoring, quiz, leaderboard export, save system,
  and AutoHand. Expect to hand-repair decompiler artifacts — iterator/async state machines,
  `<>c__DisplayClass` closures, lowered switches.
- **Do not decompile redistributables.** Restore URP, Cinemachine, Input System, Post-processing,
  TextMeshPro, XR Management, Oculus XR, MockHMD via Package Manager at 2021.3.12f1-compatible
  versions. For NPOI, Newtonsoft.Json, NaughtyAttributes, SharpZipLib, copy the existing managed
  DLLs from `Managed/` into `Assets/Plugins/` — they are platform-agnostic and this avoids version
  drift. Add a `link.xml` so IL2CPP stripping doesn't gut them (NPOI and Newtonsoft use heavy
  reflection).
- Commit the recovered project with a correct Unity `.gitignore` — never commit `Library/`,
  `Temp/`, `obj/`, or build output.
- Note for the user in your report: AutoHand is a paid Asset Store product. Restoring it to repair
  their own app is reasonable, but recommend they hold a license; if they can supply a clean
  AutoHand package, prefer it over the decompiled copy.

**Gate 1:** Project committed. A CI job opens it in Unity 2021.3.12f1 with **zero compile errors**,
all six scenes present, build scene list in exactly the order in §2.

### Phase 2 — Fix rendering, proven headlessly (Job B)
This is where you kill the black screen. Build a real validation suite — it is stronger evidence
than a screenshot:
- **Material audit:** fail on any null shader or `Hidden/InternalErrorShader`. Remap to URP.
- **Pipeline assert:** URP asset assigned in Graphics settings and every Quality level, for Android.
- **Per-scene load test:** load each of the six scenes, assert zero exceptions/errors in the log.
- **Camera assert:** each scene has an enabled camera, sane clear flags, non-zero culling mask,
  sane clip planes; XR rig and `TrackedPoseDriver` wired.
- **Memory report:** dump `BuildReport` per-scene and per-asset sizes; quantify H1.
- **Best-effort screenshots:** run the Editor *without* `-nographics`, under `xvfb` with Mesa
  llvmpipe, and `ScreenCapture.CaptureScreenshot` each scene in playmode; upload as artifacts.
  Attempt MockHMD stereo the same way, testing both Multiview and Multi-pass per H4. If software
  GL proves unworkable, say so and lean on the assertions above — do not fake it.

**Gate 2:** All six scenes load clean; zero missing-shader materials; URP asserted; camera/XR
asserted; memory quantified. Screenshots attached if obtainable.

### Phase 3 — Android/Quest build (Job C)
- Player Settings: **IL2CPP**, **ARM64 only** (ARMv7 unchecked), Vulkan and/or GLES3 (no GLES2),
  Linear color space, Min API 29+, Target API 32+, `.NET Standard 2.1`.
- XR Plug-in Management → Android → **Oculus** enabled. Multiview first, Multi-pass as fallback.
- Manifest: `com.oculus.intent.category.VR` and
  `<uses-feature android:name="android.hardware.vr.headtracking" android:required="true"/>`.
- Texture compression **ASTC**; apply size caps and mipmap streaming per H1; compress/stream audio.
  Get `5SD Map` comfortably under ~2.5 GB and **document the number**.
- Build via `game-ci/unity-builder`, `targetPlatform: Android`.

**Gate 3:** APK builds green in CI.

### Phase 4 — Verify and ship (Job D)
- Static APK checks: `unzip -l`, `aapt dump badging`. Assert **`lib/arm64-v8a/libil2cpp.so` exists**
  and **`lib/armeabi-v7a/` does not**; assert the VR intent category and headtracking feature;
  assert sane APK size; assert all six scenes are in the build.
- Verify save/leaderboard I/O in an Editor test: `_MyProject/saves/AudioSettings.json` and
  `PlayerInfo.json` must round-trip in the original format, and NPOI must still emit
  `RSAF_Leaderboard.xls` / `RSAF_VIP_Leaderboard.xls`.
- **Attach the APK to a GitHub Release.** That is the user's deliverable — they download it
  straight to a phone or PC and sideload with SideQuest. They never touch their Mac for this work.

**Gate 4:** APK on a Release, statically verified, all evidence in CI artifacts.

### Phase 5 — Hand off
- Rewrite `README.md` to match reality: what was recovered and how, real build settings, how to
  rebuild the APK from CI, known visual differences.
- Write the on-device test script (§8, item 7).
- Open a PR with the full story.

---

## 6. Rules

- **Function over fidelity.** Every scene, interaction, quiz, tutorial step and leaderboard export
  must work. Visual differences from the original PC build are acceptable and expected — do not
  burn days chasing pixel parity, but document what differs.
- **Never modify the original build files.** `RSAF_VRWarehouse_Data/`, `MonoBleedingEdge/`, `*.exe`,
  `UnityPlayer.dll` are the only surviving copy of this game. Read-only, always. Add the recovered
  project alongside them, never in place of them.
- **Do not run `git reset --hard`, `git checkout .`, or `git clean` against the original build
  paths.** In a fresh clone the LFS content is what it is — leave it alone.
- **Never claim on-device success.** No headset exists in this loop. Always distinguish "asserted
  in a headless Editor test", "verified statically in the APK", and "requires a headset to confirm".
- Do not fake progress. If a scene won't render, say so with the CI log that proves it.
- Commit messages: `<type>: <description>` (feat/fix/refactor/docs/chore).
- Keep files focused and reasonably sized; many small files over few large ones.

---

## 7. The one thing you must ask the user for — do it in Phase 0, all at once

You cannot do these; they are browser-only and tied to their accounts.

1. **Unity license.** Run `game-ci/unity-request-activation-file` in Actions, download the `.alf`
   artifact, and give them exact instructions: upload it at `license.unity3d.com/manual`, choose
   Unity Personal, download the returned `.ulf`, and paste its full contents into the repo secret
   `UNITY_LICENSE`. Also `UNITY_EMAIL` and `UNITY_PASSWORD`. Give them click-by-click steps and
   the direct link — assume no prior GameCI knowledge.
2. **GitHub LFS storage.** Tell them how many GB the recovered project will add and that they may
   need a data pack (roughly $5/month per 50 GB) before you can push it. Get a yes before pushing.

Ask once, clearly, with exact steps. Then keep working on everything that isn't blocked.

## 8. Definition of done

1. The recovered Unity 2021.3.12f1 project committed to the repo, opening with zero compile errors
   in CI. **This is the most valuable output — the project must never be lost again.**
2. Headless validation suite green: six scenes load clean, no missing shaders, URP assigned,
   cameras/XR asserted. Screenshots attached if software rendering allowed it.
3. `RSAF_VRWarehouse.apk` built reproducibly in Actions, ARM64 + IL2CPP, statically verified,
   attached to a GitHub Release.
4. A documented memory budget for `5SD Map` showing H1 is addressed.
5. Save files and NPOI `.xls` leaderboard export verified working.
6. `README.md` rewritten to match reality, with CI rebuild instructions.
7. An on-device test script covering: app launches; menu renders and is interactable; audio
   settings persist; keyboard/name entry works; tutorial completes; **`5SD Map` loads and renders**
   (the actual bug); hands track and objects are grabbable; the scenario completes end to end;
   score writes to the leaderboard; the `.xls` export appears.
8. An honest statement of what remains unverified because no headset was in the loop.
