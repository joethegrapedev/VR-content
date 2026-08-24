# Installing RSAF VR Warehouse Safety on a Meta Quest

This guide assumes no technical background. Follow it in order.

**Time needed:** about 30 minutes for the first headset (most of it waiting for a
download), then about 5 minutes for each additional headset.

**Why is this not just "download from the store"?** This is an internal training app,
not a public product, so it is not on the Meta store. Putting an app on a headset
yourself is called *sideloading*, and Meta requires you to unlock the headset first.
That unlocking is the fiddly part; the rest is a double-click.

---

## What you need before you start

- A **Meta Quest 2, 3, 3S or Pro** headset
- The **Meta Horizon** app on your phone, signed in to the same account as the headset
  (this is the app you used to set the headset up; it used to be called "Oculus")
- A **Windows or Mac computer**
- A **USB-C cable that carries data**. This matters more than people expect — many
  cables that came with phones or battery packs only carry power, and they will never
  work no matter what else you do. The cable that came with the headset is fine.
- About **2 GB free space** on the headset and **2 GB** on the computer

---

## Part 1 — Unlock the headset (once per headset)

You only ever do this once for each headset.

### 1.1 Make your Meta account a developer account

Meta will not let a normal account unlock a headset.

1. On a computer, go to <https://developer.oculus.com/manage/organizations/create/>
2. Sign in with the **same Meta account** that the headset uses.
3. Create an organisation. The name is unimportant — your team or school name is fine.
   This is free and does not mean you are publishing anything.
4. If prompted, complete the verification step. Meta asks for either **two-factor
   authentication** or a **credit card** on the account. Two-factor is the easier route
   and costs nothing.

> If you skip this, the Developer Mode switch in Part 1.2 will either be missing or
> refuse to stay on. It is the single most common thing that goes wrong.

### 1.2 Turn on Developer Mode

1. Put the headset on and make sure it is switched on and connected to Wi-Fi.
2. Open the **Meta Horizon** app on your phone.
3. Find your headset in the app — usually under a **Devices** or **Menu → Devices**
   section — and tap it.
4. Tap **Headset settings**, then **Developer Mode**.
5. Turn **Developer Mode on**.
6. Turn the headset fully off and on again. The setting does not take effect until it
   restarts.

> Meta reorganises this app regularly, so the exact wording may differ. You are looking
> for your headset's settings, then a "Developer Mode" switch.

### 1.3 Approve the computer

1. Plug the headset into the computer with the USB-C cable.
2. **Put the headset on.** A prompt appears *inside* the headset asking whether to allow
   USB debugging.
3. Tick **Always allow from this computer**, then choose **Allow**.

If you never see the prompt, take the headset off and on again while it is plugged in —
it only appears when the headset is awake and being worn.

---

## Part 2 — Get the installer

Go to the **[latest release](../../releases/latest)** and download the one file for your
computer:

| Computer | Download |
|---|---|
| Windows | `install-on-quest-windows.bat` |
| Mac | `Install-on-Quest-Mac.zip` — then unzip it |

That is all you need. The installer fetches the app itself, so **do not** download the
1 GB `.apk` by hand.

> You do not need a GitHub account, and you do not need the rest of the repository.

If you would rather have everything at once, the green **Code → Download ZIP** button on
the project page gives you the installers plus all the documentation.

---

## Part 3 — Run the installer

Open the `install` folder and double-click the file for your computer:

| Computer | File to double-click |
|---|---|
| Windows | `install-on-quest-windows.bat` |
| Mac | `install-on-quest-mac.command` |

A black window opens and reports what it is doing. It will:

1. Download a small set of Android tools from Google (about 10 MB, first time only).
2. Download the app itself (about **1 GB** — this is the slow part, often 10–20 minutes).
3. Wait for the headset, then install the app onto it.

**Leave the window open and the cable plugged in until it says it has finished.**
Installing a file this large onto a headset takes several minutes on its own, and the
window will look frozen while it happens. That is normal.

### If your computer blocks the file

This is expected — the file was downloaded from the internet and is not signed by
Apple or Microsoft.

- **Windows:** a blue "Windows protected your PC" box appears. Click **More info**, then
  **Run anyway**.
- **Mac:** you may see "cannot be opened because it is from an unidentified developer".
  Right-click the file, choose **Open**, then click **Open** in the box that appears.
  You only have to do this the first time.

---

## Part 4 — Open the app in the headset

Sideloaded apps do not appear alongside normal apps, which catches everybody out.

1. Put the headset on. You can unplug the cable now.
2. Open the **App Library** (the grid icon on the toolbar).
3. Find the **filter dropdown in the top-right corner** — it usually says *All*.
4. Change it to **Unknown Sources**.
5. **RSAF_VRWarehouse** is in that list. Select it to start.

> If the list is empty, the install did not actually finish. Go back to Part 3 and watch
> the window for an error message.

---

## Part 5 — Check it works

Run through this once on the first headset:

- [ ] The app opens and you can see the menu in 3D, not a flat floating window
- [ ] Background music is playing on the menu
- [ ] You can type a name using the in-headset keyboard and continue
- [ ] The warehouse scene loads and you can look and move around
- [ ] You can hear footsteps as you move, and room ambience in the background

If the app opens as a flat panel instead of surrounding you, it launched in non-VR mode.
Close it fully and start it again from **Unknown Sources**.

---

## Doing the rest of the headsets

Each additional headset needs:

- **Part 1.2 and 1.3** (Developer Mode and approving the computer) — per headset
- **Part 3** (double-click the installer) — but it is much faster, because the app has
  already been downloaded and is reused

Part 1.1 and Part 2 are done once in total, not once per headset.

**Tip for managing several headsets:** open `install/headset-card.html` in a browser and
print it at 100% scale. You get eight small cards carrying a QR code to these
instructions — attach one to each headset or its case, so anyone who picks it up later
can find the app without hunting for this document.

---

## Collecting trainee results

The app saves every completed run to a spreadsheet **on the headset itself**. To get
those onto a computer, plug the headset in and double-click:

| Computer | File to double-click |
|---|---|
| Windows | `install/get-results-windows.bat` |
| Mac | `install/get-results-mac.command` |

The spreadsheets are copied to your Desktop into a dated folder, and open with Excel,
Numbers or Google Sheets.

**Results are stored per-headset.** A trainee's score lives only on the headset they
used, so collect from each one. Uninstalling the app deletes its results, so collect
before removing or updating it.

---

## Updating to a newer version later

Run the installer again with the newer file. If it refuses with a message about an
incompatible update, the old copy has to go first:

1. Collect the results (above) — **uninstalling deletes them**.
2. In the headset: **App Library → Unknown Sources**, then the **⋯** menu next to
   RSAF_VRWarehouse → **Uninstall**.
3. Run the installer again.

---

## If something goes wrong

See **[docs/TROUBLESHOOTING.md](docs/TROUBLESHOOTING.md)**, which lists the specific
error messages and what each one means.
