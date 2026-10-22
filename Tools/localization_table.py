#!/usr/bin/env python3
"""
ミニゲームのイベントとプロパティを、今の日本語訳と一緒に一覧にします（訳の見直し用）。

  python3 Tools/localization_table.py events  > events.tsv   # ゲーム, アクション, 英語名, 今の訳
  python3 Tools/localization_table.py params  > params.tsv   # ゲーム, アクション, プロパティ, 英語名, 今の訳, 英語の説明

ソースコードの new Minigame / new GameAction / new Param を読みます。
訳は Localization/ja の JSON から、ゲーム別のキー（ゲーム/アクション[/プロパティ]）を優先して引きます。
"""
import glob
import json
import os
import re
import sys

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
JA = os.path.join(PROJECT, 'Assets', 'Resources', 'Localization', 'ja')


def split_args(src, start):
    """arguments of the call whose '(' is at `start` → (list of raw argument strings, end index)"""
    depth, i, cur, args, in_str = 0, start, '', [], None
    while i < len(src):
        c = src[i]
        if in_str:
            cur += c
            if c == '\\':
                cur += src[i + 1]
                i += 1
            elif c == in_str:
                in_str = None
        elif c in '"\'':
            in_str = c
            cur += c
        elif c in '([{':
            depth += 1
            if depth > 1:
                cur += c
        elif c in ')]}':
            depth -= 1
            if depth == 0:
                args.append(cur.strip())
                return args, i
            cur += c
        elif c == ',' and depth == 1:
            args.append(cur.strip())
            cur = ''
        else:
            if depth >= 1:
                cur += c
        i += 1
    return args, i


def string_value(arg):
    m = re.fullmatch(r'\$?"((?:[^"\\]|\\.)*)"', arg.strip())
    return m.group(1).replace('\\"', '"').replace('\\n', '\n') if m else None


def entries():
    files = sorted(glob.glob(os.path.join(PROJECT, 'Assets', 'Scripts', 'Games', '**', '*.cs'), recursive=True))
    files.append(os.path.join(PROJECT, 'Assets', 'Scripts', 'Minigames.cs'))
    for path in files:
        src = open(path, encoding='utf-8', errors='ignore').read()
        marks = []
        for kind in ('Minigame', 'GameAction', 'Param'):
            for m in re.finditer(r'new %s\s*\(' % kind, src):
                args, _ = split_args(src, m.end() - 1)
                marks.append((m.start(), kind, args))
        marks.sort(key=lambda x: x[0])
        game = action = None
        for _, kind, args in marks:
            if kind == 'Minigame':
                game = string_value(args[0]) if args else None
            elif kind == 'GameAction' and game:
                action = (string_value(args[0]), string_value(args[1]) if len(args) > 1 else None)
            elif kind == 'Param' and game and action and len(args) >= 3:
                prop, caption = string_value(args[0]), string_value(args[2])
                tooltip = string_value(args[3]) if len(args) > 3 else ''
                if prop and caption is not None:
                    yield game, action, prop, caption, tooltip or ''


def main(kind):
    if kind == 'events':
        ev = json.load(open(os.path.join(JA, 'events.json')))
        seen = set()
        for game, (act, name), *_ in entries():
            if (game, act) in seen:
                continue
            seen.add((game, act))
            cur = ev.get(f'{game}/{act}') or ev.get(name or '') or ''
            print('\t'.join([game, act, name or '', cur]))
    else:
        pa = json.load(open(os.path.join(JA, 'params.json')))
        for game, (act, _), prop, caption, tooltip in entries():
            cur = pa.get(f'{game}/{act}/{prop}') or pa.get(caption) or ''
            print('\t'.join(s.replace('\t', ' ').replace('\n', ' ') for s in [game, act, prop, caption, cur, tooltip]))


if __name__ == '__main__':
    main(sys.argv[1] if len(sys.argv) > 1 else 'params')
