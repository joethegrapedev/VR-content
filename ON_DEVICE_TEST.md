# On-device test script (Meta Quest)

Nothing in this rebuild has been verified on hardware. Every prior result is a headless Editor
assertion or a static file check. This is the script that closes that gap.

App id: `com.sp.fyp3a25.rsafvrwarehouse`
Writable data path on device: `/sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/`

---

## 0. Install

```bash
adb devices                      # headset must appear; approve "Allow USB debugging" in-headset
adb install -r RSAF_VRWarehouse.apk
```

If install fails with `INSTALL_FAILED_UPDATE_INCOMPATIBLE`, the old sideloaded build is still
present with a different signature:
```bash
adb uninstall com.sp.fyp3a25.rsafvrwarehouse && adb install RSAF_VRWarehouse.apk
```

Launch from **Library → Unknown Sources → RSAF_VRWarehouse**.

## 1. Capture logs while you test

Run this in a terminal for the whole session. Without it, a failure tells you nothing.

```bash
adb logcat -c                                     # clear first
adb logcat Unity:V CRASH:V AndroidRuntime:E DEBUG:V *:S | tee quest-test.log
```

Useful filters afterwards:
```bash
grep -iE "SavePaths|persistentDataPath|UnauthorizedAccess|IOException" quest-test.log
grep -iE "shader|InternalError|magenta"                                  quest-test.log
grep -iE "lowmemorykiller|Out of memory|Failed to allocate|OOM"          quest-test.log
grep -iE "XR|Oculus|loader|display subsystem"                            quest-test.log
```

---

## 2. The ordered test

Each step names what to look for **and what it proves**, since each maps to a specific fix.

### T1 — App starts and renders (proves F5: pipeline assigned, F6: Android XR loader)
- [ ] You see the Menu scene in 3D, not black, not a flat 2D panel
- [ ] Head tracking works — the view moves when you move
- [ ] Nothing is solid magenta or solid black

**If black here:** the XR loader or render pipeline did not take. Check
`grep -iE "XR|Oculus|loader" quest-test.log` for whether the Oculus subsystem started.

### T2 — Menu renders correctly (proves F4/F7: shaders and UI script refs)
- [ ] Menu text is readable and correctly shaped (proves the TextMeshPro remap)
- [ ] Buttons are visible, correctly coloured, and highlight on hover
- [ ] Geometry has proper lighting and materials — not flat white, not black

**Note:** two highlight materials (`Highlight.mat`, `Highlight 2.mat`) still use a stub
shader. Object highlight outlines may look wrong. That is a known, cosmetic gap.

### T3 — Audio Settings persist (proves F1: the persistentDataPath fix)
- [ ] Open Audio Settings, change the volume sliders
- [ ] Back out, fully quit the app, relaunch, reopen Audio Settings
- [ ] **The volume values you set are still there**

Verify on disk:
```bash
adb shell cat /sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/_MyProject/saves/AudioSettings.json
```
Expect `{"Volume":[...]}`. **If this file does not exist, F1 is not fixed** — that is the single
most important check in this document.

### T4 — Name entry and the scene transition (THE original bug)
- [ ] Reach the keyboard / name-entry screen
- [ ] Type a name of **at least 3 characters** (2 or fewer is rejected by design)
- [ ] Submit

- [ ] **`5SD Map` loads and renders.** This is the exact transition that used to go black.

Verify the save was written:
```bash
adb shell cat /sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/_MyProject/saves/PlayerInfo.json
```

**If it hangs or goes black here:** capture immediately —
```bash
grep -iE "IOException|UnauthorizedAccess|lowmemorykiller|Failed to allocate" quest-test.log
```
An IO error means F1 regressed. A memory kill means F3 (the scene is too heavy) and the
`build-report.md` asset sizes are the next thing to look at.

### T5 — The warehouse scene holds up (proves F3: memory)
- [ ] Scene renders fully — floor, shelving, lighting, no missing chunks
- [ ] Look around slowly through 360°; nothing pops in as black or magenta
- [ ] Stand still for 2 minutes — the app does not die

This is the 1.1 GB scene. If it is going to run out of memory, it happens here.

### T6 — Hands and interaction (AutoHand)
- [ ] Both hands are visible and track your controllers
- [ ] Fingers animate on grip/trigger
- [ ] You can **grab** an object, hold it, and release it
- [ ] Locomotion works (teleport and/or smooth, whichever the app uses)
- [ ] Objects collide with the world rather than passing through

Layers 8–11 (`HandPlayer`/`Hand`/`Grabbable`/`Grabbing`) drive this. If grabbing fails while
hands still render, layer assignments are the first suspect.

### T7 — The training scenario end to end
Recovered code indicates PPE selection, a CO2 fire extinguisher, hazards, and timed indicators.
- [ ] Complete the tutorial
- [ ] Select PPE; correct and incorrect choices are both recognised
- [ ] Use the fire extinguisher on a fire and the fire goes out
- [ ] Timed indicators (3 min / 10 min) fire when expected
- [ ] The scenario can be completed to its end state

### T8 — Scoring and leaderboard (proves F1 for NPOI export)
- [ ] Finish a run and reach the score/leaderboard screen
- [ ] Your name from T4 appears
- [ ] The score is plausible

Pull the generated spreadsheets off the device:
```bash
adb shell ls -la /sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/Output/
adb pull /sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/Output/ ./leaderboard-output/
```
Expect `RSAF_Leaderboard.xls` and `RSAF_VIP_Leaderboard.xls`, and they should open in Excel.
**If the folder is missing, the NPOI export path did not survive** — same F1 class of bug, and
NPOI is also the most likely thing to be broken by IL2CPP code stripping (it uses heavy
reflection). Look for `MissingMethodException` or `TypeInitializationException` in the log.

### T9 — Comfort
- [ ] Frame rate feels smooth, no persistent judder
- [ ] The 72 Hz fixed timestep was preserved, so physics should feel as originally authored

---

## 3. If something fails

Capture these three things and they will almost always identify the cause:

```bash
adb logcat -d > failure.log
adb shell dumpsys meminfo com.sp.fyp3a25.rsafvrwarehouse > meminfo.txt
adb shell ls -laR /sdcard/Android/data/com.sp.fyp3a25.rsafvrwarehouse/files/ > datafiles.txt
```

Rough triage:

| Symptom | Most likely cause |
|---|---|
| Black from launch | XR loader not active, or no pipeline asset |
| Renders but everything black/magenta | shader remap incomplete for that material set |
| Text missing or garbled | TextMeshPro essentials/shader binding |
| Black exactly at the `5SD Map` transition | F1 regressed, or out of memory |
| Dies after a while in the warehouse | memory — check `build-report.md` asset sizes |
| Hands render but cannot grab | layer assignments 8–11 |
| No `.xls` output | IL2CPP stripped NPOI — needs a `link.xml` entry |
