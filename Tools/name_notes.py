#!/usr/bin/env python3
"""
エディタの項目名（イベント・プロパティ・選択肢）を自分で決めるための、この Mac だけで動くページです。

  python3 Tools/name_notes.py        # ブラウザで http://127.0.0.1:8770 が開きます。終わるときは Ctrl+C

- 入力した名前は Tools/name_notes_edits.json に保存されます。
- ページの「アプリに反映」で Assets/Resources/Localization/ja の JSON に書き込みます。アプリに出すにはビルドが必要です。
"""
import http.server
import json
import os
import re
import subprocess
import sys
import webbrowser

HERE = os.path.dirname(os.path.abspath(__file__))
JA = os.path.join(os.path.dirname(HERE), 'Assets', 'Resources', 'Localization', 'ja')
EDITS = os.path.join(HERE, 'name_notes_edits.json')
PORT = 8770
ORIGIN = f'http://127.0.0.1:{PORT}'


def load(name):
    return json.load(open(os.path.join(JA, name), encoding='utf-8'))


def dump(name, data):
    with open(os.path.join(JA, name), 'w', encoding='utf-8') as f:
        f.write(json.dumps(data, ensure_ascii=False, indent=1) + '\n')


def table(kind):
    out = subprocess.run([sys.executable, os.path.join(HERE, 'localization_table.py'), kind], capture_output=True, text=True, check=True).stdout
    return [line.split('\t') for line in out.splitlines()]


def read_edits():
    return json.load(open(EDITS, encoding='utf-8')) if os.path.exists(EDITS) else {}


def write_edits(edits):
    with open(EDITS, 'w', encoding='utf-8') as f:
        json.dump(edits, f, ensure_ascii=False, indent=1)


def official_keys():
    """the event names checked against the official games (marked 公式 on the page)"""
    path = os.path.join(HERE, 'name_notes_official.txt')
    return set(open(path, encoding='utf-8').read().split('\n')) if os.path.exists(path) else set()


def page_data():
    names, tips, enums, official = load('games.json'), load('tooltips.json'), load('enums.json'), official_keys()
    games = {}
    for g, a, en, ja in table('events'):
        info = names.get(g, {})
        games.setdefault(g, {'id': g, 'name': info.get('name', g), 'sort': info.get('sort', g), 'events': []})
        games[g]['events'].append({'a': a, 'en': en, 'ja': ja, 'off': f'{g}/{a}' in official, 'params': []})
    for g, a, p, en, ja, tip in table('params'):
        for e in games.get(g, {'events': []})['events']:
            if e['a'] == a:
                e['params'].append({'p': p, 'en': en, 'ja': ja, 'tip': tips.get(tip, tip)})
                break
    return {'games': sorted(games.values(), key=lambda x: x['sort']), 'edits': read_edits(),
            'enums': [{'k': k, 'ja': v} for k, v in enums.items() if k != '//']}


def apply_edits():
    """write the saved names into the localization files; a name set for one item wins over a shared word"""
    edits = list(read_edits().values())
    events, params, enums = load('events.json'), load('params.json'), load('enums.json')
    for e in edits:
        if e['kind'] != 'common':
            continue
        kind = e['key'].split(':', 1)[0]
        for row in table('events' if kind == 'event' else 'params'):
            if kind == 'event' and row[2] == e['en']:
                events[f'{row[0]}/{row[1]}'] = e['value']
            elif kind == 'param' and row[3] == e['en']:
                params[f'{row[0]}/{row[1]}/{row[2]}'] = e['value']
    for e in edits:
        target = {'event': events, 'param': params, 'enum': enums}.get(e['kind'])
        if target is not None:
            target[e['key']] = e['value']
    dump('events.json', events), dump('params.json', params), dump('enums.json', enums)
    write_edits({})
    return len(edits)


class Handler(http.server.BaseHTTPRequestHandler):
    def send(self, code, body=b'{}', ctype='application/json'):
        self.send_response(code)
        self.send_header('Content-Type', ctype)
        self.send_header('Content-Length', str(len(body)))
        self.end_headers()
        self.wfile.write(body)

    def do_GET(self):
        if self.path != '/':
            return self.send(404)
        text = json.dumps(page_data(), ensure_ascii=False, separators=(',', ':')).replace('</', '<\\/')
        page = open(os.path.join(HERE, 'name_notes.html'), encoding='utf-8').read().replace('/*DATA*/', text)
        self.send(200, page.encode('utf-8'), 'text/html; charset=utf-8')

    def do_POST(self):
        if self.headers.get('Origin') != ORIGIN:  # only this page may write
            return self.send(403)
        body = json.loads(self.rfile.read(int(self.headers.get('Content-Length', 0))) or '{}')
        if self.path == '/api/edit':
            edits = read_edits()
            if body.get('entry'):
                edits[body['id']] = body['entry']
            else:
                edits.pop(body['id'], None)
            write_edits(edits)
            return self.send(200)
        if self.path == '/api/apply':
            n = apply_edits()
            print(f'{n}件を反映しました。アプリに出すにはビルドしてください。')
            return self.send(200)
        self.send(404)

    def log_message(self, *args):
        pass


if __name__ == '__main__':
    server = http.server.ThreadingHTTPServer(('127.0.0.1', PORT), Handler)
    print(f'{ORIGIN} を開きます。終わるときは Ctrl+C')
    if '--no-browser' not in sys.argv:
        webbrowser.open(ORIGIN)
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        pass
