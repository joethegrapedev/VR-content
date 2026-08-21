#!/usr/bin/env python3
"""Concatenate the generated one-shots into a single audition file."""
from __future__ import annotations

import sys
from pathlib import Path

import numpy as np

import render
import verify
from dsp import SAMPLE_RATE

GAP_SECONDS = 0.45


def main(source: Path, out_path: Path) -> int:
    clips = sorted(p for p in source.glob("SFX_*.wav"))
    if not clips:
        print(f"no SFX clips in {source}", file=sys.stderr)
        return 1

    gap = np.zeros(int(GAP_SECONDS * SAMPLE_RATE))
    pieces = []
    for path in clips:
        data, _ = verify.read_wav(path)
        mono = data.mean(axis=1) if data.ndim == 2 else data
        # Looping beds are long; take a representative few seconds only.
        pieces.extend([mono[: 3 * SAMPLE_RATE], gap])
        print(f"  {path.stem}")

    render.write_wav(np.concatenate(pieces), out_path)
    print(f"\n{len(clips)} clips -> {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main(Path(sys.argv[1]), Path(sys.argv[2])))
