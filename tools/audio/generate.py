#!/usr/bin/env python3
"""Render every procedural audio asset into the Unity project.

Usage:
    python generate.py [--out DIR] [--seed N] [--only PREFIX]

All output is original synthesis with no third-party samples, so the
resulting clips carry no licence or attribution obligation.
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

import numpy as np

import ambience
import music
import render
import sfx
from dsp import SAMPLE_RATE, SynthesisError

DEFAULT_OUT = Path(__file__).resolve().parents[2] / \
    "UnityProject/Assets/Resources/Audio"

# Clips live under Resources so they are guaranteed into the IL2CPP build
# rather than depending on a scene reference surviving asset stripping.
CATEGORIES = (("Music_", "Music"), ("Amb_", "Ambience"), ("SFX_", "SFX"))
DEFAULT_SEED = 20260821


def build_catalogue(seed: int) -> dict:
    """Every clip gets its own stream, keyed by name, so edits stay local."""
    return {
        **ambience.catalogue(seed),
        **music.catalogue(seed + 1),
        **sfx.catalogue(seed + 2),
    }


def category_for(name: str) -> str:
    """Map a clip name to its Resources subfolder."""
    for prefix, folder in CATEGORIES:
        if name.startswith(prefix):
            return folder
    raise SynthesisError(f"clip {name!r} has no known category prefix")


def describe(name: str, clip: np.ndarray) -> str:
    channels = 1 if clip.ndim == 1 else clip.shape[1]
    seconds = len(clip) / SAMPLE_RATE
    peak = float(np.max(np.abs(clip)))
    return (f"{name:<34} {seconds:>6.2f}s  "
            f"{'mono' if channels == 1 else 'stereo'}  peak {peak:.3f}")


def main(argv: list[str]) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--out", type=Path, default=DEFAULT_OUT)
    parser.add_argument("--seed", type=int, default=DEFAULT_SEED)
    parser.add_argument("--only", default="",
                        help="render only clips whose name starts with this")
    args = parser.parse_args(argv)

    try:
        catalogue = build_catalogue(args.seed)
    except SynthesisError as error:
        print(f"synthesis failed: {error}", file=sys.stderr)
        return 1

    selected = {name: clip for name, clip in catalogue.items()
                if name.startswith(args.only)}
    if not selected:
        print(f"no clips match prefix {args.only!r}", file=sys.stderr)
        return 1

    args.out.mkdir(parents=True, exist_ok=True)
    failures = []
    for name in sorted(selected):
        clip = selected[name]
        try:
            path = render.render(
                clip, name, args.out / category_for(name))
        except (SynthesisError, render.EncodeError) as error:
            failures.append(name)
            print(f"FAIL {name}: {error}", file=sys.stderr)
            continue
        size_kb = path.stat().st_size / 1024
        print(f"{describe(name, clip)}  ->  {size_kb:>7.1f} KB")

    print(f"\n{len(selected) - len(failures)}/{len(selected)} clips written "
          f"to {args.out}")
    return 1 if failures else 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
