"""One-shot and looping sound effects.

All clips are mono so Unity can spatialise them as 3-D sources. UI sounds
are deliberately dry; physical sounds carry a little warehouse reverb.
"""
from __future__ import annotations

import numpy as np

import dsp


def _tail(signal: np.ndarray, rng: np.random.Generator, room: float = 1.8,
          wet: float = 0.22) -> np.ndarray:
    """Place a dry one-shot in the room without washing it out."""
    return dsp.normalise(dsp.reverb(signal, room, wet, rng, damping=3200.0))


# ----------------------------------------------------------------- footsteps

def footstep_concrete(rng: np.random.Generator) -> np.ndarray:
    """Boot on concrete: a low thud, a mid slap, and a little grit."""
    length = 0.34
    thud = dsp.lowpass(dsp.noise(length, rng), 260.0)
    thud *= dsp.percussive(length, 0.030, attack=0.001)

    slap = dsp.bandpass(dsp.noise(length, rng), 700.0, 3600.0)
    slap *= dsp.percussive(length, 0.014, attack=0.0008)

    grit = dsp.bandpass(dsp.noise(length, rng), 3000.0, 7000.0)
    grit *= dsp.percussive(length, 0.030, attack=0.004) * 0.06

    body = dsp.resonant(thud, rng.uniform(95, 135), q=5.0)
    mixed = dsp.lowpass(
        body * 1.6 + slap * rng.uniform(0.45, 0.70) + grit, 9000.0, order=2)
    return dsp.match_rms(_tail(dsp.fade(mixed, 0.0005, 0.05), rng, 1.4, 0.16), -22.0)


def footstep_metal(rng: np.random.Generator) -> np.ndarray:
    """Step on steel grating or a loading ramp."""
    length = 0.55
    strike = dsp.percussive(length, 0.020, attack=0.0006) * dsp.noise(length, rng)
    ring = sum(
        dsp.sine(freq, length) * gain * dsp.exp_decay(length, tau)
        for freq, gain, tau in ((520.0, 0.5, 0.10), (1180.0, 0.35, 0.07),
                                (2260.0, 0.20, 0.05))
    )
    mixed = dsp.bandpass(strike, 300.0, 8000.0) * 0.8 + ring * 0.5
    return dsp.match_rms(_tail(dsp.fade(mixed, 0.0005, 0.08), rng, 2.0, 0.28), -22.0)


# ------------------------------------------------------------------------ UI

def ui_hover() -> np.ndarray:
    length = 0.10
    tone = dsp.sine(1760.0, length) * 0.6 + dsp.sine(2640.0, length) * 0.25
    return dsp.match_rms(dsp.fade(tone * dsp.percussive(length, 0.022), 0.002, 0.04), -30.0)


def ui_click() -> np.ndarray:
    length = 0.14
    tone = dsp.sine(880.0, length) * 0.7 + dsp.sine(1320.0, length) * 0.35
    click = dsp.sine(2640.0, length) * dsp.percussive(length, 0.006) * 0.4
    return dsp.match_rms(
        dsp.fade(tone * dsp.percussive(length, 0.030) + click, 0.001, 0.05), -24.0)


def ui_confirm() -> np.ndarray:
    """Rising two-note affirmative."""
    length = 0.34
    out = np.zeros(dsp.n_samples(length))
    for freq, offset in ((880.0, 0.0), (1318.5, 0.09)):
        note = dsp.sine(freq, 0.24) * 0.7 + dsp.sine(freq * 2, 0.24) * 0.2
        note = note * dsp.percussive(0.24, 0.075)
        start = int(offset * dsp.SAMPLE_RATE)
        out = dsp.mix_at(out, note, start)
    return dsp.match_rms(dsp.fade(out, 0.002, 0.06), -22.0)


def ui_back() -> np.ndarray:
    """Falling two-note negative-space cue for cancel/back."""
    length = 0.30
    out = np.zeros(dsp.n_samples(length))
    for freq, offset in ((880.0, 0.0), (587.3, 0.08)):
        note = dsp.sine(freq, 0.20) * 0.7 * dsp.percussive(0.20, 0.060)
        start = int(offset * dsp.SAMPLE_RATE)
        out = dsp.mix_at(out, note, start)
    return dsp.match_rms(dsp.fade(out, 0.002, 0.06), -25.0)


def ui_error() -> np.ndarray:
    """Dull buzz — wrong answer, invalid input."""
    length = 0.36
    buzz = dsp.saw(150.0, length) * 0.6 + dsp.saw(151.6, length) * 0.5
    buzz = dsp.lowpass(buzz, 1400.0)
    gate = 0.5 + 0.5 * np.sign(dsp.sine(18.0, length))
    return dsp.match_rms(
        dsp.fade(buzz * gate * dsp.adsr(length, 0.01, 0.10, 0.55, 0.20), 0.003, 0.06),
        -23.0)


def keyboard_key() -> np.ndarray:
    """Soft key press for the name-entry keyboard."""
    length = 0.09
    tick = dsp.sine(1200.0, length) * dsp.percussive(length, 0.008) * 0.5
    body = dsp.sine(420.0, length) * dsp.percussive(length, 0.016) * 0.5
    return dsp.match_rms(dsp.fade(tick + body, 0.001, 0.03), -27.0)


# --------------------------------------------------------------- game events

def task_correct(rng: np.random.Generator) -> np.ndarray:
    """Bright three-note arpeggio — a correct safety choice."""
    length = 0.85
    out = np.zeros(dsp.n_samples(length))
    for midi, offset in ((72, 0.00), (76, 0.075), (79, 0.150), (84, 0.225)):
        freq = 440.0 * 2 ** ((midi - 69) / 12)
        note = (dsp.sine(freq, 0.55) + dsp.sine(freq * 2, 0.55) * 0.28)
        note *= dsp.percussive(0.55, 0.16)
        start = int(offset * dsp.SAMPLE_RATE)
        out = dsp.mix_at(out, note * 0.6, start)
    return dsp.match_rms(_tail(dsp.fade(out, 0.002, 0.12), rng, 2.0, 0.30), -21.0)


def task_wrong(rng: np.random.Generator) -> np.ndarray:
    """Descending minor cue — an unsafe choice."""
    length = 0.80
    out = np.zeros(dsp.n_samples(length))
    for midi, offset in ((67, 0.00), (63, 0.11), (60, 0.22)):
        freq = 440.0 * 2 ** ((midi - 69) / 12)
        note = (dsp.sine(freq, 0.50) + dsp.triangle(freq, 0.50) * 0.25)
        note *= dsp.percussive(0.50, 0.14)
        start = int(offset * dsp.SAMPLE_RATE)
        out = dsp.mix_at(out, note * 0.6, start)
    return dsp.match_rms(_tail(dsp.fade(out, 0.002, 0.12), rng, 1.8, 0.25), -22.0)


def game_start(rng: np.random.Generator) -> np.ndarray:
    """Upward swell into a bright chord — the run begins."""
    length = 1.6
    swell = dsp.bandpass(dsp.pink_noise(0.9, rng), 300.0, 6000.0)
    swell *= np.linspace(0.0, 1.0, len(swell)) ** 2 * 0.35

    chord = np.zeros(dsp.n_samples(length))
    for midi in (60, 64, 67, 72):
        freq = 440.0 * 2 ** ((midi - 69) / 12)
        note = dsp.sine(freq, 1.0) * dsp.percussive(1.0, 0.32)
        start = int(0.75 * dsp.SAMPLE_RATE)
        chord = dsp.mix_at(chord, note * 0.4, start)
    chord[:len(swell)] += swell
    return dsp.match_rms(_tail(dsp.fade(chord, 0.005, 0.25), rng, 2.6, 0.35), -20.0)


def game_end(rng: np.random.Generator) -> np.ndarray:
    """Settled resolving chord — the run is over."""
    length = 2.4
    out = np.zeros(dsp.n_samples(length))
    for midi, offset in ((53, 0.0), (60, 0.05), (64, 0.10), (69, 0.15)):
        freq = 440.0 * 2 ** ((midi - 69) / 12)
        note = (dsp.sine(freq, 1.9) + dsp.sine(freq * 2, 1.9) * 0.22)
        note *= dsp.percussive(1.9, 0.60)
        start = int(offset * dsp.SAMPLE_RATE)
        out = dsp.mix_at(out, note * 0.45, start)
    return dsp.match_rms(_tail(dsp.fade(out, 0.005, 0.4), rng, 3.0, 0.40), -21.0)


def countdown_tick() -> np.ndarray:
    length = 0.12
    tone = dsp.sine(1046.5, length) * dsp.percussive(length, 0.024)
    return dsp.match_rms(dsp.fade(tone, 0.001, 0.04), -26.0)


def teleport(rng: np.random.Generator) -> np.ndarray:
    """Short airy whoosh with a pitch rise."""
    length = 0.60
    air = dsp.bandpass(dsp.pink_noise(length, rng), 400.0, 9000.0)
    air *= np.hanning(len(air)) ** 1.5
    rise = dsp.sine_sweep(320.0, 1400.0, length) * dsp.percussive(length, 0.22) * 0.35
    return dsp.match_rms(_tail(dsp.fade(air * 0.8 + rise, 0.005, 0.15), rng, 2.0, 0.30), -23.0)


# ---------------------------------------------------------------- physicality

def wood_impact(rng: np.random.Generator) -> np.ndarray:
    """Wooden crate or pallet hitting the floor."""
    length = 0.45
    hit = dsp.percussive(length, 0.035, attack=0.0008) * dsp.noise(length, rng)
    body = sum(
        dsp.resonant(hit, freq, q=7.0) * gain
        for freq, gain in ((180.0, 1.0), (330.0, 0.5), (620.0, 0.25))
    )
    return dsp.match_rms(_tail(dsp.fade(body, 0.0005, 0.10), rng, 1.9, 0.26), -20.0)


def metal_impact(rng: np.random.Generator) -> np.ndarray:
    """Steel shelving or a dropped tool."""
    length = 1.1
    hit = dsp.percussive(length, 0.028, attack=0.0005) * dsp.noise(length, rng)
    ring = sum(
        dsp.sine(freq, length) * gain * dsp.exp_decay(length, tau)
        for freq, gain, tau in ((430.0, 0.6, 0.30), (970.0, 0.45, 0.22),
                                (1830.0, 0.28, 0.15), (3100.0, 0.15, 0.10))
    )
    mixed = dsp.bandpass(hit, 250.0, 9000.0) * 0.7 + ring * 0.6
    return dsp.match_rms(_tail(dsp.fade(mixed, 0.0005, 0.20), rng, 2.4, 0.34), -20.0)


def cardboard_impact(rng: np.random.Generator) -> np.ndarray:
    """Light box — dull, damped, no ring."""
    length = 0.30
    hit = dsp.percussive(length, 0.022, attack=0.001) * dsp.noise(length, rng)
    dull = dsp.lowpass(hit, 900.0)
    scuff = dsp.bandpass(dsp.noise(length, rng), 2000.0, 7000.0)
    scuff *= dsp.percussive(length, 0.040, attack=0.005) * 0.25
    return dsp.match_rms(_tail(dsp.fade(dull + scuff, 0.0005, 0.08), rng, 1.5, 0.18), -22.0)


def water_splash(rng: np.random.Generator) -> np.ndarray:
    """Foot into a spill — the wet-floor hazard."""
    length = 0.70
    burst = dsp.bandpass(dsp.noise(length, rng), 600.0, 8000.0)
    burst *= dsp.percussive(length, 0.075, attack=0.002)
    droplets = np.zeros(len(burst))
    for _ in range(9):
        blip = dsp.sine(rng.uniform(1400, 3600), 0.05) * dsp.percussive(0.05, 0.010)
        start = int(rng.uniform(0.03, 0.45) * dsp.SAMPLE_RATE)
        droplets = dsp.mix_at(droplets, blip * rng.uniform(0.15, 0.4), start)
    return dsp.match_rms(_tail(dsp.fade(burst + droplets, 0.002, 0.15), rng, 1.8, 0.24), -22.0)


def extinguisher_spray(rng: np.random.Generator) -> np.ndarray:
    """Looping CO2 discharge."""
    seconds = 6.0
    jet = dsp.bandpass(dsp.pink_noise(seconds, rng), 900.0, 11000.0)
    body = dsp.lowpass(dsp.brown_noise(seconds, rng), 400.0) * 0.35
    turbulence = 0.85 + 0.15 * (0.5 + 0.5 * dsp.sine(7.0, seconds))
    mixed = dsp.normalise((jet + body) * turbulence)
    return dsp.match_rms(dsp.seamless_loop(mixed, 1.0), -21.0)


# --------------------------------------------------------------- machines

def forklift_idle(rng: np.random.Generator) -> np.ndarray:
    """Looping diesel-ish idle."""
    seconds = 8.0
    firing = sum(
        dsp.sine(freq, seconds) * gain
        for freq, gain in ((22.0, 1.0), (44.0, 0.6), (66.0, 0.35),
                           (88.0, 0.20), (132.0, 0.10))
    )
    roughness = dsp.lowpass(dsp.brown_noise(seconds, rng), 600.0) * 0.5
    wobble = 0.9 + 0.1 * dsp.sine(2.6, seconds)
    mixed = dsp.soft_clip(dsp.normalise((firing + roughness) * wobble), drive=1.6)
    return dsp.match_rms(dsp.seamless_loop(dsp.lowpass(mixed, 3000.0), 1.0), -22.0)


def forklift_beep(rng: np.random.Generator) -> np.ndarray:
    """Reversing alarm — one beep, meant to be looped with a gap."""
    length = 0.90
    out = np.zeros(dsp.n_samples(length))
    beep = dsp.sine(1000.0, 0.30) * 0.7 + dsp.sine(2000.0, 0.30) * 0.2
    beep *= dsp.adsr(0.30, 0.005, 0.02, 0.9, 0.05)
    out[:len(beep)] += beep
    return dsp.match_rms(_tail(dsp.fade(out, 0.002, 0.05), rng, 2.2, 0.30), -21.0)


def alarm_loop(rng: np.random.Generator) -> np.ndarray:
    """Fire/evacuation alarm — looping two-tone."""
    seconds = 2.0
    out = np.zeros(dsp.n_samples(seconds))
    for freq, offset in ((880.0, 0.0), (660.0, 0.5), (880.0, 1.0), (660.0, 1.5)):
        tone = dsp.sine(freq, 0.45) * 0.6 + dsp.sine(freq * 2, 0.45) * 0.25
        tone *= dsp.adsr(0.45, 0.01, 0.05, 0.85, 0.10)
        start = int(offset * dsp.SAMPLE_RATE)
        end = min(start + len(tone), len(out))
        out[start:end] += tone[:end - start]
    return dsp.match_rms(dsp.normalise(out), -20.0)



def door_open(rng: np.random.Generator) -> np.ndarray:
    """Industrial door: latch clack, motor-driven slide, settling clunk."""
    length = 2.0
    out = np.zeros(dsp.n_samples(length))

    latch = dsp.percussive(0.25, 0.012, attack=0.0005) * dsp.noise(0.25, rng)
    latch = dsp.bandpass(latch, 900.0, 6000.0) * 0.8
    out = dsp.mix_at(out, latch, 0)

    slide = dsp.bandpass(dsp.pink_noise(1.3, rng), 150.0, 2400.0)
    motor = dsp.sine(78.0, 1.3) * 0.5 + dsp.sine(156.0, 1.3) * 0.2
    travel = (slide * 0.7 + motor * 0.6) * dsp.fade(np.ones(dsp.n_samples(1.3)), 0.12, 0.25)
    out = dsp.mix_at(out, travel * 0.55, dsp.n_samples(0.18))

    clunk = dsp.percussive(0.5, 0.045, attack=0.001) * dsp.noise(0.5, rng)
    clunk = dsp.resonant(clunk, 150.0, q=6.0) * 0.9
    out = dsp.mix_at(out, clunk, dsp.n_samples(1.45))

    return dsp.match_rms(_tail(dsp.fade(out, 0.001, 0.15), rng, 2.2, 0.30), -21.0)


def door_close(rng: np.random.Generator) -> np.ndarray:
    """The same mechanism closing — travel first, then a heavier impact."""
    length = 1.9
    out = np.zeros(dsp.n_samples(length))

    slide = dsp.bandpass(dsp.pink_noise(1.2, rng), 150.0, 2200.0)
    motor = dsp.sine(72.0, 1.2) * 0.5 + dsp.sine(144.0, 1.2) * 0.2
    travel = (slide * 0.7 + motor * 0.6) * dsp.fade(np.ones(dsp.n_samples(1.2)), 0.15, 0.20)
    out = dsp.mix_at(out, travel * 0.5, 0)

    impact = dsp.percussive(0.7, 0.055, attack=0.0008) * dsp.noise(0.7, rng)
    impact = (dsp.resonant(impact, 120.0, q=5.0) * 1.0
              + dsp.bandpass(impact, 600.0, 4000.0) * 0.35)
    out = dsp.mix_at(out, impact, dsp.n_samples(1.15))

    return dsp.match_rms(_tail(dsp.fade(out, 0.002, 0.15), rng, 2.4, 0.32), -20.0)


def catalogue(seed: int) -> dict:
    def rng(name: str):
        return dsp.rng_for(name, seed)

    clips = {
        "SFX_UI_Hover": ui_hover(),
        "SFX_UI_Click": ui_click(),
        "SFX_UI_Confirm": ui_confirm(),
        "SFX_UI_Back": ui_back(),
        "SFX_UI_Error": ui_error(),
        "SFX_Keyboard_Key": keyboard_key(),
        "SFX_Countdown_Tick": countdown_tick(),
        "SFX_Task_Correct": task_correct(rng("SFX_Task_Correct")),
        "SFX_Task_Wrong": task_wrong(rng("SFX_Task_Wrong")),
        "SFX_Game_Start": game_start(rng("SFX_Game_Start")),
        "SFX_Game_End": game_end(rng("SFX_Game_End")),
        "SFX_Teleport": teleport(rng("SFX_Teleport")),
        "SFX_Wood_Impact": wood_impact(rng("SFX_Wood_Impact")),
        "SFX_Metal_Impact": metal_impact(rng("SFX_Metal_Impact")),
        "SFX_Cardboard_Impact": cardboard_impact(rng("SFX_Cardboard_Impact")),
        "SFX_Water_Splash": water_splash(rng("SFX_Water_Splash")),
        "SFX_Extinguisher_Spray": extinguisher_spray(rng("SFX_Extinguisher_Spray")),
        "SFX_Forklift_Idle": forklift_idle(rng("SFX_Forklift_Idle")),
        "SFX_Forklift_Beep": forklift_beep(rng("SFX_Forklift_Beep")),
        "SFX_Alarm_Loop": alarm_loop(rng("SFX_Alarm_Loop")),
        "SFX_Door_Open": door_open(rng("SFX_Door_Open")),
        "SFX_Door_Close": door_close(rng("SFX_Door_Close")),
    }
    for index in range(1, 7):
        key = f"SFX_Footstep_Concrete_{index:02d}"
        clips[key] = footstep_concrete(rng(key))
    for index in range(1, 4):
        key = f"SFX_Footstep_Metal_{index:02d}"
        clips[key] = footstep_metal(rng(key))
    return clips
