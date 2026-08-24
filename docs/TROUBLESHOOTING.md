# Troubleshooting

Problems are grouped by when they happen. Most have the same handful of causes.

---

## Setting up the headset

### The Developer Mode switch is missing, greyed out, or turns itself back off

Your Meta account is not a developer account yet. Follow
[INSTALL.md Part 1.1](../INSTALL.md#11-make-your-meta-account-a-developer-account) —
you must create an organisation *and* complete Meta's verification (two-factor
authentication or a credit card on the account).

Also confirm the phone app is signed in to **the same Meta account the headset uses**.
If a headset was set up by somebody else, their account controls this setting, not yours.

### The "Allow USB debugging" prompt never appears in the headset

It only appears while the headset is **awake and being worn**, and only after Developer
Mode is on *and the headset has been restarted*.

1. Confirm Developer Mode is on and the headset has been rebooted since.
2. Plug the cable in.
3. Put the headset on and wait 10 seconds.

If still nothing, unplug, restart the headset, and plug in again while wearing it.

---

## Installing

### "No headset became ready" / the installer waits forever

In order of how often it is the cause:

1. **The cable is charge-only.** Many USB-C cables carry power but no data, and give
   exactly this symptom. Try the cable the headset came with, or another known data cable.
   This is the most common cause by a wide margin.
2. **USB debugging was not approved.** Put the headset on and look for the prompt.
3. **Developer Mode is off**, or the headset was not restarted after switching it on.
4. **A USB hub or dock is in the way.** Plug directly into the computer.

To check what the computer can see, open a terminal or command prompt in the
`install/platform-tools` folder and run `adb devices`:

| Output | Meaning |
|---|---|
| nothing listed | Not connected — cable or port problem |
| `<serial>  unauthorized` | Connected, but you have not approved it in the headset |
| `<serial>  device` | Working correctly |

### "INSTALL_FAILED_INSUFFICIENT_STORAGE"

The headset is full. The app needs roughly 2 GB free. In the headset, go to
**Settings → Storage** and remove some apps or recordings.

### "INSTALL_FAILED_UPDATE_INCOMPATIBLE" or "VERSION_DOWNGRADE"

A copy of the app is already installed that was built on a different computer, so it
carries a different signature and cannot be updated in place.

**Collect any trainee results first — uninstalling deletes them.** Then remove the old
copy and install again:

```
adb uninstall com.sp.fyp3a25.rsafvrwarehouse
```

Or in the headset: **App Library → Unknown Sources → ⋯ → Uninstall**.

### The installer window closes instantly

It is reporting an error too fast to read. Open a terminal or command prompt, drag the
installer file into it, and press Enter — the window then stays open.

### Windows or macOS refuses to run the installer

Expected: the file is not signed by Microsoft or Apple.

- **Windows:** "Windows protected your PC" → **More info** → **Run anyway**.
- **Mac:** right-click the file → **Open** → **Open**. Do this once; afterwards
  double-clicking works.

### The download keeps failing or produces a tiny file

The installer checks the size and refuses anything under 100 MB, because a partial
download will install and then crash. Delete the partial `RSAF_VRWarehouse.apk`, and
either run the installer again or download the file manually from the
[releases page](../../releases/latest) and place it in the `install` folder.

Corporate and school networks sometimes block large downloads from GitHub. A home or
mobile connection usually solves it.

---

## Running the app

### The app is not in the headset's library

Sideloaded apps are hidden by default. In **App Library**, change the filter in the
**top-right corner** from *All* to **Unknown Sources**.

If it is genuinely absent, the install did not complete. Re-run the installer and read
the window rather than closing it.

### It opens as a flat floating window instead of surrounding me

It started in non-VR mode. Close it completely — **⋯ → Quit**, not just taking the
headset off — and relaunch from **Unknown Sources**.

If it happens every time, the headset may not be recognised as VR-capable by this build.
Report it along with the headset model.

### No sound at all

Work through these in order:

1. Headset volume is up, and it is not connected to a Bluetooth speaker or headphones.
2. In the app's own **Audio Settings** screen, check the Music, Ambience and
   Sound Effects sliders are not at zero. These are saved and persist between sessions,
   so a slider someone else moved stays moved.
3. Fully quit and relaunch the app.

### Sound effects play but there is no music or background atmosphere

The Music and Ambience sliders are separate from Sound Effects. Check all of them in the
app's Audio Settings screen.

### It runs slowly or stutters

This build has not been performance-tuned for the headset. Close other running apps
(**App Library → ⋯ → Quit** on each) and restart the headset before a session.

---

## Results

### The results script says no results were found

Results are only written when somebody **finishes a run** — either completing all
thirteen hazards or letting the timer expire. Quitting partway records nothing.

Results are also stored **per headset**. A run done on one headset does not appear on
another.

### A trainee's score is missing from the spreadsheet

The leaderboard keeps a ranked table, so a low score can be pushed out by higher ones.
Unfinished runs are recorded with a **DNF** timing.

### The results disappeared

Uninstalling the app deletes its stored data, including the spreadsheets. Always collect
results before uninstalling or replacing the app.

---

## Still stuck

Collect this before asking for help — it makes the difference between a guess and an answer:

1. Which headset model, and which computer operating system.
2. The exact text of the error, or a photo of the window.
3. The output of `adb devices`.
4. A log from the headset while reproducing the problem:

   ```
   adb logcat -s Unity:V "[Audio]":V *:E
   ```
