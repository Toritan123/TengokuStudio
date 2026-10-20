#!/usr/bin/env python3
"""
声のサンプルが何という言葉かを、読み上げ音声（macOS の say）との似かたで推測します。
話し手が違うので確実ではありません。候補を絞るための目安です。

  Tools/.venv/bin/python Tools/word_match.py <読み上げ音声...> -- <サンプル...>

読み上げ音声ごとに、似ているサンプルを上から5つ出します（MFCC の DTW 距離。小さいほど似ている）。
"""
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from voice_match import load, RATE  # noqa: E402


def mel_filters(n_fft, n_mels=26):
    def hz2mel(h):
        return 2595 * np.log10(1 + h / 700)
    mels = np.linspace(hz2mel(80), hz2mel(6000), n_mels + 2)
    hz = 700 * (10 ** (mels / 2595) - 1)
    bins = np.floor((n_fft + 1) * hz / RATE).astype(int)
    fb = np.zeros((n_mels, n_fft // 2 + 1))
    for m in range(1, n_mels + 1):
        a, b, c = bins[m - 1], bins[m], bins[m + 1]
        fb[m - 1, a:b] = (np.arange(a, b) - a) / max(1, b - a)
        fb[m - 1, b:c] = (c - np.arange(b, c)) / max(1, c - b)
    return fb


N_FFT, HOP = 512, 220
FB = mel_filters(N_FFT)
DCT = np.cos(np.pi / 26 * (np.arange(26)[None, :] + 0.5) * np.arange(1, 13)[:, None])


def mfcc(x):
    # trim silence
    env = np.abs(x)
    on = np.where(env > env.max() * 0.05)[0]
    if len(on) == 0:
        return None
    x = x[on[0]:on[-1] + 1]
    if len(x) < N_FFT:
        x = np.pad(x, (0, N_FFT - len(x)))
    frames = np.lib.stride_tricks.sliding_window_view(x, N_FFT)[::HOP] * np.hanning(N_FFT)
    spec = np.abs(np.fft.rfft(frames)) ** 2
    m = np.log(spec @ FB.T + 1e-8) @ DCT.T
    m -= m.mean(0)  # cepstral mean normalisation (removes speaker / channel colour a bit)
    return m / (m.std(0) + 1e-6)


def dtw(a, b):
    d = np.sqrt(((a[:, None] - b[None]) ** 2).sum(-1))
    n, m = d.shape
    if max(n, m) > 2.5 * min(n, m):
        return np.inf
    acc = np.full((n + 1, m + 1), np.inf)
    acc[0, 0] = 0
    for i in range(1, n + 1):
        for j in range(1, m + 1):
            acc[i, j] = d[i - 1, j - 1] + min(acc[i - 1, j], acc[i, j - 1], acc[i - 1, j - 1])
    return acc[n, m] / (n + m)


def main(args):
    k = args.index('--')
    refs, cands = args[:k], args[k + 1:]
    cm = {c: mfcc(load(c)) for c in cands}
    for r in refs:
        rm = mfcc(load(r))
        scores = sorted((dtw(rm, f), c) for c, f in cm.items() if f is not None)
        print(f'{os.path.basename(r):20}', '  '.join(f'{os.path.basename(c)[:7]} {s:.2f}' for s, c in scores[:5]))


if __name__ == '__main__':
    main(sys.argv[1:])
