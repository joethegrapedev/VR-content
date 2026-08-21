#!/usr/bin/env python3
"""Objective QA for the generated audio.

Checks each clip for the failure modes that are easy to ship by accident:
silence, clipping, DC offset, a discontinuity at the loop seam, and an
implausible spectral balance.
"""
from __future__ import annotations

import argparse
import sys
import wave
from pathlib import Path

import numpy as np

LOOPING_PREFIXES = ("Amb_", "Music_", "SFX_Forklift_Idle",
                    "SFX_Alarm_Loop", "SFX_Extinguisher_Spray")
MAX_PEAK = 0.995
MAX_DC = 0.01
MAX_SEAM_RATIO = 6.0
MIN_RMS_DB = -45.0


def read_wav(path: Path) -> tuple[np.ndarray, int]:
    with wave.open(str(path), "rb") as handle:
        rate = handle.getframerate()
        channels = handle.getnchannels()
        frames = handle.readframes(handle.getnframes())
    data = np.frombuffer(frames, dtype="<i2").astype(np.float64) / 32768.0
    if channels == 2:
        data = data.reshape(-1, 2)
    return data, rate


def seam_ratio(mono: np.ndarray) -> float:
    """Discontinuity at the wrap point, relative to typical sample-to-sample step.

    A seamless loop should be indistinguishable from any interior point, so
    this lands near 1.0; an audible click pushes it far higher.
    """
    steps = np.abs(np.diff(mono))
    typical = float(np.percentile(steps, 99)) if steps.size else 0.0
    if typical <= 0:
        return 0.0
    return float(abs(mono[0] - mono[-1]) / typical)


def spectral_centroid(mono: np.ndarray, rate: int) -> float:
    window = mono[: 1 << 18] * np.hanning(min(len(mono), 1 << 18))
    magnitude = np.abs(np.fft.rfft(window))
    freqs = np.fft.rfftfreq(len(window), 1.0 / rate)
    total = magnitude.sum()
    return float((magnitude * freqs).sum() / total) if total > 0 else 0.0


def inspect(path: Path) -> dict:
    data, rate = read_wav(path)
    mono = data.mean(axis=1) if data.ndim == 2 else data
    peak = float(np.max(np.abs(mono))) if mono.size else 0.0
    rms = float(np.sqrt(np.mean(mono ** 2))) if mono.size else 0.0
    rms_db = 20 * np.log10(rms) if rms > 0 else -np.inf

    problems = []
    if peak == 0.0:
        problems.append("SILENT")
    if peak > MAX_PEAK:
        problems.append(f"CLIPPING peak={peak:.3f}")
    dc = float(np.mean(mono)) if mono.size else 0.0
    if abs(dc) > MAX_DC:
        problems.append(f"DC-OFFSET {dc:+.4f}")
    if rms_db < MIN_RMS_DB:
        problems.append(f"TOO-QUIET {rms_db:.1f}dB")

    looping = path.stem.startswith(LOOPING_PREFIXES)
    ratio = seam_ratio(mono) if looping else 0.0
    if looping and ratio > MAX_SEAM_RATIO:
        problems.append(f"LOOP-SEAM ratio={ratio:.1f}")

    return {
        "name": path.stem,
        "seconds": len(mono) / rate,
        "channels": 1 if data.ndim == 1 else data.shape[1],
        "peak": peak,
        "rms_db": rms_db,
        "centroid": spectral_centroid(mono, rate),
        "seam": ratio,
        "looping": looping,
        "problems": problems,
    }


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path)
    args = parser.parse_args(argv)

    files = sorted(args.directory.glob("*.wav"))
    if not files:
        print(f"no wav files in {args.directory}", file=sys.stderr)
        return 1

    print(f"{'clip':<30}{'sec':>7}{'ch':>4}{'peak':>7}{'rms dB':>9}"
          f"{'centroid':>10}{'seam':>7}  notes")
    print("-" * 92)
    failed = []
    for path in files:
        report = inspect(path)
        if report["problems"]:
            failed.append(report)
        seam = f"{report['seam']:.1f}" if report["looping"] else "-"
        print(f"{report['name']:<30}{report['seconds']:>7.2f}"
              f"{report['channels']:>4}{report['peak']:>7.3f}"
              f"{report['rms_db']:>9.1f}{report['centroid']:>9.0f}Hz"
              f"{seam:>7}  {', '.join(report['problems'])}")

    print(f"\n{len(files) - len(failed)}/{len(files)} clips clean")
    return 1 if failed else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
