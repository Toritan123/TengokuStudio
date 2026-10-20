#!/usr/bin/env python3
"""
共通バンドルに入っている音のうち、言語によって中身が違うもの（＝声）を探します。

  Tools/.venv/bin/python Tools/find_voices.py <Heaven Studio のゲームフォルダ> <RHRE のゲーム名（En/Ja を除く）>

Heaven Studio の各ファイルを RHRE の英語版と照合し、同じ名前の日本語版が英語版と違う音なら「声」とします。
"""
import glob
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from voice_match import load, compare  # noqa: E402

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
RHRE = os.path.expanduser('~/.rhre3/sfx/master/games')


def rhre_files(game):
    return {os.path.splitext(os.path.basename(f))[0]: f for f in glob.glob(os.path.join(RHRE, game, '**', '*.ogg'), recursive=True)}


def main(hs_game, rhre_game, en_suffix='En', ja_suffix='Ja'):
    en, ja = rhre_files(rhre_game + en_suffix), rhre_files(rhre_game + ja_suffix)
    en_audio = {n: load(p) for n, p in en.items()}
    snd = os.path.join(PROJECT, 'Assets', 'Bundled', 'Games', hs_game, 'Sounds')
    for f in sorted(glob.glob(snd + '/**/*', recursive=True)):
        if f.endswith('.meta') or os.path.isdir(f) or '/jp/' in f or not f.lower().endswith(('.wav', '.ogg', '.mp3')):
            continue
        a = load(f)
        (m, t, g), name = max(((compare(a, b), n) for n, b in en_audio.items()), key=lambda x: x[0][0])
        rel = os.path.relpath(f, snd)
        if m < 0.6:
            print(f'  {rel:40s} RHRE 英語版に見当たらない（最も近い {name} {m:.2f}）')
            continue
        if name not in ja:
            print(f'  {rel:40s} = {name} ({m:.2f}) 日本語版に同名なし')
            continue
        same, _, _ = compare(en_audio[name], load(ja[name]))
        kind = '共通の音' if same > 0.9 else '★声（言語で違う）'
        print(f'  {rel:40s} = {name} ({m:.2f} @{t:.3f}s)  {kind} 英日一致 {same:.2f}')


if __name__ == '__main__':
    main(*sys.argv[1:])
