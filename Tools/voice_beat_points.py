"""
日本語の声と英語の声の「拍に聞こえる点」を比べ、ずれを一覧にします（voice_timing.json の offset を決める材料）。

  Tools/.venv/bin/python Tools/voice_beat_points.py   # 結果は /Volumes/SSD-SCTU3A/TengokuStudio/work/pcenter.json

Where each voice 'lands' on the beat (perceptual center): the moment the first syllable rises to half of its peak.
Compare japanese and english clips of the same name; the japanese one needs to be played early by the difference."""
import glob, json, os, re, subprocess, sys
import numpy as np
sys.path.insert(0, '/Users/sumirintarou/HeavenStudio-master/Tools')
from align_jp_voices import english_files, GAMES
RATE = 44100

def env(path):
    raw = subprocess.run(['ffmpeg', '-v', 'error', '-i', path, '-ac', '1', '-ar', str(RATE), '-f', 's16le', '-'], capture_output=True, check=True).stdout
    x = np.frombuffer(raw, np.int16).astype(np.float32) / 32768
    win, hop = int(RATE * 0.005), int(RATE * 0.001)
    if len(x) < win: return None
    frames = np.lib.stride_tricks.sliding_window_view(x, win)[::hop]
    return np.sqrt((frames ** 2).mean(axis=1))  # one value per ms

def pcenter(path):
    e = env(path)
    if e is None or e.max() < 1e-4: return None
    onset = int(np.argmax(e >= e.max() * 10 ** (-30 / 20)))
    first = e[onset:onset + 250]
    m = first.max()
    return (onset + int(np.argmax(first >= 0.5 * m))) / 1000, onset / 1000

def game_id(folder):
    for p in glob.glob(f'/Users/sumirintarou/HeavenStudio-master/Assets/Scripts/Games/{folder}/*.cs'):
        m = re.search(r'new Minigame\(\s*"([^"]+)"', open(p, encoding='utf-8', errors='ignore').read())
        if m: return m.group(1)
    return None

rows = []
for folder in sorted(os.listdir(GAMES)):
    jp = os.path.join(GAMES, folder, 'Sounds', 'jp')
    if not os.path.isdir(jp): continue
    gid = game_id(folder); en = english_files(folder)
    for name in sorted(os.listdir(jp)):
        base, ext = os.path.splitext(name)
        if ext.lower() not in ('.wav', '.ogg', '.mp3') or base not in en: continue
        a, b = pcenter(en[base]), pcenter(os.path.join(jp, name))
        if not a or not b: continue
        rows.append({'folder': folder, 'game': gid, 'sound': base, 'en_pc': a[0], 'en_on': a[1], 'jp_pc': b[0], 'jp_on': b[1], 'diff': round(b[0] - a[0], 3)})
json.dump(rows, open('/Volumes/SSD-SCTU3A/TengokuStudio/work/pcenter.json', 'w'), indent=1)
d = np.array([r['diff'] for r in rows])
print(len(rows), 'voices; median', np.median(d), 'ms-ish dist:', np.percentile(d, [5, 25, 50, 75, 95]))
for folder in sorted({r['folder'] for r in rows}):
    g = [r['diff'] for r in rows if r['folder'] == folder]
    print(f'{folder:18s} n={len(g):3d} median {np.median(g)*1000:+5.0f}ms  min {min(g)*1000:+5.0f}  max {max(g)*1000:+5.0f}')
