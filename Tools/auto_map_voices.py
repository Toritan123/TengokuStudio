#!/usr/bin/env python3
"""
共通バンドルにある英語の声を探し、RHRE3 の日本語版との対応表（import_rhre_voices.py の MAPPINGS 用）を作ります。

  Tools/.venv/bin/python Tools/auto_map_voices.py <ゲームフォルダ> <RHRE名> [英語名=日本語名の置き換え ...]

英語版と日本語版で中身が違う音（＝声）だけを出します。置き換えは正規表現で、名前の違う声に使います
（例：hereWeGo=dokkoisho）。
"""
import glob
import json
import os
import re
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from voice_match import load, compare  # noqa: E402

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RHRE = os.path.expanduser('~/.rhre3/sfx/master/games')


def files(game):
    return {os.path.splitext(os.path.basename(f))[0]: f for f in glob.glob(os.path.join(RHRE, game, '**', '*.ogg'), recursive=True)}


def main(hs_game, rhre_game, *subs):
    subs = [s.split('=', 1) for s in subs]
    en, ja = files(rhre_game + 'En'), files(rhre_game + 'Ja')
    en_audio = {n: load(p) for n, p in en.items()}
    snd = os.path.join(PROJECT, 'Assets', 'Bundled', 'Games', hs_game, 'Sounds')
    mapping, notes = {}, []
    for f in sorted(glob.glob(snd + '/**/*', recursive=True)):
        if f.endswith('.meta') or os.path.isdir(f) or '/jp/' in f or not f.lower().endswith(('.wav', '.ogg', '.mp3')):
            continue
        if not os.path.exists(f + '.meta') or 'assetBundleVariant: en' in open(f + '.meta').read():
            continue
        a = load(f)
        (m, t, g), name = max(((compare(a, b), n) for n, b in en_audio.items()), key=lambda x: x[0][0])
        if m < 0.6:
            continue
        ja_name = name
        for pat, rep in subs:
            ja_name = re.sub(pat, rep, ja_name)
        if ja_name not in ja:
            notes.append(f'{os.path.basename(f)}: {name} → 日本語版に {ja_name} なし')
            continue
        if ja_name == name and compare(en_audio[name], load(ja[name]))[0] > 0.9:
            continue  # same sound in both languages: not a voice
        base = os.path.splitext(os.path.basename(f))[0]
        mapping[base] = ja_name
        if t > 0.01:
            notes.append(f'{base}: 英語版は {name} の {t:.3f}秒目から')
    print(json.dumps(mapping, ensure_ascii=False, indent=1))
    for n in notes:
        print('#', n)


if __name__ == '__main__':
    main(*sys.argv[1:])
