# RSAF_VRWarehouse — Recovered Project Settings (Forensic Analysis)

**Source:** Compiled Unity `2021.3.12f1` Windows Standalone player (`RSAF_VRWarehouse.exe`), analyzed read-only.
**Method:** `strings -a`, `xxd`, and a hand-written Python parser for Unity's `SerializedFile` binary format (header/type-table/object-table), applied to `globalgamemanagers` (no embedded type tree — `m_EnableTypeTree = False`, so field names are not present; values below were recovered either as literal ASCII strings, or as raw bytes at object offsets I independently verified against known Unity default values).
**Confidence key:** ✅ = read directly from binary (high confidence) · 🟡 = inferred from circumstantial evidence (flagged) · ❌ = NOT RECOVERABLE.

Target platform recovered from the `SerializedFile` header: `m_TargetPlatform = 19` = **StandaloneWindows64** ✅ (confirms this is the Windows player, as stated).

---

## 1. Build Scene List ✅

Read directly from the `BuildSettings` object (classID 141, byte range `159064–159492` in `globalgamemanagers`). The array header confirms `sceneCount = 6`, immediately followed by the 6 scene paths in build order:

| # | Scene Path |
|---|---|
| 0 | `Assets/_MyProject/Scenes/Menu.unity` |
| 1 | `Assets/_MyProject/Scenes/Audio Settings.unity` |
| 2 | `Assets/_MyProject/Scenes/Leaderboard.unity` |
| 3 | `Assets/_MyProject/Scenes/Tutorial.unity` |
| 4 | `Assets/_MyProject/Scenes/Keyboard.unity` |
| 5 | `Assets/_MyProject/Scenes/5SD Map.unity` |

Matches the expected list exactly. Project root folder confirmed as `Assets/_MyProject/`.

---

## 2. Graphics Settings

- **Render pipeline asset type**: `UnityEngine.Rendering.Universal.UniversalRenderPipeline` ✅ — found as a literal string inside the `GraphicsSettings` object (classID 30, offset `157984`, size 796 bytes). This is the C# type of the object assigned to `GraphicsSettings.m_CustomRenderPipeline`, i.e. a **URP Pipeline Asset**. **URP is confirmed.** The specific asset's file name is not stored here (only its type), so it can't be recovered from this file.
- **Always Included Shaders**: recovered a clean list of **246 shader names** ✅ from a large object at classID 94 (offset `8544`, size 12273 bytes — this is almost certainly Unity's internal shader-name registry / GraphicsSettings-adjacent list, not `GraphicsSettings` itself, which is a separate, smaller object). Full list saved in analysis; all 246 are **standard Unity built-in, Legacy, Mobile, URP, TextMeshPro, and Post-Processing Stack v2 shaders** (e.g. `Universal Render Pipeline/Lit`, `Universal Render Pipeline/Simple Lit`, `Hidden/Universal Render Pipeline/...` post-effects, `TextMeshPro/Distance Field`, `Hidden/Post FX/...` from Unity.Postprocessing). **No custom/game-specific shader names appear in this list.**
- Custom shaders (if any exist in the project) are NOT recoverable from the in-scope files — they would live inside `sharedassets*`/`level*` files as `Shader` objects, which were explicitly out of scope for this analysis. Treat "no custom shaders found" as a scope limitation, not proof none exist.

---

## 3. Quality Settings

Object: classID 47 (`QualitySettings`), offset `159496`, size 456 bytes.

- **Level count**: 3 ✅ (array count read directly).
- **Level names, in order** ✅: `Performant` (index 0), `Balanced` (index 1), `High Fidelity` (index 2). The stock Unity default level names (Very Low/Low/Medium/High/Very High/Ultra) were fully replaced — none of them appear anywhere in the file.
- **Active/default quality level**: 🟡 A leading `int32 = 2` immediately precedes the level-count field, consistent with `m_CurrentQuality = 2` (i.e. "High Fidelity" active by default) — plausible but not verified against a type tree, treat as medium-confidence.
- **Per-level fields (shadows, shadow resolution/distance, texture quality/mipmap limit, anti-aliasing, vSync count, LOD bias, streaming mipmaps, etc.)**: ❌ NOT RECOVERABLE. These fields exist in the same 456-byte object but their exact byte offsets require Unity's internal `QualitySettings` struct layout for 2021.3, which isn't derivable without a type tree. Attempting to guess field order risks reporting wrong values — deliberately not attempted.
- **Render pipeline asset per level**: ❌ NOT RECOVERABLE (same reason).

---

## 4. Player Settings

| Field | Value | Confidence |
|---|---|---|
| Company Name | `SP_FYP_3A25` | ✅ (from `app.info` line 1, corroborated by same string in `globalgamemanagers` near start of the `PlayerSettings` object) |
| Product Name | `RSAF_VRWarehouse` | ✅ (from `app.info` line 2, matches .exe name) |
| Bundle/App Identifier (Android/Quest) | — | ❌ NOT RECOVERABLE — no `com.company.product`-style string appears anywhere in `globalgamemanagers`/`.assets`. This is a Windows Standalone build; a mobile bundle identifier may never have been set, or is simply not embedded in a Windows player. **This must be set manually when configuring the Quest build.** |
| Bundle Version | `0.1.0` | ✅ (literal string, positioned directly after company/product/category strings in `PlayerSettings`) |
| App Category string | `public.app-category.games` | ✅ (present, though this is an Apple-platform metadata field that Unity embeds regardless of target platform) |
| Color Space | 🟡 Likely **Linear** | Not directly recoverable (no type tree). Inferred only from URP project convention (URP templates default to Linear) — **not confirmed**, verify manually. |
| Graphics APIs (Windows) | ❌ NOT RECOVERABLE | Stored as an integer enum array with no literal strings; cannot be read via strings-only analysis. |
| Scripting Backend | **Mono (Mono2x)** | ✅ HIGH CONFIDENCE — the shipped player includes a full `MonoBleedingEdge/` runtime folder and there is **no `GameAssembly.dll`** anywhere in the build root. Both are definitive signatures of the Mono scripting backend, not IL2CPP. |
| API Compatibility Level | 🟡 Ambiguous | Both `mscorlib.dll` and `netstandard.dll` are present in `Managed/`, which is consistent with either `.NET Standard 2.0` or `.NET Framework` compatibility level under Mono in 2021.3 — cannot distinguish from the file listing alone. NOT RECOVERABLE with certainty. |
| Target Architecture | Windows x86_64 (implied) | 🟡 Only `RSAF_VRWarehouse.exe` + `UnityPlayer.dll` (no separate 32-bit binaries) are present; this is a Standalone Windows 64-bit player per the `SerializedFile` header (`TargetPlatform=19`). Explicit "Architecture" PlayerSettings field is not separately recoverable. |
| Graphics Jobs | **Enabled** (`gfx-enable-gfx-jobs=1`) | ✅ from `boot.config` |
| Native Graphics Jobs | **Enabled** (`gfx-enable-native-gfx-jobs=1`) | ✅ from `boot.config` |
| HDR Display Output | **Disabled** (`hdr-display-enabled=0`) | ✅ from `boot.config` |
| XR SDK Pre-Init Library | `OculusXRPlugin` | ✅ from `boot.config` (`xrsdk-pre-init-library=OculusXRPlugin`) |

---

## 5. XR Settings

- **XR loaders/subsystems present** ✅ (confirmed by `UnitySubsystems/*/UnitySubsystemsManifest.json` + matching `Managed/` DLLs):
  - `OculusXRPlugin` (`Unity.XR.Oculus.dll`) — display id `"oculus display"`, input id `"oculus input"`, `disablesLegacyVr: true`.
  - `UnityOpenXR` (`Unity.XR.OpenXR.dll` + `Unity.XR.OpenXR.Features.*.dll`, including `OculusQuestSupport`, `ConformanceAutomation`, `RuntimeDebugger`, `MockRuntime`) — version `1.5.3`.
  - `UnityMockHMD` (`Unity.XR.MockHMD.dll`) — for in-editor/no-headset testing.
  - Supporting packages: `Unity.XR.Management.dll` (XR Plugin Management), `Unity.XR.CoreUtils.dll`, `Unity.XR.Interaction.Toolkit.dll`, `Unity.Subsystem.Registration.dll`, `UnityEngine.SpatialTracking.dll`, `UnityEngine.XR.LegacyInputHelpers.dll`.
- **Active/pre-initialized loader** ✅: `boot.config` explicitly sets `xrsdk-pre-init-library=OculusXRPlugin`, and `RuntimeInitializeOnLoads.json` includes `Unity.XR.Oculus.OculusLoader.RuntimeLoadOVRPlugin` plus `UnityEngine.XR.Management.XRGeneralSettings.AttemptInitializeXRSDKOnLoad` / `AttemptStartXRSDKOnBeforeSplashScreen` and URP's `UnityEngine.Rendering.Universal.XRSystem.XRSystemInit`. Together these confirm **XR Plugin Management "Initialize XR on Startup" is enabled, with Oculus as the active loader for the Standalone target**, and OpenXR/MockHMD available but not the pre-init default.
- **Stereo rendering mode** ❌ NOT RECOVERABLE — no literal string evidence found (it's an integer enum with no type tree). Do not assume Single Pass Instanced — verify manually.
- Note: this is a **Windows Standalone** build referencing Oculus/OpenXR desktop VR runtimes. When rebuilding for **Quest/Android**, the loader configuration (`Android` platform XR Plug-in Management settings — separate from what's shown here for `Standalone`) is a **separate, unrecovered configuration set** and must be configured fresh; nothing in these files tells us what (if anything) was configured for the Android platform tab.

---

## 6. Tags and Layers ✅ (fully recovered, byte-verified)

Recovered from the `TagManager` object (classID 78, offset `8192`, size 304 bytes) — decoded field-by-field from raw bytes (array counts + length-prefixed strings), not just `strings` output, so this is high confidence.

**Tags (5 custom tags, plus Unity's hardcoded defaults which are never stored as strings — Untagged, Respawn, Finish, EditorOnly, MainCamera, Player, GameController):**

1. `CO2`
2. `CorrectPPE`
3. `WrongPPE`
4. `Blood`
5. `Explode`

**Layers (32 total; blank = unused/empty slot):**

| # | Name | | # | Name |
|---|---|---|---|---|
| 0 | Default | | 16 | *(empty)* |
| 1 | TransparentFX | | 17 | *(empty)* |
| 2 | Ignore Raycast | | 18 | *(empty)* |
| 3 | *(empty)* | | 19 | *(empty)* |
| 4 | Water | | 20 | *(empty)* |
| 5 | UI | | 21 | *(empty)* |
| 6 | *(empty)* | | 22 | *(empty)* |
| 7 | *(empty)* | | 23 | *(empty)* |
| 8 | **HandPlayer** | | 24 | *(empty)* |
| 9 | **Hand** | | 25 | *(empty)* |
| 10 | **Grabbable** | | 26 | *(empty)* |
| 11 | **Grabbing** | | 27 | *(empty)* |
| 12–15 | *(empty)* | | 28–31 | *(empty)* |

Layers 8–11 (HandPlayer/Hand/Grabbable/Grabbing) are the standard **AutoHand** VR-interaction-asset layer convention, confirming AutoHand is in use (also confirmed independently by `Assembly-CSharp.dll` containing classes `AutoHandPlayer`, `Grabbable`, `AutoHandSettings`, `Finger`, `GrabLock`, `AutoHandExtensions`, etc.).

**Sorting Layers**: 1 total — `Default` only (unmodified, no additional 2D sorting layers configured). ✅

---

## 7. Physics Settings

Object: `PhysicsManager` (classID 55), offset `158800`, size 264 bytes. Decoded directly as raw floats/ints and cross-checked against known Unity defaults.

- **Gravity**: `(0, -9.81, 0)` ✅ — read as the first three float32 fields; exact match to Unity's default gravity vector. **Not customized.**
- **Default Material**: `None` ✅ (PPtr fileID=0, pathID=0 — no default `PhysicMaterial` assigned).
- **Bounce Threshold**: `2.0` 🟡 — matches Unity's default value exactly (medium-high confidence given exact default match, but field position not verified via type tree).
- **Sleep Threshold**: `0.005` 🟡 — matches Unity's default value exactly (same caveat).
- **Layer Collision Matrix**: for all 12 *named* layers (0–11, i.e. every layer actually in use in this project — Default, TransparentFX, Ignore Raycast, Water, UI, HandPlayer, Hand, Grabbable, Grabbing), the raw matrix bytes are **`0xFFFFFFFF`** (all bits set) for every row ✅. This means **no layer-collision exclusions were configured** among the layers this project actually uses — every used layer collides with every other used layer, i.e. the collision matrix is the Unity default (untouched) for all populated layers. (Bytes for the unused layer indices 28–31 show a non-uniform bit-truncation pattern consistent with Unity's internal upper-triangular matrix storage optimization for unused/high-index layers — not meaningful since no layers are defined there, and not reported as specific values to avoid a misleading result.)
- **Solver iterations, contact offset, max angular speed, world bounds, etc.**: ❌ NOT RECOVERABLE precisely — present in the same byte range but exact field order for Unity 2021.3's `PhysicsManager` struct could not be confirmed without a type tree; guessing risked wrong values, so these are intentionally omitted.

---

## 8. Input

- **Legacy InputManager**: ✅ Fully configured and present (classID 13, offset `5608`, size 2580 bytes). All axis names recovered by reading the object's string table directly:

  | Axis | Type / Notes |
  |---|---|
  | Horizontal | Keyboard (left/right) + duplicate entry for Joystick |
  | Vertical | Keyboard (down/up) + duplicate entry for Joystick |
  | Fire1 | left ctrl / mouse 0 + duplicate entry: joystick button 0 |
  | Fire2 | left alt / mouse 1 + duplicate: joystick button 1 |
  | Fire3 | left shift / mouse 2 + duplicate: joystick button 2 |
  | Jump | space + duplicate: joystick button 3 |
  | Mouse X / Mouse Y / Mouse ScrollWheel | mouse delta axes |
  | Submit | return / joystick button 0 (two entries) |
  | Cancel | escape / joystick button 1 |
  | Enable Debug Button 1 | left ctrl / joystick button 8 |
  | Enable Debug Button 2 | backspace / joystick button 9 |
  | Debug Reset | left alt / joystick button 1 |
  | Debug Next | page down / joystick button 5 |
  | Debug Previous | page up / joystick button 4 |
  | Debug Validate | return / joystick button 0 |
  | Debug Persistent | right shift / joystick button 2 |
  | Debug Multiplier | left shift / joystick button 3 |
  | Debug Horizontal | left/right (x2 entries) |
  | Debug Vertical | down/up (x2 entries) |

  The first 9 axes (Horizontal…Mouse ScrollWheel) plus Submit/Cancel are Unity's **unmodified stock default axes**. The `Debug *` and `Enable Debug Button *` axes are **custom, developer-added** — almost certainly wired to an in-app debug console/menu.

- **New Input System**: `Unity.InputSystem.dll` is present in `Managed/`, and `RuntimeInitializeOnLoads.json` includes `Unity.InputSystem.InputSystem.RunInitializeInPlayer` / `RunInitialUpdate` — confirms the **Input System package is installed and actively initialized** at runtime.
- **Active Input Handling** (Legacy / Input System Package / Both): ❌ NOT RECOVERABLE with certainty from these files. Both systems are clearly present and initialized; this is consistent with (but doesn't prove) the "Both" setting — treat as unconfirmed.

---

## 9. Audio Settings ✅ (fully recovered, byte-verified — all default values, unmodified)

Object: `AudioManager` (classID 11), offset `8496`, size 48 bytes. Decoded field-by-field:

| Field | Value |
|---|---|
| Volume | 1.0 |
| Rolloff Scale | 1.0 |
| Doppler Factor | 1.0 |
| Default Speaker Mode | 2 (Stereo) |
| Sample Rate | 0 (= "use system/platform default", not overridden) |
| DSP Buffer Size | 1024 |
| Virtual Voice Count | 512 |
| Real Voice Count | 32 |
| Spatializer Plugin | *(none set)* |
| Ambisonic Decoder Plugin | *(none set)* |
| Disable Audio | false |
| Virtualize Effects | true |

Every one of these is Unity's out-of-the-box default — **the Audio Manager was never customized** by the developers.

---

## 10. Time Settings (bonus — decoded, since it's directly relevant to VR feel)

Object: `TimeManager` (classID 5), offset `158784`, size 16 bytes:

| Field | Value | Note |
|---|---|---|
| **Fixed Timestep** | **0.013889 s (≈ 1/72 s)** | ✅ **Customized away from Unity's default (0.02 s / 50 Hz).** 1/72 s corresponds almost exactly to a **72 Hz** physics tick rate — the native refresh rate of the Meta/Oculus Quest and Quest 2 in default mode. This was very likely deliberately tuned for VR physics smoothness on Quest-class headsets. |
| Maximum Allowed Timestep | 0.333333 s | Unity default, unmodified |
| Time Scale | 1.0 | Unity default, unmodified |
| Maximum Particle Timestep | 0.03 s | Unity default, unmodified |

---

## 11. Package / Assembly Inventory

From `ScriptingAssemblies.json` (the authoritative list of assemblies actually loaded at runtime) cross-referenced against `RSAF_VRWarehouse_Data/Managed/`.

### Unity packages (com.unity.*)

| Assembly | Package |
|---|---|
| `Unity.RenderPipelines.Core.Runtime.dll`, `Unity.RenderPipelines.Core.ShaderLibrary.dll` | `com.unity.render-pipelines.core` |
| `Unity.RenderPipelines.Universal.Runtime.dll`, `Unity.RenderPipelines.Universal.Shaders.dll`, `Unity.RenderPipeline.Universal.ShaderLibrary.dll` | `com.unity.render-pipelines.universal` |
| `Unity.RenderPipelines.ShaderGraph.ShaderGraphLibrary.dll` | `com.unity.shadergraph` (URP dependency) |
| `Unity.InputSystem.dll` | `com.unity.inputsystem` |
| `Unity.Mathematics.dll` | `com.unity.mathematics` |
| `Unity.Burst.dll` + `Unity.Burst.Cecil*.dll`, `Unity.Burst.Unsafe.dll` | `com.unity.burst` |
| `Unity.TextMeshPro.dll` | `com.unity.textmeshpro` |
| `Unity.Timeline.dll` | `com.unity.timeline` |
| `Unity.Postprocessing.Runtime.dll` | `com.unity.postprocessing` (legacy PPv2) |
| `Cinemachine.dll` | `com.unity.cinemachine` |
| `UnityEngine.UI.dll` | `com.unity.ugui` |
| `Unity.VisualScripting.Core/Flow/State.dll` + `Antlr3.Runtime.dll` | `com.unity.visualscripting` |
| `Unity.XR.CoreUtils.dll` | `com.unity.xr.core-utils` |
| `Unity.XR.Interaction.Toolkit.dll` | `com.unity.xr.interaction.toolkit` |
| `Unity.XR.Management.dll` | `com.unity.xr.management` |
| `Unity.XR.MockHMD.dll` | `com.unity.xr.mock-hmd` |
| `Unity.XR.Oculus.dll` | `com.unity.xr.oculus` |
| `Unity.XR.OpenXR.dll` + `Unity.XR.OpenXR.Features.*.dll` | `com.unity.xr.openxr` |
| `Unity.Subsystem.Registration.dll` | `com.unity.subsystemregistration` (XR dependency) |
| `UnityEngine.SpatialTracking.dll`, `UnityEngine.XR.LegacyInputHelpers.dll` | `com.unity.xr.legacyinputhelpers` |
| `Unity.Services.Core*.dll` (Analytics/Configuration/Device/Environments/Internal/Networking/Registration/Scheduler/Telemetry/Threading) | `com.unity.services.core` |
| `Accessibility.dll` | 🟡 possibly `com.unity.accessibility` (preview) or a Mono BCL accessibility bridge — not listed in `ScriptingAssemblies.json`'s runtime-used names, so likely unused/vestigial; low confidence on exact origin |

### Third-party plain DLLs (not Unity packages)

| Assembly | Purpose |
|---|---|
| `NPOI.dll`, `NPOI.OOXML.dll`, `NPOI.OpenXml4Net.dll`, `NPOI.OpenXmlFormats.dll` | NPOI — reading/writing Excel `.xlsx`/Office Open XML files (likely used for importing warehouse/leaderboard data) |
| `ICSharpCode.SharpZipLib.dll` | SharpZipLib — zip/compression support (NPOI's OOXML reader depends on this) |
| `Newtonsoft.Json.dll` | Json.NET |
| `NaughtyAttributes.Core.dll` | NaughtyAttributes — custom Unity Inspector attributes asset |

### Source-only assets (compiled directly into `Assembly-CSharp.dll`, no separate DLL)

- **AutoHand** — confirmed via `Autohand`/`Autohand.Demo` namespaces and classes (`AutoHandPlayer`, `Grabbable`, `AutoHandSettings`, `Finger`, `GrabLock`, `DistanceGrabbable`, `GrabbablePose`, etc.) and the AutoHand-specific layers (HandPlayer/Hand/Grabbable/Grabbing) and preloaded assets (`autohandembelm`, `autohandlogo`, `autohandsettings`, `customjoint`, `defaultjoint`, `friction`/`nofriction`/`slightfriction`). Also custom XR-integration wrapper scripts were found: `OpenXRAutoHandFingerBender`, `OpenXRHandPlayerControllerLink`, `OpenXRTeleporterLink` — indicating the developers wrote custom glue code linking AutoHand to the OpenXR/Oculus input path.
- **Modern UI Pack (MUIP)** — confirmed via classes `CustomDropdown`, `ButtonManager`, `ContextMenuManager`, `AnimatedIconHandler`, and the preloaded asset `muip manager`.
- Game-specific gameplay scripts (non-package), e.g. `BowlingManager`, `BoxingGlove`, `DoorAnimator`, `ExtinguishableFire`, `CollisionSound`, `DetectPlayer`, etc. — all custom to this project.

### Mono/.NET BCL companions (ship automatically with the Mono scripting backend, not project dependencies)

`Mono.Posix.dll`, `Mono.Security.dll`, `Mono.WebBrowser.dll`, `System.Core.dll`, `System.Data*.dll`, `System.Drawing.dll`, `System.Xml*.dll`, `System.Windows.Forms.dll`, `System.Runtime*.dll`, `System.Security.dll`, `mscorlib.dll`, `netstandard.dll`, etc.

---

## Summary of biggest risks for a rebuilt Quest/Android target

See the concise summary returned alongside this report.
