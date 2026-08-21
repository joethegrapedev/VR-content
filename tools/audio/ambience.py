"""Looping ambience beds for the warehouse environment.

Every generator returns a seamlessly loopable clip. Positional emitters are
mono so Unity can spatialise them; the room tone bed is stereo and played 2D.
"""
from __future__ import annotations

import numpy as np

import dsp

MAINS_HZ = 50.0          # Singapore mains frequency; hum sits on its harmonics
ROOM_TONE_SECONDS = 62.0
EMITTER_SECONDS = 14.0
CROSSFADE = 2.0


def _slow_lfo(seconds: float, rate_hz: float, depth: float,
              phase: float = 0.0) -> np.ndarray:
    """A gentle amplitude wobble that keeps static beds from sounding dead."""
    return 1.0 - depth + depth * (0.5 + 0.5 * dsp.sine(rate_hz, seconds, phase))


def hvac_bed(seconds: float, rng: np.random.Generator) -> np.ndarray:
    """Deep air-handling rumble — the backbone of a large indoor space."""
    rumble = dsp.lowpass(dsp.brown_noise(seconds, rng), 160.0, order=2)
    rumble *= _slow_lfo(seconds, 0.045, 0.30)

    drone = sum(
        dsp.sine(freq, seconds, phase) * gain
        for freq, gain, phase in (
            (41.0, 0.55, 0.0), (58.0, 0.32, 1.1), (87.0, 0.18, 2.3))
    )
    drone *= _slow_lfo(seconds, 0.031, 0.22, phase=0.8)

    airflow = dsp.bandpass(dsp.pink_noise(seconds, rng), 300.0, 3200.0)
    airflow *= _slow_lfo(seconds, 0.07, 0.35, phase=2.0) * 0.16

    return dsp.normalise(rumble * 0.7 + drone * 0.5 + airflow)


def fluorescent_hum(seconds: float, rng: np.random.Generator) -> np.ndarray:
    """Ballast buzz — mains harmonics plus a thin crackle."""
    hum = sum(
        dsp.sine(MAINS_HZ * n, seconds) * gain
        for n, gain in ((2, 1.0), (4, 0.45), (6, 0.22), (8, 0.10))
    )
    crackle = dsp.highpass(dsp.pink_noise(seconds, rng), 5000.0) * 0.05
    flicker = _slow_lfo(seconds, 0.9, 0.10)
    return dsp.normalise((hum + crackle) * flicker)


def air_vent(seconds: float, rng: np.random.Generator) -> np.ndarray:
    """Directed air hiss for ceiling vents and ducting."""
    hiss = dsp.bandpass(dsp.pink_noise(seconds, rng), 500.0, 6500.0)
    body = dsp.lowpass(dsp.brown_noise(seconds, rng), 220.0) * 0.4
    return dsp.normalise((hiss + body) * _slow_lfo(seconds, 0.11, 0.25))


def distant_machinery(seconds: float, rng: np.random.Generator) -> np.ndarray:
    """Muffled plant noise heard through a wall — heavily lowpassed."""
    motor = sum(
        dsp.sine(freq, seconds) * gain
        for freq, gain in ((28.0, 0.9), (56.0, 0.4), (112.0, 0.15))
    )
    grind = dsp.lowpass(dsp.brown_noise(seconds, rng), 400.0)
    combined = dsp.lowpass(motor * 0.6 + grind * 0.8, 700.0, order=3)
    return dsp.normalise(combined * _slow_lfo(seconds, 0.06, 0.35))


def conveyor_loop(seconds: float, rng: np.random.Generator) -> np.ndarray:
    """Belt rumble with a periodic roller tick."""
    belt = dsp.bandpass(dsp.pink_noise(seconds, rng), 90.0, 1800.0)
    ticks = np.zeros(len(belt))
    step = int(0.36 * dsp.SAMPLE_RATE)
    click = dsp.percussive(0.05, 0.012) * dsp.noise(0.05, rng)
    click = dsp.bandpass(click, 800.0, 5000.0)
    for start in range(0, len(ticks) - len(click), step):
        ticks = dsp.mix_at(ticks, click * rng.uniform(0.5, 1.0), start)
    return dsp.normalise(belt * 0.8 + ticks * 0.5)


def _sparse_events(seconds: float, rng: np.random.Generator,
                   count: int, make_event) -> np.ndarray:
    """Scatter `count` one-shots across the bed, clear of the loop seam."""
    bed = np.zeros(dsp.n_samples(seconds))
    safe_end = len(bed) - int(CROSSFADE * dsp.SAMPLE_RATE)
    for _ in range(count):
        event = make_event()
        if len(event) >= safe_end:
            continue
        start = int(rng.uniform(0, safe_end - len(event)))
        bed = dsp.mix_at(bed, event, start)
    return bed


def warehouse_room_tone(rng: np.random.Generator) -> np.ndarray:
    """The main stereo bed: HVAC, plant noise and occasional distant activity."""
    seconds = ROOM_TONE_SECONDS

    def distant_clank() -> np.ndarray:
        strike = dsp.percussive(0.9, 0.10) * dsp.noise(0.9, rng)
        strike = dsp.resonant(strike, rng.uniform(180, 520), q=9.0)
        return dsp.reverb(strike, 2.6, 0.85, rng, damping=2200.0) * 0.30

    def forklift_pass() -> np.ndarray:
        length = 3.2
        engine = dsp.lowpass(dsp.brown_noise(length, rng), 300.0)
        engine *= dsp.sine(0.35, length) * 0.3 + 0.7
        swell = np.hanning(dsp.n_samples(length))
        return dsp.reverb(engine * swell, 2.2, 0.7, rng) * 0.22

    base = hvac_bed(seconds, rng) * 0.85 + distant_machinery(seconds, rng) * 0.35
    events = (_sparse_events(seconds, rng, 5, distant_clank)
              + _sparse_events(seconds, rng, 2, forklift_pass))

    mono = dsp.normalise(base + events)
    left = dsp.lowpass(mono, 11000.0) * 1.0
    right = dsp.lowpass(np.roll(mono, 320), 10500.0) * 0.97
    stereo = dsp.to_stereo(
        dsp.seamless_loop(left, CROSSFADE),
        dsp.seamless_loop(right, CROSSFADE))
    return dsp.match_rms(stereo, -26.0)


def menu_room_tone(rng: np.random.Generator) -> np.ndarray:
    """A quieter, calmer bed for menu and leaderboard scenes."""
    seconds = 34.0
    bed = hvac_bed(seconds, rng) * 0.6
    air = dsp.bandpass(dsp.pink_noise(seconds, rng), 400.0, 2600.0) * 0.12
    mono = dsp.normalise(dsp.lowpass(bed + air, 6000.0))
    stereo = dsp.to_stereo(
        dsp.seamless_loop(mono, CROSSFADE),
        dsp.seamless_loop(np.roll(mono, 512) * 0.98, CROSSFADE))
    return dsp.match_rms(stereo, -30.0)


def catalogue(seed: int) -> dict:
    """Name -> clip. Mono entries are positional; stereo are 2-D beds."""
    def rng(name: str):
        return dsp.rng_for(name, seed)

    def emitter(name: str, generator, target_db: float):
        """Build a looping positional emitter from its own generator stream."""
        raw = generator(EMITTER_SECONDS, rng(name))
        return dsp.match_rms(dsp.seamless_loop(raw, CROSSFADE), target_db)

    return {
        "Amb_Warehouse_RoomTone": warehouse_room_tone(rng("Amb_Warehouse_RoomTone")),
        "Amb_Menu_RoomTone": menu_room_tone(rng("Amb_Menu_RoomTone")),
        "Amb_Fluorescent_Hum": emitter(
            "Amb_Fluorescent_Hum", fluorescent_hum, -30.0),
        "Amb_Air_Vent": emitter("Amb_Air_Vent", air_vent, -27.0),
        "Amb_Distant_Machinery": emitter(
            "Amb_Distant_Machinery", distant_machinery, -25.0),
        "Amb_Conveyor": emitter("Amb_Conveyor", conveyor_loop, -25.0),
    }
