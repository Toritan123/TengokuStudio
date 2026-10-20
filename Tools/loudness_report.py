#!/usr/bin/env python3
"""
英語版と日本語版の声の「聞こえる大きさ」（LUFS）をゲームごとに比べます。

  python3 Tools/loudness_report.py <ゲームフォルダ...>
"""
import os
import re
import statistics
import subprocess
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from align_jp_voices import english_files, GAMES  # noqa: E402


def lufs(path):
    # short sounds: pad with silence so the 400ms measurement window fits, and resample to a rate ebur128 handles
    out = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-i', path, '-af', 'aresample=48000,apad=pad_dur=0.5,ebur128',
                          '-f', 'null', '-'], capture_output=True, text=True).stderr
    m = re.findall(r'^\s+I:\s+(-?[\d.]+) LUFS', out, re.M)
    return float(m[-1]) if m else None


def report(game):
    english = english_files(game)
    jp_dir = os.path.join(GAMES, game, 'Sounds', 'jp')
    diffs = []
    for name in sorted(os.listdir(jp_dir)):
        base, ext = os.path.splitext(name)
        if ext.lower() not in ('.wav', '.ogg', '.mp3') or base not in english:
            continue
        e, j = lufs(english[base]), lufs(os.path.join(jp_dir, name))
        if e is None or j is None or e < -60 or j < -60:
            continue
        diffs.append(j - e)
    if diffs:
        print(f'{game:18s} 日本語は英語より {statistics.median(diffs):+5.1f} LU（{len(diffs)}個の中央値、幅 {min(diffs):+.1f}〜{max(diffs):+.1f}）')


if __name__ == '__main__':
    for g in sys.argv[1:]:
        report(g)
