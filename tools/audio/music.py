"""Calm ambient background music.

Sparse mallet/piano-like notes over slow pads, in the vein of the quiet
Minecraft soundtrack: wide voicings, lots of space, heavy reverb. Every
track loops seamlessly and is mixed well below dialogue so it never
competes with the safety narration.
"""
from __future__ import annotations

import numpy as np

import dsp

A4_MIDI = 69
A4_HZ = 440.0
CROSSFADE = 3.0

# Scale degrees as semitone offsets from the root.
MAJOR_PENTATONIC = (0, 2, 4, 7, 9)
MINOR_PENTATONIC = (0, 3, 5, 7, 10)


class Note:
    """An immutable note event positioned on a beat grid."""

    __slots__ = ("midi", "start_beat", "beats", "velocity")

    def __init__(self, midi: int, start_beat: float, beats: float,
                 velocity: float = 1.0):
        if not 0 <= midi <= 127:
            raise dsp.SynthesisError(f"midi out of range: {midi}")
        if beats <= 0:
            raise dsp.SynthesisError(f"beats must be positive, got {beats}")
        self.midi = midi
        self.start_beat = start_beat
        self.beats = beats
        self.velocity = velocity


def midi_to_hz(midi: float) -> float:
    return A4_HZ * (2.0 ** ((midi - A4_MIDI) / 12.0))


def mallet_tone(freq: float, seconds: float, velocity: float,
                rng: np.random.Generator) -> np.ndarray:
    """Soft struck tone: partials decay faster as they climb, like felt piano."""
    partials = ((1.0, 1.00, 1.00), (2.0, 0.42, 0.62), (3.0, 0.20, 0.42),
                (4.0, 0.11, 0.30), (5.5, 0.06, 0.22), (7.0, 0.03, 0.16))
    out = np.zeros(dsp.n_samples(seconds))
    for ratio, gain, decay_scale in partials:
        # Slight inharmonicity and detune stop it sounding like a pure sine stack.
        detune = 1.0 + rng.uniform(-0.0016, 0.0016)
        partial_freq = freq * ratio * detune
        if partial_freq > dsp.SAMPLE_RATE * 0.45:
            continue
        tau = max(seconds * 0.34 * decay_scale, 0.05)
        out += dsp.sine(partial_freq, seconds) * dsp.exp_decay(seconds, tau) * gain

    strike = dsp.percussive(min(0.06, seconds), 0.008) * dsp.noise(
        min(0.06, seconds), rng)
    strike = dsp.bandpass(strike, freq * 1.5, min(freq * 9, 14000.0)) * 0.09
    out[:len(strike)] += strike

    attack = max(int(0.006 * dsp.SAMPLE_RATE), 1)
    out[:attack] *= np.linspace(0.0, 1.0, attack)
    return dsp.normalise(out) * velocity


def pad_tone(freq: float, seconds: float, velocity: float,
             rng: np.random.Generator) -> np.ndarray:
    """Slow-swelling detuned pad, filtered so it stays behind the mix."""
    layers = np.zeros(dsp.n_samples(seconds))
    for cents in (-7.0, -2.5, 0.0, 3.0, 8.0):
        detuned = freq * (2.0 ** (cents / 1200.0))
        layers += dsp.sine(detuned, seconds, phase=rng.uniform(0, 2 * np.pi))
        layers += dsp.triangle(detuned * 2, seconds) * 0.18
    shimmer = 0.85 + 0.15 * (0.5 + 0.5 * dsp.sine(0.08, seconds))
    shaped = dsp.lowpass(layers * shimmer, freq * 4.5 + 250.0, order=2)
    env = dsp.adsr(seconds, seconds * 0.35, seconds * 0.15, 0.75, seconds * 0.45)
    return dsp.normalise(shaped * env) * velocity


def render_notes(notes, bpm: float, total_beats: float, voice,
                 rng: np.random.Generator, tail_seconds: float = 4.0) -> np.ndarray:
    """Lay a sequence of Note events onto a timeline using `voice` to synthesise."""
    beat_seconds = 60.0 / bpm
    length = int((total_beats * beat_seconds + tail_seconds) * dsp.SAMPLE_RATE)
    timeline = np.zeros(length)
    for note in notes:
        duration = note.beats * beat_seconds
        rendered = voice(midi_to_hz(note.midi), duration, note.velocity, rng)
        start = int(note.start_beat * beat_seconds * dsp.SAMPLE_RATE)
        end = min(start + len(rendered), length)
        if start >= length:
            continue
        timeline[start:end] += rendered[:end - start]
    return timeline


def chord_notes(root_midi: int, intervals, start_beat: float, beats: float,
                velocity: float = 0.6):
    return [Note(root_midi + i, start_beat, beats, velocity) for i in intervals]


def _melody(root_midi: int, scale, bars: int, beats_per_bar: float,
            rng: np.random.Generator, density: float, octaves=(0, 12)):
    """Scatter sparse melodic notes over the bar grid."""
    notes = []
    for bar in range(bars):
        for beat in np.arange(0, beats_per_bar, 1.0):
            if rng.random() > density:
                continue
            degree = int(rng.integers(0, len(scale)))
            octave = int(rng.choice(octaves))
            jitter = rng.uniform(-0.04, 0.04)
            notes.append(Note(
                root_midi + scale[degree] + octave,
                bar * beats_per_bar + beat + jitter,
                rng.uniform(1.8, 3.4),
                rng.uniform(0.30, 0.62)))
    return notes


def _assemble(pads: np.ndarray, leads: np.ndarray, rng: np.random.Generator,
              room: float, wet: float, target_db: float) -> np.ndarray:
    """Blend pad and lead layers, apply reverb, and widen to a stereo loop."""
    mono = dsp.normalise(pads * 0.55 + leads * 0.75)
    wetted = dsp.reverb(mono, room, wet, rng, damping=5200.0, predelay=0.03)
    left = dsp.lowpass(wetted, 12000.0)
    right = dsp.lowpass(np.roll(wetted, 480), 11500.0)
    stereo = dsp.to_stereo(
        dsp.seamless_loop(left, CROSSFADE),
        dsp.seamless_loop(right * 0.98, CROSSFADE))
    return dsp.match_rms(stereo, target_db)


def menu_theme(rng: np.random.Generator) -> np.ndarray:
    """Warm and welcoming — plays under the main menu and keyboard scene."""
    bpm, beats_per_bar, bars = 58.0, 4.0, 8
    root = 60  # C4
    progression = ((0, (0, 4, 7, 11)), (9, (0, 3, 7, 10)),
                   (5, (0, 4, 7, 11)), (7, (0, 4, 7, 9)))

    pad_events = []
    for bar in range(bars):
        offset, intervals = progression[bar % len(progression)]
        pad_events += chord_notes(
            root - 12 + offset, intervals, bar * beats_per_bar, beats_per_bar * 0.98, 0.5)

    lead_events = _melody(root + 12, MAJOR_PENTATONIC, bars, beats_per_bar,
                          rng, density=0.34, octaves=(0, 12))

    total = bars * beats_per_bar
    pads = render_notes(pad_events, bpm, total, pad_tone, rng, tail_seconds=5.0)
    leads = render_notes(lead_events, bpm, total, mallet_tone, rng, tail_seconds=5.0)
    return _assemble(pads, leads, rng, room=3.0, wet=0.42, target_db=-24.0)


def gameplay_theme(rng: np.random.Generator) -> np.ndarray:
    """Very sparse — sits under narration without ever pulling focus."""
    bpm, beats_per_bar, bars = 48.0, 4.0, 12
    root = 57  # A3
    progression = ((0, (0, 7, 12)), (5, (0, 7, 11)),
                   (3, (0, 7, 12)), (7, (0, 5, 10)))

    pad_events = []
    for bar in range(bars):
        offset, intervals = progression[bar % len(progression)]
        pad_events += chord_notes(
            root - 12 + offset, intervals, bar * beats_per_bar, beats_per_bar * 1.9, 0.45)

    lead_events = _melody(root + 12, MINOR_PENTATONIC, bars, beats_per_bar,
                          rng, density=0.13, octaves=(0, 12))

    total = bars * beats_per_bar
    pads = render_notes(pad_events, bpm, total, pad_tone, rng, tail_seconds=6.0)
    leads = render_notes(lead_events, bpm, total, mallet_tone, rng, tail_seconds=6.0)
    return _assemble(pads, leads, rng, room=3.4, wet=0.5, target_db=-30.0)


def leaderboard_theme(rng: np.random.Generator) -> np.ndarray:
    """Reflective and light — the wind-down after a run."""
    bpm, beats_per_bar, bars = 64.0, 3.0, 8
    root = 62  # D4
    progression = ((0, (0, 4, 7, 11)), (7, (0, 3, 7, 10)),
                   (5, (0, 4, 7, 9)), (2, (0, 4, 7, 11)))

    pad_events = []
    for bar in range(bars):
        offset, intervals = progression[bar % len(progression)]
        pad_events += chord_notes(
            root - 12 + offset, intervals, bar * beats_per_bar, beats_per_bar * 0.95, 0.5)

    lead_events = _melody(root + 12, MAJOR_PENTATONIC, bars, beats_per_bar,
                          rng, density=0.40, octaves=(0, 12))

    total = bars * beats_per_bar
    pads = render_notes(pad_events, bpm, total, pad_tone, rng, tail_seconds=5.0)
    leads = render_notes(lead_events, bpm, total, mallet_tone, rng, tail_seconds=5.0)
    return _assemble(pads, leads, rng, room=2.8, wet=0.45, target_db=-25.0)


def tension_theme(rng: np.random.Generator) -> np.ndarray:
    """Low unease for hazard moments — drone and minor seconds, no melody."""
    seconds = 40.0
    drone = sum(
        dsp.sine(freq, seconds) * gain
        for freq, gain in ((55.0, 1.0), (58.27, 0.55), (110.0, 0.30), (164.8, 0.12))
    )
    swell = 0.7 + 0.3 * (0.5 + 0.5 * dsp.sine(0.05, seconds))
    grit = dsp.lowpass(dsp.brown_noise(seconds, rng), 500.0) * 0.35
    mono = dsp.normalise(dsp.lowpass(drone * swell + grit, 2400.0))
    wetted = dsp.reverb(mono, 3.6, 0.5, rng, damping=1800.0)
    stereo = dsp.to_stereo(
        dsp.seamless_loop(wetted, CROSSFADE),
        dsp.seamless_loop(np.roll(wetted, 700) * 0.97, CROSSFADE))
    return dsp.match_rms(stereo, -28.0)


def catalogue(seed: int) -> dict:
    def rng(name: str):
        return dsp.rng_for(name, seed)

    return {
        "Music_Menu": menu_theme(rng("Music_Menu")),
        "Music_Gameplay_Calm": gameplay_theme(rng("Music_Gameplay_Calm")),
        "Music_Leaderboard": leaderboard_theme(rng("Music_Leaderboard")),
        "Music_Tension": tension_theme(rng("Music_Tension")),
    }
