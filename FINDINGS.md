# Root-cause findings — RSAF VR Warehouse

Status legend: **CONFIRMED** = proven from recovered code/binaries. **SUSPECTED** = consistent
with evidence, not yet proven.

---

## F1 — CONFIRMED — All persistence writes to a read-only path on Android

**Severity: critical. This alone prevents the app from ever entering the main scene on Quest.**

The codebase has **14 uses of `Application.dataPath` and 0 uses of `Application.persistentDataPath`.**
On Windows, `Application.dataPath` is the writable `RSAF_VRWarehouse_Data/` folder — which is why
the original build shipped with `_MyProject/saves/*.json` and `Output/*.xls` inside it. On Android,
`Application.dataPath` resolves inside the APK and is **read-only**; every write throws.

The failure lands directly on the critical path:

```csharp
// KeyboardTypingArea.cs:318-328  (identical logic in TypingArea.cs:27-37)
if (!Directory.Exists(Application.dataPath + savePath))
    Directory.CreateDirectory(Application.dataPath + savePath);   // <-- THROWS on Android
string path = Application.dataPath + savePath + "/PlayerInfo.json";
File.WriteAllText(path, contents);                                // <-- THROWS on Android
SceneManager.LoadScene("5SD Map");                                // <-- NEVER REACHED
```

This matches the reported symptom exactly: the player enters a name, hears the audio, and the app
never transitions into the warehouse.

### All affected sites

| File | Line | Purpose |
|---|---|---|
| `KeyboardTypingArea.cs` | 318, 320, 322 | player name save, gates `LoadScene("5SD Map")` |
| `TypingArea.cs` | 27, 29, 31 | player name save, gates `LoadScene("5SD Map")` |
| `AudioSettings.cs` | 24, 26, 28 | volume settings persistence |
| `Leaderboard.cs` | 12, 103 | reads `PlayerInfo.json` |
| `retrieveData.cs` | 58 | NPOI `.xls` leaderboard export |
| `MyRuntimeTest.cs` | 42 | NPOI `.xls` leaderboard export |
| `LargeScreenShot.cs` | 32 | screenshot to `dataPath/../` |

`savePath` is `"/_MyProject/saves"` in all three declaring files.

### Fix
Introduce one small path helper and route every site through it. Do not scatter
`persistentDataPath` inline — a single source of truth keeps the Windows and Android behaviour
consistent and testable. Read paths must fall back to the shipped `dataPath` copy (for seeded
data) while all writes go to `persistentDataPath`.

---

## F2 — CONFIRMED — Decompiled sources target C# 12; Unity 2021.3 compiles C# 9

ILSpy 11 emitted primary constructors (`public struct PlayerActions(Myproject wrapper)`) and other
post-C#9 syntax. Unity 2021.3.12f1 cannot parse this. Recovered sources must be decompiled with
`-lv CSharp9_0`, with a CI guard that fails on too-new syntax.

---

## F3 — SUSPECTED — Previous APK was ARMv7 + Mono, a 32-bit address space

The repo README states the prior Quest build targeted ARMv7 with the Mono backend. The main scene
(`5SD Map`) is backed by ~1.1 GB of assets; a 32-bit process has ~3 GB usable address space.
Even with F1 fixed, this is likely to OOM. Target ARM64 + IL2CPP, ASTC compression, texture size
caps, and mipmap streaming. Quantify with a `BuildReport` before and after.

---

## F4 — SUSPECTED — Shaders will not survive asset extraction

Extraction tools cannot recover compiled shaders. Expect materials resolving to
`Hidden/InternalErrorShader` or rendering black. Must be audited and remapped to URP equivalents,
and asserted in an Editor test.

---

## Third-party dependencies recovered in `Assembly-CSharp.dll`

These were compiled into the game assembly rather than referenced as packages:

- **AutoHand** (`Autohand`, `Autohand.Demo`) — paid Asset Store VR interaction framework; the core
  hands/grab/locomotion layer.
- **Michsky.MUIP** — Modern UI Pack, paid Asset Store UI toolkit.
- **DentedPixel** — LeanTween tweening library.
- **UnityEngine.PostProcessing** — the *legacy* Post Processing Stack v1, compiled in, not the
  `com.unity.postprocessing` package. Note this when reconstructing the package manifest.

`Myproject.cs` is the auto-generated Input System actions wrapper, not a game manager.

---

# Validation evidence (headless, Unity 2021.3.12f1)

Produced by `RebuildValidation.ValidateAll` against the reconstructed project. This is
machine-checked evidence, not inspection by eye, and it reproduces with no GPU and no headset.

## F5 — CONFIRMED — No render pipeline asset assigned anywhere

```
- Graphics default pipeline: <none>            ** FAIL
- Quality[0] `Performant`:     <none>          ** FAIL
- Quality[1] `Balanced`:       <none>          ** FAIL
- Quality[2] `High Fidelity`:  <none>          ** FAIL
```

The project is URP (confirmed independently from `globalgamemanagers`, which references the full
`Hidden/Universal Render Pipeline/*` shader set). A URP project with no `UniversalRenderPipelineAsset`
assigned renders **nothing** — audio and game logic continue while the screen stays black.

Asset extraction cannot recover a pipeline asset, so this was always going to be missing in any
rebuild from the shipped player. **On its own this is sufficient to cause the reported symptom.**

## F4 — CONFIRMED — Extraction produced only placeholder shaders

All 51 recovered `.shader` files carry AssetRipper's `//DummyShaderTextExporter` marker: they are
non-functional stubs. Classification by declared shader name:

| Group | Count | Disposition |
|---|---|---|
| `Universal Render Pipeline/*`, `Hidden/Universal*`, `Hidden/kMotion/*`, `Hidden/Core/*` | 36 | Deleted — the URP package provides the real ones |
| `TextMeshPro/*` | 3 | Deleted — the TextMeshPro package provides the real ones |
| `Hidden/Post FX/*` (legacy PostProcessing Stack v1) | 14 | Kept — no installed package provides these; v1 does not work under URP and is likely dead code |
| `Outlined/Silhouette Only` | 1 | **The only genuinely custom shader.** Still a stub; needs a real URP implementation |

Materials bind shaders by GUID, so deleting the stubs orphaned those references — validation
then reported **298 of 343 materials** on `Hidden/InternalErrorShader`. Repaired by recovering the
`dummyGuid -> shaderName` map from a fresh extraction and rewriting each `.mat` to the real
package shader's GUID.

## F6 — CONFIRMED — XR was configured for Windows/Oculus only

`boot.config` sets `xrsdk-pre-init-library=OculusXRPlugin` for the Standalone target. XR
Plug-in Management stores loader configuration **per build target**, and the Android tab was never
configured — there is no Android XR loader setting to recover. An Android build with no XR loader
enabled never starts VR rendering, producing a black screen in-headset.

Additionally, **no Android bundle identifier exists anywhere in the shipped build**, so one must be
set explicitly.

## Recovered project settings worth preserving

- **Fixed Timestep = 1/72 s** — deliberately tuned to Quest's 72 Hz refresh. Must be preserved or
  physics behaviour and VR comfort will differ from the original.
- **Quality levels**: exactly three — `Performant`, `Balanced`, `High Fidelity`.
- **Tags** (5): `CO2`, `CorrectPPE`, `WrongPPE`, `Blood`, `Explode`.
- **Layers 8-11**: `HandPlayer`, `Hand`, `Grabbable`, `Grabbing` — AutoHand's convention. Physics
  and hand interaction break if these shift.
- **Company / Product**: `SP_FYP_3A25` / `RSAF_VRWarehouse`, version `0.1.0`.

## Reconstruction milestone

`Assembly-CSharp.dll` compiles clean in Unity 2021.3.12f1 at **727,552 bytes**, against the
original shipped assembly's **728,064 bytes** — a 512-byte delta across ~40,000 lines of recovered
C#, indicating a faithful recovery.
