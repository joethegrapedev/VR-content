"""Core DSP primitives for procedural audio generation.

All functions are pure: they take arrays and return new arrays, never
mutating their inputs. Sample rate is passed explicitly rather than held
as global state.
"""
from __future__ import annotations

import hashlib

import numpy as np

SAMPLE_RATE = 44100
EPSILON = 1e-12


class SynthesisError(Exception):
    """Raised when a synthesis parameter is outside its valid range."""


def _validate_duration(seconds: float) -> None:
    if not np.isfinite(seconds) or seconds <= 0:
        raise SynthesisError(f"duration must be positive and finite, got {seconds!r}")


def rng_for(name: str, seed: int) -> np.random.Generator:
    """A generator private to one clip.

    Deriving each clip's stream from its own name means adding, removing or
    reordering clips in a catalogue cannot change any other clip's output.
    A single shared stream made every clip depend on generation order.
    """
    digest = hashlib.sha256(f"{seed}:{name}".encode()).digest()
    return np.random.default_rng(int.from_bytes(digest[:8], "little"))


def n_samples(seconds: float, rate: int = SAMPLE_RATE) -> int:
    """Canonical sample count for a duration.

    Every module must derive lengths from this so independently generated
    layers are guaranteed to line up sample-for-sample.
    """
    _validate_duration(seconds)
    return int(round(seconds * rate))


def time_axis(seconds: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    """Return a time axis in seconds, exclusive of the end point."""
    return np.arange(n_samples(seconds, rate), dtype=np.float64) / rate


def silence(seconds: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    return np.zeros(n_samples(seconds, rate), dtype=np.float64)


# ---------------------------------------------------------------- oscillators

def sine(freq: float, seconds: float, phase: float = 0.0,
         rate: int = SAMPLE_RATE) -> np.ndarray:
    return np.sin(2 * np.pi * freq * time_axis(seconds, rate) + phase)


def sine_sweep(f0: float, f1: float, seconds: float,
               rate: int = SAMPLE_RATE) -> np.ndarray:
    """Linear-frequency chirp from f0 to f1."""
    t = time_axis(seconds, rate)
    inst = f0 + (f1 - f0) * (t / max(t[-1], EPSILON))
    return np.sin(2 * np.pi * np.cumsum(inst) / rate)


def saw(freq: float, seconds: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    t = time_axis(seconds, rate)
    return 2.0 * ((freq * t) % 1.0) - 1.0


def triangle(freq: float, seconds: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    return 2.0 * np.abs(saw(freq, seconds, rate)) - 1.0


def noise(seconds: float, rng: np.random.Generator,
          rate: int = SAMPLE_RATE) -> np.ndarray:
    return rng.standard_normal(n_samples(seconds, rate))


def pink_noise(seconds: float, rng: np.random.Generator,
               rate: int = SAMPLE_RATE) -> np.ndarray:
    """Pink (1/f) noise via spectral shaping — smoother than filter cascades."""
    n = n_samples(seconds, rate)
    spectrum = np.fft.rfft(rng.standard_normal(n))
    freqs = np.fft.rfftfreq(n, 1.0 / rate)
    shaping = np.ones_like(freqs)
    shaping[1:] = 1.0 / np.sqrt(freqs[1:])
    return normalise(np.fft.irfft(spectrum * shaping, n))


def brown_noise(seconds: float, rng: np.random.Generator,
                rate: int = SAMPLE_RATE) -> np.ndarray:
    """Brown (1/f^2) noise — deep rumble, good for distant machinery."""
    n = n_samples(seconds, rate)
    spectrum = np.fft.rfft(rng.standard_normal(n))
    freqs = np.fft.rfftfreq(n, 1.0 / rate)
    shaping = np.ones_like(freqs)
    shaping[1:] = 1.0 / freqs[1:]
    return normalise(np.fft.irfft(spectrum * shaping, n))


# ----------------------------------------------------------------- envelopes

def adsr(seconds: float, attack: float, decay: float, sustain: float,
         release: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    """Classic ADSR contour clamped to the requested duration."""
    n = n_samples(seconds, rate)
    if not 0.0 <= sustain <= 1.0:
        raise SynthesisError(f"sustain must be in [0, 1], got {sustain!r}")

    a = min(int(attack * rate), n)
    d = min(int(decay * rate), max(n - a, 0))
    r = min(int(release * rate), max(n - a - d, 0))
    s = max(n - a - d - r, 0)

    return np.concatenate([
        np.linspace(0.0, 1.0, a, endpoint=False),
        np.linspace(1.0, sustain, d, endpoint=False),
        np.full(s, sustain),
        np.linspace(sustain, 0.0, r),
    ])[:n]


def exp_decay(seconds: float, tau: float, rate: int = SAMPLE_RATE) -> np.ndarray:
    """Exponential decay with time constant tau (seconds)."""
    if tau <= 0:
        raise SynthesisError(f"tau must be positive, got {tau!r}")
    return np.exp(-time_axis(seconds, rate) / tau)


def percussive(seconds: float, tau: float, attack: float = 0.002,
               rate: int = SAMPLE_RATE) -> np.ndarray:
    """Fast attack into an exponential tail — the shape of an impact."""
    env = exp_decay(seconds, tau, rate)
    a = max(int(attack * rate), 1)
    ramp = np.ones_like(env)
    ramp[:a] = np.linspace(0.0, 1.0, a)
    return env * ramp


def fade(signal: np.ndarray, fade_in: float = 0.01, fade_out: float = 0.01,
         rate: int = SAMPLE_RATE) -> np.ndarray:
    """Apply raised-cosine fades to both ends without mutating the input."""
    out = np.array(signal, dtype=np.float64, copy=True)
    n = len(out)
    fi = min(int(fade_in * rate), n // 2)
    fo = min(int(fade_out * rate), n // 2)
    if fi > 0:
        out[:fi] *= 0.5 * (1 - np.cos(np.linspace(0, np.pi, fi)))
    if fo > 0:
        out[-fo:] *= 0.5 * (1 + np.cos(np.linspace(0, np.pi, fo)))
    return out


# ------------------------------------------------------------------- filters

def _spectral_mask(n: int, rate: int, mask_fn) -> np.ndarray:
    freqs = np.fft.rfftfreq(n, 1.0 / rate)
    return mask_fn(freqs)


def lowpass(signal: np.ndarray, cutoff: float, order: int = 2,
            rate: int = SAMPLE_RATE) -> np.ndarray:
    """Zero-phase Butterworth-style lowpass in the frequency domain."""
    if cutoff <= 0:
        raise SynthesisError(f"cutoff must be positive, got {cutoff!r}")
    n = len(signal)
    mask = _spectral_mask(
        n, rate, lambda f: 1.0 / np.sqrt(1.0 + (f / cutoff) ** (2 * order)))
    return np.fft.irfft(np.fft.rfft(signal) * mask, n)


def highpass(signal: np.ndarray, cutoff: float, order: int = 2,
             rate: int = SAMPLE_RATE) -> np.ndarray:
    if cutoff <= 0:
        raise SynthesisError(f"cutoff must be positive, got {cutoff!r}")
    n = len(signal)

    def mask_fn(f):
        ratio = np.divide(cutoff, np.maximum(f, EPSILON))
        return 1.0 / np.sqrt(1.0 + ratio ** (2 * order))

    return np.fft.irfft(np.fft.rfft(signal) * _spectral_mask(n, rate, mask_fn), n)


def bandpass(signal: np.ndarray, low: float, high: float, order: int = 2,
             rate: int = SAMPLE_RATE) -> np.ndarray:
    if low >= high:
        raise SynthesisError(f"low ({low}) must be below high ({high})")
    return lowpass(highpass(signal, low, order, rate), high, order, rate)


def resonant(signal: np.ndarray, freq: float, q: float = 12.0,
             rate: int = SAMPLE_RATE) -> np.ndarray:
    """Emphasise a single frequency — used to give impacts a pitched body."""
    if freq <= 0 or q <= 0:
        raise SynthesisError(f"freq and q must be positive, got {freq!r}, {q!r}")
    n = len(signal)
    bandwidth = freq / q

    def mask_fn(f):
        return 1.0 / np.sqrt(1.0 + ((f - freq) / bandwidth) ** 2)

    return np.fft.irfft(np.fft.rfft(signal) * _spectral_mask(n, rate, mask_fn), n)


def fft_convolve(signal: np.ndarray, kernel: np.ndarray) -> np.ndarray:
    """Linear convolution via FFT.

    Direct convolution is O(n*m), which is untenable for minute-long beds
    against a multi-second impulse response; this is O(n log n).
    """
    n = len(signal) + len(kernel) - 1
    size = 1 << (n - 1).bit_length()
    spectrum = np.fft.rfft(signal, size) * np.fft.rfft(kernel, size)
    return np.fft.irfft(spectrum, size)[:n]


# -------------------------------------------------------------------- reverb

def reverb(signal: np.ndarray, room_seconds: float, wet: float,
           rng: np.random.Generator, damping: float = 4000.0,
           predelay: float = 0.02, rate: int = SAMPLE_RATE) -> np.ndarray:
    """Convolution reverb against a synthetic exponential-noise impulse.

    A warehouse is a large reflective box; room_seconds around 1.8-3.0 with
    heavy damping reproduces that without a real impulse response.
    """
    if not 0.0 <= wet <= 1.0:
        raise SynthesisError(f"wet must be in [0, 1], got {wet!r}")
    _validate_duration(room_seconds)

    ir_len = int(room_seconds * rate)
    tail = rng.standard_normal(ir_len) * np.exp(
        -np.linspace(0, 6.0, ir_len))
    tail = lowpass(tail, damping, order=1, rate=rate)
    # Sparse early reflections keep the tail from sounding like plain noise.
    for delay_ms, gain in ((11, 0.7), (23, 0.5), (37, 0.4), (53, 0.3)):
        idx = int(delay_ms * rate / 1000)
        if idx < ir_len:
            tail[idx] += gain
    tail = tail / (np.max(np.abs(tail)) + EPSILON)

    pre = int(predelay * rate)
    padded = np.concatenate([np.zeros(pre), signal])
    wet_signal = fft_convolve(padded, tail)[:len(signal)]
    wet_signal = wet_signal / (np.max(np.abs(wet_signal)) + EPSILON)
    dry_signal = signal / (np.max(np.abs(signal)) + EPSILON)
    return (1.0 - wet) * dry_signal + wet * wet_signal


# ------------------------------------------------------------------ dynamics

def remove_dc(signal: np.ndarray, cutoff: float = 18.0,
              rate: int = SAMPLE_RATE) -> np.ndarray:
    """Strip sub-audible DC/rumble that heavy lowpassing can leave behind.

    A DC offset eats headroom and can thump speakers on loop restart.
    """
    centred = signal - np.mean(signal)
    return highpass(centred, cutoff, order=1, rate=rate)


def normalise(signal: np.ndarray, peak: float = 1.0) -> np.ndarray:
    """Scale so the largest absolute sample equals `peak`."""
    magnitude = np.max(np.abs(signal))
    if magnitude < EPSILON:
        return np.zeros_like(signal)
    return signal * (peak / magnitude)


def soft_clip(signal: np.ndarray, drive: float = 1.0) -> np.ndarray:
    """Gentle tanh saturation — tames peaks without audible clipping."""
    if drive <= 0:
        raise SynthesisError(f"drive must be positive, got {drive!r}")
    return np.tanh(signal * drive) / np.tanh(drive)


def rms(signal: np.ndarray) -> float:
    return float(np.sqrt(np.mean(np.square(signal)) + EPSILON))


def match_rms(signal: np.ndarray, target_db: float) -> np.ndarray:
    """Scale a signal to a target RMS in dBFS, then guard against clipping."""
    target = 10.0 ** (target_db / 20.0)
    scaled = signal * (target / rms(signal))
    peak = np.max(np.abs(scaled))
    return scaled / peak * 0.99 if peak > 0.99 else scaled


def mix_at(timeline: np.ndarray, signal: np.ndarray, start: int) -> np.ndarray:
    """Additively mix `signal` into a copy of `timeline` at sample `start`.

    Anything past the end of the timeline is truncated rather than raising,
    so layers can overhang without every caller re-deriving the bounds.
    """
    out = np.array(timeline, dtype=np.float64, copy=True)
    if start >= len(out) or start + len(signal) <= 0:
        return out
    begin = max(start, 0)
    clipped = signal[begin - start:]
    end = min(begin + len(clipped), len(out))
    out[begin:end] += clipped[:end - begin]
    return out


# --------------------------------------------------------------- loop helper

def seamless_loop(signal: np.ndarray, crossfade: float,
                  rate: int = SAMPLE_RATE) -> np.ndarray:
    """Fold the tail back over the head so the result loops without a seam.

    The returned clip is `crossfade` seconds shorter than the input.
    """
    n_cross = int(crossfade * rate)
    if n_cross <= 0 or n_cross * 2 >= len(signal):
        raise SynthesisError(
            f"crossfade {crossfade}s does not fit signal of {len(signal)} samples")

    head = np.array(signal[:-n_cross], dtype=np.float64, copy=True)
    tail = signal[-n_cross:]
    ramp = np.linspace(0.0, 1.0, n_cross)
    head[:n_cross] = head[:n_cross] * ramp + tail * (1.0 - ramp)
    return head


def to_stereo(left: np.ndarray, right: np.ndarray) -> np.ndarray:
    """Interleave two equal-length channels into an (n, 2) array."""
    if len(left) != len(right):
        raise SynthesisError(
            f"channel length mismatch: {len(left)} vs {len(right)}")
    return np.stack([left, right], axis=1)


def widen(signal: np.ndarray, amount: float, rng: np.random.Generator,
          rate: int = SAMPLE_RATE) -> np.ndarray:
    """Turn a mono signal into a wide stereo pair via decorrelated reverb."""
    left = reverb(signal, 1.2, amount * 0.5, rng, rate=rate)
    right = reverb(signal, 1.3, amount * 0.5, rng, rate=rate)
    return to_stereo(left, right)
