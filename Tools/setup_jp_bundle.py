#!/usr/bin/env python3
"""
Sounds/jp/ に置いた日本語音声を、ゲームの日本語用アセットバンドルに入れます。

  python3 Tools/setup_jp_bundle.py CheerReaders TapTroupe ...

- jp フォルダと各ファイルの .meta に、英語版と同じバンドル名・バリアント jp を設定します
  （取り込み設定は英語版のファイルからコピー）。
- ゲーム定義の対応言語（supportedLocales）に "jp" を足します。
- 英語版にあって日本語版にないファイルを表示します（足りないと、その声は鳴りません）。
"""
import glob
import os
import re
import sys
import uuid

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAMES = os.path.join(PROJECT, 'Assets', 'Bundled', 'Games')
SCRIPTS = os.path.join(PROJECT, 'Assets', 'Scripts', 'Games')


def guid_of(meta):
    if os.path.exists(meta):
        m = re.search(r'guid: (\w+)', open(meta).read())
        if m:
            return m.group(1)
    return uuid.uuid4().hex


def english_files(snd):
    en_dirs = [m[:-5] for m in glob.glob(snd + '/**/*.meta', recursive=True)
               if os.path.isdir(m[:-5]) and 'assetBundleVariant: en' in open(m).read()]
    out = {}
    for f in glob.glob(snd + '/**/*', recursive=True):
        if f.endswith('.meta') or os.path.isdir(f) or '/jp/' in f or not os.path.exists(f + '.meta'):
            continue
        t = open(f + '.meta').read()
        if 'assetBundleVariant: en' in t or any(f.startswith(d + '/') for d in en_dirs):
            out[os.path.splitext(os.path.basename(f))[0]] = t
    counts = {}
    for t in out.values():
        m = re.search(r'assetBundleName: (\S+)', t)
        if m:
            counts[m.group(1)] = counts.get(m.group(1), 0) + 1
    for d in en_dirs:
        m = re.search(r'assetBundleName: (\S+)', open(d + '.meta').read())
        if m:
            counts[m.group(1)] = counts.get(m.group(1), 0) + 1000
    if not counts:
        return out, set()
    # a stray file from another game's bundle shouldn't count: use the bundle most files are in
    bundle = max(counts, key=counts.get)
    out = {n: t for n, t in out.items() if re.search(r'assetBundleName: (\S+)', t) is None or bundle in t}
    return out, {bundle}


def setup(game):
    snd = os.path.join(GAMES, game, 'Sounds')
    jp = os.path.join(snd, 'jp')
    english, bundles = english_files(snd)
    if len(bundles) != 1:
        print(f'{game}: 英語版のバンドルが特定できません {bundles}')
        return
    bundle = bundles.pop()
    jp_files = {os.path.splitext(f)[0]: f for f in os.listdir(jp) if not f.endswith('.meta') and not f.startswith('.')}
    for base, meta in english.items():
        if base not in jp_files:
            continue
        path = os.path.join(jp, jp_files[base])
        t = re.sub(r'guid: \w+', 'guid: ' + guid_of(path + '.meta'), meta, count=1)
        t = re.sub(r'assetBundleName:.*', f'assetBundleName: {bundle}', t)
        t = re.sub(r'assetBundleVariant:.*', 'assetBundleVariant: jp', t)
        open(path + '.meta', 'w').write(t)
    open(jp + '.meta', 'w').write(
        f'fileFormatVersion: 2\nguid: {guid_of(jp + ".meta")}\nfolderAsset: yes\nDefaultImporter:\n'
        f'  externalObjects: {{}}\n  userData: \n  assetBundleName: {bundle}\n  assetBundleVariant: jp\n')

    # add "jp" to the game's supported locales
    name = bundle.split('/')[0]
    for path in glob.glob(os.path.join(SCRIPTS, '**', '*.cs'), recursive=True):
        src = open(path, encoding='utf-8', errors='surrogateescape').read()
        new = re.sub(r'("%s",\s*"\w+",\s*new List<string>\(\)\s*\{)([^}]*)\}' % re.escape(name),
                     lambda m: m.group(0) if '"jp"' in m.group(2) else
                     (f'{m.group(1)}{m.group(2).rstrip()}, "jp" }}' if m.group(2).strip() else f'{m.group(1)} "en", "jp" }}'), src)
        if new != src:
            open(path, 'w', encoding='utf-8', errors='surrogateescape').write(new)
    missing = sorted(set(english) - set(jp_files))
    print(f'{game}: {bundle} 日本語 {len(set(english) & set(jp_files))}/{len(english)}' + (f'  足りない: {missing}' if missing else ''))


if __name__ == '__main__':
    for game in sys.argv[1:]:
        setup(game)
