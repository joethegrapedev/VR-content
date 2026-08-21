"""Encoding helpers: float arrays in, audio files out.

Unity spatialises only mono sources in 3D, so positional assets must stay
mono; music and global ambience beds are rendered stereo.
"""
from __future__ import annotations

import subprocess
import wave
from pathlib import Path

import numpy as np

import dsp
from dsp import SAMPLE_RATE, SynthesisError

#: Leave a little headroom so Vorbis encoding cannot clip.
PEAK_CEILING = 0.97


class EncodeError(Exception):
    """Raised when the external encoder fails."""


def _to_int16(signal: np.ndarray) -> np.ndarray:
    peak = float(np.max(np.abs(signal))) if signal.size else 0.0
    if peak > 1.0:
        signal = signal / peak
    return np.clip(signal * 32767.0, -32768, 32767).astype("<i2")


def write_wav(signal: np.ndarray, path: Path, rate: int = SAMPLE_RATE) -> Path:
    """Write a mono (n,) or stereo (n, 2) float array as 16-bit PCM."""
    if signal.ndim not in (1, 2):
        raise SynthesisError(f"expected 1-D or 2-D array, got shape {signal.shape}")
    channels = 1 if signal.ndim == 1 else signal.shape[1]
    if channels not in (1, 2):
        raise SynthesisError(f"expected 1 or 2 channels, got {channels}")

    path.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(path), "wb") as handle:
        handle.setnchannels(channels)
        handle.setsampwidth(2)
        handle.setframerate(rate)
        handle.writeframes(_to_int16(signal).tobytes())
    return path


def encode_ogg(wav_path: Path, ogg_path: Path, quality: int = 5) -> Path:
    """Transcode a WAV to Ogg Vorbis, raising with stderr context on failure."""
    ogg_path.parent.mkdir(parents=True, exist_ok=True)
    command = [
        "ffmpeg", "-y", "-loglevel", "error",
        "-i", str(wav_path),
        "-c:a", "libvorbis", "-qscale:a", str(quality),
        str(ogg_path),
    ]
    result = subprocess.run(command, capture_output=True, text=True)
    if result.returncode != 0:
        raise EncodeError(
            f"ffmpeg failed for {wav_path.name} (exit {result.returncode}):\n"
            f"{result.stderr.strip()}")
    if not ogg_path.exists() or ogg_path.stat().st_size == 0:
        raise EncodeError(f"encoder produced no output for {ogg_path.name}")
    return ogg_path


def apply_headroom(signal: np.ndarray, ceiling: float = PEAK_CEILING) -> np.ndarray:
    """Scale down if the peak exceeds the ceiling, leaving quieter clips alone.

    This runs last, after DC removal: filtering can lift the peak back above
    a limit applied earlier in the chain.
    """
    peak = float(np.max(np.abs(signal))) if signal.size else 0.0
    return signal * (ceiling / peak) if peak > ceiling else signal


def render(signal: np.ndarray, name: str, out_dir: Path,
           rate: int = SAMPLE_RATE) -> Path:
    """Render one clip to `<out_dir>/<name>.wav`.

    WAV is deliberately the delivery format: Unity's AudioImporter
    re-compresses to Vorbis for the Android build, so shipping a lossless
    master avoids a double-lossy generation, and unlike MP3 it carries no
    encoder padding that would put a click in a seamless loop.
    """
    if signal.size == 0:
        raise SynthesisError(f"refusing to render empty signal for {name!r}")
    if signal.ndim == 1:
        cleaned = dsp.remove_dc(signal, rate=rate)
    else:
        cleaned = np.stack(
            [dsp.remove_dc(signal[:, ch], rate=rate)
             for ch in range(signal.shape[1])], axis=1)
    return write_wav(apply_headroom(cleaned), out_dir / f"{name}.wav", rate)
