#!/usr/bin/env python3
"""
日本語音声（Sounds/jp/）の音量と頭の無音を、同じ名前の英語版に合わせます。

  python3 Tools/align_jp_voices.py KarateMan AirRally ...
  python3 Tools/align_jp_voices.py --no-align Splashdown   # 頭の位置は変えない
  python3 Tools/align_jp_voices.py --per-file KarateMan    # 1ファイルずつ英語版の音量に合わせる

- ゲームのコードは英語版の頭の無音を前提にタイミングを組んでいるので、
  声の立ち上がり位置を英語版と同じにします（無音を足す／削る）。
- 音量は「聞こえる大きさ」（LUFS）で比べ、日本語版の声どうしの音量バランス（例：カラテ家の「さん」と「Hit」）を
  保ったまま、ゲーム全体で1つの量だけ上げ下げして英語版に近づけます。音割れしそうなところはリミッターで抑えます。
  英語版と日本語版では声ごとのバランスが違うので、1ファイルずつ合わせると不自然になります。
- 元のファイルは JapaneseVoiceSources/<ゲーム>/ に保存し、毎回そこから作り直します。
  何度実行しても結果は同じです。
- 出力は .wav です（拡張子が変わっても .meta はそのまま引き継ぎます）。
"""
import glob
import math
import os
import re
import shutil
import struct
import subprocess
import sys

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAMES = os.path.join(PROJECT, 'Assets', 'Bundled', 'Games')
SOURCES = os.path.join(PROJECT, 'JapaneseVoiceSources')
RATE = 44100
WINDOW = 220            # 5ms
ONSET_DB = -30          # 立ち上がり判定：ファイルのピークから -30dB
ALIGN_MIN = 0.008       # これより小さいずれは直さない
MAX_GAIN = 12.0
LIMIT_PUSH = 5.0  # dB the limiter may squash to keep short voices as loud as the english ones
PEAK_LIMIT = -0.3


def decode(path):
    data = subprocess.run(['ffmpeg', '-v', 'error', '-i', path, '-ac', '1', '-ar', str(RATE), '-f', 's16le', '-'],
                          capture_output=True, check=True).stdout
    return struct.unpack('<%dh' % (len(data) // 2), data)


def analyze(path):
    s = decode(path)
    peak = max((abs(x) for x in s), default=0) or 1
    thr = peak * 10 ** (ONSET_DB / 20)
    onset, active = None, []
    for i in range(0, len(s), WINDOW):
        seg = s[i:i + WINDOW]
        if math.sqrt(sum(x * x for x in seg) / len(seg)) >= thr:
            if onset is None:
                onset = i / RATE
            active.extend(seg)
    loud = math.sqrt(sum(x * x for x in active) / len(active)) if active else 1
    db = lambda v: 20 * math.log10(max(v, 1) / 32768)
    return {'onset': onset or 0.0, 'loud': lufs(path) if peak > 1 else db(loud), 'peak': db(peak), 'silent': peak <= 1}


def lufs(path):
    """perceived loudness (EBU R128). Short sounds are padded so the 400ms window fits."""
    out = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-i', path, '-af', 'aresample=48000,apad=pad_dur=0.5,ebur128',
                          '-f', 'null', '-'], capture_output=True, text=True).stderr
    m = re.findall(r'^\s+I:\s+(-?[\d.]+) LUFS', out, re.M)
    return float(m[-1]) if m else -70.0


def english_files(game):
    snd = os.path.join(GAMES, game, 'Sounds')
    en_dirs = [m[:-5] for m in glob.glob(snd + '/**/*.meta', recursive=True)
               if os.path.isdir(m[:-5]) and 'assetBundleVariant: en' in open(m, errors='ignore').read()]
    out = {}
    for f in glob.glob(snd + '/**/*', recursive=True):
        if f.endswith('.meta') or os.path.isdir(f) or '/jp/' in f or not os.path.exists(f + '.meta'):
            continue
        if 'assetBundleVariant: en' in open(f + '.meta', errors='ignore').read() or any(f.startswith(d + '/') for d in en_dirs):
            out[os.path.splitext(os.path.basename(f))[0]] = f
    return out


def process(game, align=True, per_file=False):
    jp_dir = os.path.join(GAMES, game, 'Sounds', 'jp')
    src_dir = os.path.join(SOURCES, game)
    os.makedirs(src_dir, exist_ok=True)
    english = english_files(game)
    print(f'== {game}')
    items = []
    for name in sorted(os.listdir(jp_dir)):
        base, ext = os.path.splitext(name)
        if ext.lower() not in ('.ogg', '.wav', '.mp3') or base not in english:
            continue
        current = os.path.join(jp_dir, name)
        # keep the untouched original, and always rebuild from it
        originals = glob.glob(os.path.join(src_dir, base + '.*'))
        if not originals:
            shutil.copy2(current, os.path.join(src_dir, name))
            originals = [os.path.join(src_dir, name)]
        source = originals[0]
        en, jp = analyze(english[base]), analyze(source)
        if en['silent'] or jp['silent']:
            print(f'  {base:28s} 無音のファイルなので変更なし')
            continue
        items.append((base, current, source, en, jp))

    # one gain for the whole game keeps the japanese balance between voices
    diffs = sorted(en['loud'] - jp['loud'] for _, _, _, en, jp in items)
    game_gain = (diffs[(len(diffs) - 1) // 2] + diffs[len(diffs) // 2]) / 2 if diffs else 0.0
    if not per_file:
        print(f'  ゲーム全体の音量 {game_gain:+.1f}dB')

    for base, current, source, en, jp in items:
        gain = en['loud'] - jp['loud'] if per_file else game_gain
        gain = max(-MAX_GAIN, min(MAX_GAIN, gain))
        shift = en['onset'] - jp['onset'] if align else 0.0
        if abs(shift) < ALIGN_MIN:
            shift = 0.0

        filters = []
        if shift > 0:
            filters.append(f'adelay={shift * 1000:.1f}:all=1')
        elif shift < 0:
            filters.append(f'atrim=start={-shift:.4f},asetpts=PTS-STARTPTS,afade=t=in:d=0.003')
        target = os.path.join(jp_dir, base + '.wav')
        tmp = target + '.tmp.wav'
        wanted, extra = jp['loud'] + gain, 0.0
        while True:
            level = [f'volume={gain + extra:.2f}dB']
            if jp['peak'] + gain + extra > PEAK_LIMIT:
                # keep the loudness and catch the peaks instead of turning the whole voice down
                level.append(f'alimiter=limit={10 ** (PEAK_LIMIT / 20):.3f}:attack=1:release=30:latency=1')
            subprocess.run(['ffmpeg', '-v', 'error', '-y', '-i', source, '-af', ','.join(filters + level),
                            '-ar', str(RATE), tmp], check=True)
            # short, peaky voices lose loudness in the limiter: push a little more, but squash at most LIMIT_PUSH dB
            lost = wanted - lufs(tmp)
            if len(level) == 1 or lost < 0.3 or extra >= LIMIT_PUSH:
                break
            extra = min(LIMIT_PUSH, extra + lost)
        gain += extra
        if current != target:
            os.remove(current)
            if os.path.exists(current + '.meta') and not os.path.exists(target + '.meta'):
                os.rename(current + '.meta', target + '.meta')
        os.replace(tmp, target)
        print(f'  {base:28s} 音量 {gain:+5.1f}dB  頭の位置 {shift * 1000:+5.0f}ms')

if __name__ == '__main__':
    args = sys.argv[1:]
    align = '--no-align' not in args
    per_file = '--per-file' in args
    for game in [a for a in args if not a.startswith('--')]:
        process(game, align, per_file)
