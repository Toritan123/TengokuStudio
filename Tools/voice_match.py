#!/usr/bin/env python3
"""
音声ファイルどうしを波形で照合します（どのファイルがどれに当たるかを探す）。

  Tools/.venv/bin/python Tools/voice_match.py <探したいファイル...> -- <候補ファイル...>

探したいファイルごとに、波形がいちばん似ている候補を2つ表示します。
一致度（0〜1）、候補の中で見つかった位置（秒）、音量差を出します。
"""
import os
import subprocess
import sys

import numpy as np

RATE = 22050


def load(path):
    data = subprocess.run(['ffmpeg', '-v', 'error', '-i', path, '-ac', '1', '-ar', str(RATE), '-f', 'f32le', '-'],
                          capture_output=True).stdout
    return np.frombuffer(data, dtype=np.float32).copy()


def compare(a, b):
    """(match 0..1, where the shorter one sits in the longer one in seconds, dB louder the candidate is)"""
    short, long_ = (a, b) if len(a) <= len(b) else (b, a)
    if len(short) < 10:
        return 0.0, 0.0, 0.0
    n = 1 << int(np.ceil(np.log2(len(short) + len(long_))))
    c = np.fft.irfft(np.fft.rfft(long_, n) * np.conj(np.fft.rfft(short, n)), n)[:len(long_) - len(short) + 1]
    k = int(np.argmax(np.abs(c)))
    seg = long_[k:k + len(short)]
    match = abs(c[k]) / (np.linalg.norm(short) * np.linalg.norm(seg) + 1e-9)
    gain = 20 * np.log10((np.linalg.norm(seg) + 1e-9) / (np.linalg.norm(short) + 1e-9))
    return float(match), k / RATE, float(gain if len(a) <= len(b) else -gain)


if __name__ == '__main__':
    args = sys.argv[1:]
    split = args.index('--')
    queries, pool = args[:split], args[split + 1:]
    candidates = {p: load(p) for p in pool}
    for q in queries:
        a = load(q)
        ranked = sorted(((compare(a, b), p) for p, b in candidates.items()), key=lambda x: -x[0][0])[:2]
        best = '  '.join(f'{os.path.basename(p)} 一致{m:.2f} 位置{t:.3f}s 音量{g:+.1f}dB' for (m, t, g), p in ranked)
        print(f'{os.path.basename(q):32s} {len(a) / RATE:.2f}s -> {best}')
