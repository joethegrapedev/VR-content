# Audio

## What was actually missing

The 67 original clips were **not** lost in the rebuild. All of them survived,
and every one is correctly referenced: an audit of all scenes and prefabs found
253 `AudioSource` components, 67 distinct clip GUIDs referenced, **0 dangling
references and 0 orphaned clips**.

What the game never had was atmosphere. The 67 clips break down as:

| Kind | Count |
| --- | --- |
| Task narration (`TaskNOptionM`) | 52 |
| Situational one-shots (fire, bleed, scream, grab, drop…) | 15 |
| Background music | **0** |
| Room ambience | **0** |
| Footsteps | **0** |
| UI feedback | **0** |

That silence is why the game felt static. Alongside it, four real defects kept
even the existing audio from working properly — see *Defects fixed* below.

## What was added

39 clips, generated procedurally by the scripts in `tools/audio/`. They are
original synthesis containing no third-party samples, so there is **no licence
or attribution obligation** — which matters for an FYP submission.

Every looping clip is verified seamless: the discontinuity at the wrap point is
measured against the typical sample-to-sample step, and all loops score ≤ 1.1
(a value near 1.0 is indistinguishable from any interior point).

### Music — `Assets/Resources/Audio/Music/`
Calm, sparse ambient in the vein of the quiet Minecraft soundtrack: soft mallet
tones over slow pads with wide reverb.

| Clip | Used by |
| --- | --- |
| `Music_Menu` | Menu, Keyboard, Audio Settings |
| `Music_Gameplay_Calm` | Tutorial, 5SD Map — mixed low so narration stays legible |
| `Music_Leaderboard` | Leaderboard |
| `Music_Tension` | available for hazard moments; not yet triggered |

### Ambience — `Assets/Resources/Audio/Ambience/`
`Amb_Warehouse_RoomTone` (stereo, 60 s) is the always-on bed: HVAC rumble,
distant plant noise, and occasional far-off clanks and forklift passes.
`Amb_Menu_RoomTone` is a quieter bed for the menus. `Amb_Air_Vent`,
`Amb_Fluorescent_Hum`, `Amb_Distant_Machinery` and `Amb_Conveyor` are mono
positional emitters placed on matching scene objects.

### Sound effects — `Assets/Resources/Audio/SFX/`
31 clips: 6 concrete and 3 metal footstep variants, 5 UI sounds, keyboard key,
task correct/wrong, game start/end, countdown tick, teleport, wood/metal/
cardboard impacts, water splash, extinguisher, forklift idle and beep, alarm,
and door open/close.

## How it is wired

A single `SceneAudioDirector` in each scene sets everything up at runtime.
This replaces hand-wiring hundreds of AudioSources, and means new or
dynamically spawned objects are covered automatically.

It performs, in order:

1. **`Awake`** — repairs every `ButtonVR` key (see below), before `ButtonVR.Start`
   caches its `AudioSource`.
2. **`Start`** — applies the player's saved volumes, starts the scene's music
   and ambience bed, then attaches footsteps, UI sounds, positional emitters
   and impact sounds according to the scene's profile.

Scene profiles live in `SceneAudioProfile.cs`. Menu, Keyboard and Audio
Settings deliberately share one track so the music runs unbroken while the
player names themselves.

Clips are loaded from `Resources` rather than scene references, which
guarantees they survive IL2CPP stripping. They are committed as lossless WAV
masters; Unity's importer compresses them to Vorbis for the Android player, so
nothing is encoded twice and no MP3 encoder padding can click at a loop point.

## Mixer

`Assets/Resources/Audio/AudioMixer.mixer` now has five groups:
**Master → Dialogue, SFX, Music, Ambience.**

The mixer was moved into `Resources` so it can be loaded at runtime; its GUID
is unchanged, so existing scene references still resolve.

## Defects fixed

1. **The volume sliders did nothing.** AssetRipper could not recover the exposed
   parameter names, leaving them as `pjtiwiL`, `UKVLQmH` and `WJRVUmN`, while
   `AudioSettings.cs` called `SetFloat("Dialogue", …)` and `SetFloat("SFX", …)`.
   Those calls silently returned false. The parameters are now correctly named.
2. **Settings were only applied in two scenes.** Tutorial, Keyboard and
   Leaderboard ignored the player's saved volumes entirely. `SceneAudioDirector`
   now applies them in every scene.
3. **Saving settings twice wrote stale values.** `SaveSettings()` appended to a
   shared list on each call, so the second save left the old values at the
   indices the loader reads. `AudioVolumeSettings` is immutable and always
   writes a fresh list.
4. **Half the keyboard was silent.** Of 132 `ButtonVR` keys in the Keyboard
   scene, 65 had an empty clip slot. `ButtonVrSoundBinder` fills them.
5. **`AudioStop` searched the wrong direction.** It used `GetComponentsInParent`,
   missing any `AudioSource` on a child of the panel, so narration could overlap.
   It now searches both directions.
6. **Pausing did not silence the game.** `Time.timeScale = 0` does not stop
   `AudioSource` playback; `PauseMenu` now also sets `AudioListener.pause`.

## Regenerating the audio

```bash
cd tools/audio
python -m venv .venv && ./.venv/bin/pip install numpy
./.venv/bin/python generate.py            # renders all 39 clips
./.venv/bin/python generate.py --only SFX_Door   # or just a subset
./.venv/bin/python verify.py ../../UnityProject/Assets/Resources/Audio/SFX
```

Generation is seeded and deterministic: the same seed always produces the same
clips. `verify.py` checks every clip for silence, clipping, DC offset, loop-seam
discontinuity and implausible spectral balance.

After regenerating, re-run the Unity importer configuration:

```
Unity -batchmode -quit -projectPath UnityProject -buildTarget Android \
      -executeMethod ConfigureAudio.Run
```

`ValidateAudio.Run` is the corresponding check; it fails the build if a mixer
group, exposed parameter, required clip or scene director is missing.
