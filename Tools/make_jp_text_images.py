#!/usr/bin/env python3
"""
英語の文字だけの画像（評価・「Go for a Perfect!」など）を、同じ大きさ・同じ雰囲気の日本語版に描き直します。

  python3 Tools/make_jp_text_images.py            # すべて作る
  python3 Tools/make_jp_text_images.py superb ok  # 一部だけ

- 元の画像は JapaneseImageSources/ に保存してから上書きします（2回目以降はそこから大きさを読みます）。
- 文言は日本語版（3DS「リズム天国 ザ・ベスト＋」）の画像で確かめたもの：ハイレベル／平凡／やりなおし、
  めざせパーフェクト！、しっぱい…
- 文字は元の画像の文字があった範囲（透明でない部分）に収まるように、縦横比を保って中央に置きます。
"""
import os
import random
import shutil
import sys

from PIL import Image, ImageDraw, ImageFilter, ImageFont

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
FONTS = os.path.join(PROJECT, 'Assets', 'Resources', 'Fonts')
KUROKANE = os.path.join(FONTS, 'kurokane', 'FOT-KurokaneStd_Megamix_Modified-EB.otf')
SEURAT = os.path.join(FONTS, 'seurat', 'Seurat B.otf')
UI = 'Assets/Resources/Sprites/UI'
SOURCES = os.path.join(PROJECT, 'JapaneseImageSources')
SIZE = 400  # working font size; everything is scaled to fit afterwards


def original(path):
    """the untouched english image (saved the first time)"""
    saved = os.path.join(SOURCES, path)
    if not os.path.exists(saved):
        os.makedirs(os.path.dirname(saved), exist_ok=True)
        shutil.copy2(os.path.join(PROJECT, path), saved)
    return Image.open(saved).convert('RGBA')


def text_mask(text, font, stroke=0, shear=0.0, spacing=0):
    """white-on-black mask of the text (optionally fattened and slanted), with room around it"""
    f = ImageFont.truetype(font, SIZE)
    pad = SIZE  # same canvas for every stroke width, so layers line up
    widths = [f.getlength(c) for c in text]
    w = int(sum(widths) + spacing * (len(text) - 1)) + pad * 2
    h = SIZE * 2 + pad * 2
    img = Image.new('L', (w, h), 0)
    d = ImageDraw.Draw(img)
    x = pad
    for c, cw in zip(text, widths):
        d.text((x, pad), c, font=f, fill=255)
        x += cw + spacing
    if shear:
        img = img.transform(img.size, Image.AFFINE, (1, shear, -shear * h / 2, 0, 1, 0), resample=Image.BICUBIC)
    return dilate(img, stroke) if stroke else img


def dilate(mask, r):
    """grow the shape by about r pixels with round corners. Unlike a font stroke, this only grows the
    outside of the letters (overlapping strokes inside a glyph don't leave lines)"""
    blurred = mask.filter(ImageFilter.GaussianBlur(r * 0.55))
    # a narrow ramp keeps the edge crisp (a wide one looks like a glow)
    return blurred.point(lambda v: 0 if v < 6 else min(255, (v - 6) * 64))


def gradient(size, stops):
    """vertical gradient: stops = [(0..1, (r,g,b)), ...]"""
    w, h = size
    col = Image.new('RGBA', (1, h))
    for y in range(h):
        t = y / max(1, h - 1)
        for (t0, c0), (t1, c1) in zip(stops, stops[1:]):
            if t0 <= t <= t1:
                k = 0 if t1 == t0 else (t - t0) / (t1 - t0)
                col.putpixel((0, y), tuple(int(a + (b - a) * k) for a, b in zip(c0, c1)) + (255,))
                break
    return col.resize((w, h))


def layer(mask, fill, offset=(0, 0)):
    """a colored layer from a mask; fill is a color or an image the size of the mask.
    The color stays the same and only the alpha comes from the mask (pasting through a mask would
    mix the edges with black and leave a dark fringe)."""
    color = fill.copy() if isinstance(fill, Image.Image) else Image.new('RGBA', mask.size, fill)
    alpha = Image.new('L', mask.size, 0)
    alpha.paste(mask, offset)
    if isinstance(fill, Image.Image):
        alpha = Image.composite(alpha, Image.new('L', mask.size, 0), fill.getchannel('A'))
    color.putalpha(alpha)
    return color


def compose(layers):
    out = Image.new('RGBA', layers[0].size, (0, 0, 0, 0))
    for l in layers:
        out = Image.alpha_composite(out, l)
    return out


def fit(art, target_size, box):
    """scale art (cropped to its content) into box (l, t, r, b) of a transparent canvas"""
    art = art.crop(art.getbbox())
    bw, bh = box[2] - box[0], box[3] - box[1]
    s = min(bw / art.width, bh / art.height)
    # resize with premultiplied alpha, or the black of transparent pixels bleeds in as a gray fringe
    art = art.convert('RGBa').resize((max(1, round(art.width * s)), max(1, round(art.height * s))), Image.LANCZOS).convert('RGBA')
    canvas = Image.new('RGBA', target_size, (0, 0, 0, 0))
    canvas.alpha_composite(art, (box[0] + (bw - art.width) // 2, box[1] + (bh - art.height) // 2))
    return canvas


def hex2rgb(h):
    return tuple(int(h[i:i + 2], 16) for i in (1, 3, 5))


# ---- styles -------------------------------------------------------------------------------------

def superb(text):
    """pale pink over red, dark outline and a dark drop shadow, slanted"""
    shear = 0.2
    fill = text_mask(text, KUROKANE, 0, shear)
    outline = text_mask(text, KUROKANE, 26, shear)
    shadow = text_mask(text, KUROKANE, 40, shear)
    # the gradient follows the letters, not the whole canvas
    l, t, r, b = fill.getbbox()
    g = Image.new('RGBA', fill.size, (0, 0, 0, 0))
    g.paste(gradient((r - l, b - t), [(0, hex2rgb('#FFE0E0')), (0.47, hex2rgb('#FFE0E0')), (0.53, hex2rgb('#F20000')),
                                      (0.75, hex2rgb('#F20000')), (0.9, hex2rgb('#FF0058')), (1, hex2rgb('#FF0058'))]), (l, t))
    dark = (82, 3, 3, 255)
    return compose([layer(shadow, dark, (6, 22)), layer(outline, dark), layer(fill, g)])


def ok(text):
    """green gradient, white outline, dark green shadow down-right"""
    fill = text_mask(text, KUROKANE, 0)
    white = text_mask(text, KUROKANE, 22)
    l, t, r, b = fill.getbbox()
    g = Image.new('RGBA', fill.size, (0, 0, 0, 0))
    g.paste(gradient((r - l, b - t), [(0, hex2rgb('#86F752')), (0.2, hex2rgb('#86F752')), (0.55, hex2rgb('#18B54B')),
                                      (1, hex2rgb('#12A345'))]), (l, t))
    return compose([layer(white, (38, 88, 48, 255), (34, 30)), layer(white, (255, 255, 255, 255)), layer(fill, g)])


def try_again(text, seed):
    """bumpy blue letters with a white outline; each frame is bumpy in a different way"""
    rnd = random.Random(seed)
    base = text_mask(text, SEURAT, 6)
    blue_mask = base.copy()
    white_mask = text_mask(text, SEURAT, 30)
    d_blue, d_white = ImageDraw.Draw(blue_mask), ImageDraw.Draw(white_mask)
    # bumps along the edge of the letters
    px = base.load()
    w, h = base.size
    edge = [(x, y) for y in range(1, h - 1, 5) for x in range(1, w - 1, 5)
            if px[x, y] > 128 and (px[x - 5 if x >= 5 else 0, y] < 128 or px[min(w - 1, x + 5), y] < 128
                                   or px[x, y - 5 if y >= 5 else 0] < 128 or px[x, min(h - 1, y + 5)] < 128)]
    for x, y in rnd.sample(edge, min(len(edge), len(edge) // 7)):
        r = rnd.uniform(6, 11)
        x += rnd.uniform(-4, 4)
        y += rnd.uniform(-4, 4)
        d_blue.ellipse((x - r, y - r, x + r, y + r), fill=255)
        d_white.ellipse((x - r - 20, y - r - 20, x + r + 20, y + r + 20), fill=255)
    return compose([layer(white_mask, (255, 255, 255, 255)), layer(blue_mask, (43, 59, 214, 255))])


def outlined(text):
    """white letters with a thick black outline (Go for a Perfect!)"""
    # Kurokane's を/ね have overlapping contours that render with holes, so use Seurat here
    fill = text_mask(text, SEURAT, 6, spacing=18)
    black = text_mask(text, SEURAT, 34, spacing=18)
    return compose([layer(black, (0, 0, 0, 255)), layer(fill, (255, 255, 255, 255))])


# ---- images -------------------------------------------------------------------------------------

def make(path, art):
    eng = original(path)
    box = eng.getbbox()
    fit(art, eng.size, box).save(os.path.join(PROJECT, path))
    print(f'{path} ← {eng.size[0]}x{eng.size[1]}, 文字の範囲 {box}')


JOBS = {
    'superb': lambda: make(f'{UI}/Ratings/Superb/superbrating.png', superb('ハイレベル')),
    'ok': lambda: make(f'{UI}/Ratings/OK/okrating.png', ok('平凡')),
    'tryagain': lambda: [make(f'{UI}/Ratings/TryAgain/tryagainrating000{i}.png', try_again('やりなおし', i)) for i in (1, 2, 3)],
    'goforperfect': lambda: [make(f'{UI}/Common/GameOverlays/GoForPerfect.png', outlined('めざせパーフェクト！')),
                             make(f'{UI}/Library/libraryPerfectText.png', outlined('めざせパーフェクト！'))],
}



# ---- "You Missed..." on the perfect-fail screen ------------------------------------------------
# It is a sheet of letters (ym_Y, ym_o, ...) placed one by one in CamerasGame.prefab and revealed in
# steps ("You" → "Missed" → "..."). The japanese line (from the 3DS game) is 「しっ」+「ぱい」+「…」: each
# letter gets a new image in the same sheet, unused letters are hidden.

YOU_MISSED = f'{UI}/Common/GameOverlays/You Mised.png'
PREFAB = 'Assets/Prefabs/GameView/CamerasGame.prefab'
# (group, letter object index in the group) → japanese character; None hides the letter
YM_LETTERS = {
    'You': ['し', 'っ', None],
    'Missed...': ['ぱ', 'い', None, None, None, None, '.', '.', '.'],
}
YM_HEIGHT = 1.05   # letter height in units (the english capitals are 1.11)
YM_OVERLAP = 0.1   # neighbouring outlines overlap a little, like the english letters


def you_missed():
    import re
    import uuid
    original(YOU_MISSED)
    saved_prefab = os.path.join(SOURCES, PREFAB)
    if not os.path.exists(saved_prefab):
        os.makedirs(os.path.dirname(saved_prefab), exist_ok=True)
        shutil.copy2(os.path.join(PROJECT, PREFAB), saved_prefab)
    saved_meta = os.path.join(SOURCES, YOU_MISSED + '.meta')
    if not os.path.exists(saved_meta):
        shutil.copy2(os.path.join(PROJECT, YOU_MISSED + '.meta'), saved_meta)
    prefab = open(saved_prefab, encoding='utf-8').read()
    meta = open(saved_meta, encoding='utf-8').read()

    # one glyph per letter, all cropped to the same vertical range so small kana sit on the baseline
    chars = list(dict.fromkeys(c for group in YM_LETTERS.values() for c in group if c))
    fills = {c: text_mask(c, SEURAT, 6) for c in chars}
    blacks = {c: text_mask(c, SEURAT, 34) for c in chars}
    top = min(blacks[c].getbbox()[1] for c in chars)
    bottom = max(blacks[c].getbbox()[3] for c in chars)
    glyphs = {}
    for c in chars:
        art = compose([layer(blacks[c], (0, 0, 0, 255)), layer(fills[c], (255, 255, 255, 255))])
        l, _, r, _ = blacks[c].getbbox()
        glyphs[c] = art.crop((l, top, r, bottom))

    # pack them into the sheet (same canvas size as the english one)
    sheet_w, sheet_h = Image.open(os.path.join(SOURCES, YOU_MISSED)).size
    scale = 220 / (bottom - top)
    sheet = Image.new('RGBA', (sheet_w, sheet_h), (0, 0, 0, 0))
    rects, x, y = {}, 8, sheet_h - 8 - 220
    for c in chars:
        g = glyphs[c].convert('RGBa').resize((round(glyphs[c].width * scale), 220), Image.LANCZOS).convert('RGBA')
        if c in 'ぁぃぅぇぉっゃゅょァィゥェォッャュョ':
            # small kana: 3/4 size, sitting on the baseline
            small = g.convert('RGBa').resize((round(g.width * 0.75), round(g.height * 0.75)), Image.LANCZOS).convert('RGBA')
            g = Image.new('RGBA', (small.width, 220), (0, 0, 0, 0))
            g.alpha_composite(small, (0, 220 - small.height))
        if x + g.width > sheet_w - 8:
            x, y = 8, y - 220 - 16
        sheet.alpha_composite(g, (x, y))
        rects[c] = (x, sheet_h - y - g.height, g.width, g.height)  # unity rects start at the bottom
        x += g.width + 16
    sheet.save(os.path.join(PROJECT, YOU_MISSED))

    # sprite names: the english slots keep their names (the prefab points at them), plus one new one
    names = dict(re.findall(r'^\s+(ym_\S+): (-?\d+)$', meta.split('nameFileIdTable:')[1], re.M))
    slot_names = {'You': ['ym_Y', 'ym_o', 'ym_u'],
                  'Missed...': ['ym_M', 'ym_i', 'ym_s', 'ym_s2', 'ym_e', 'ym_d', 'ym_.', 'ym_.', 'ym_.']}
    sprite_of = {}
    for group, letters in YM_LETTERS.items():
        for n, c in zip(slot_names[group], letters):
            if c:
                sprite_of[n] = c
    new_id = str(-(uuid.uuid4().int % (2 ** 62)))
    if 'ym_s2' not in names:
        entry = re.search(r'(    - serializedVersion: 2\n      name: ym_s\n.*?weights: \[\]\n)', meta, re.S).group(1)
        entry = entry.replace('name: ym_s\n', 'name: ym_s2\n')
        entry = re.sub(r'spriteID: \w+', f'spriteID: {uuid.uuid4().hex}', entry)
        entry = re.sub(r'internalID: -?\d+', f'internalID: {new_id}', entry)
        last = re.search(r'(    - serializedVersion: 2\n      name: ym_\.\n.*?weights: \[\]\n)', meta, re.S)
        meta = meta[:last.end()] + entry + meta[last.end():]
        meta = meta.replace('    nameFileIdTable:\n', f'    nameFileIdTable:\n      ym_s2: {new_id}\n', 1)
        names['ym_s2'] = new_id
    for n, c in sprite_of.items():
        rx, ry, rw, rh = rects[c]
        meta = re.sub(r'(      name: %s\n      rect:\n        serializedVersion: 2\n)        x: \S+\n        y: \S+\n        width: \S+\n        height: \S+\n' % re.escape(n),
                      lambda m: f'{m.group(1)}        x: {rx}\n        y: {ry}\n        width: {rw}\n        height: {rh}\n', meta)
    open(os.path.join(PROJECT, YOU_MISSED + '.meta'), 'w', encoding='utf-8').write(meta)

    # lay the letters out again in the prefab
    parts = re.split(r'(?=\n--- !u!)', prefab)
    def find(kind, cond):
        return [i for i, p in enumerate(parts) if p.startswith(f'\n--- !u!{kind} ') and cond(p)]
    def field(p, name):
        return re.search(r'%s: \{fileID: (-?\d+)\}' % name, p).group(1)
    for group, letters in YM_LETTERS.items():
        go = find(1, lambda p: f'm_Name: {group}\n' in p)[0]
        go_id = re.search(r'&(-?\d+)', parts[go].split('\n')[1]).group(1)
        tr = find(224, lambda p: f'm_GameObject: {{fileID: {go_id}}}' in p)[0]
        kids = re.findall(r'- \{fileID: (-?\d+)\}', parts[tr].split('m_Children:')[1].split('m_Father')[0])
        shown = [(k, c, n) for k, c, n in zip(kids, letters, slot_names[group])]
        widths = [YM_HEIGHT * rects[c][2] / rects[c][3] if c else 0 for _, c, _ in shown]
        total = sum(w for w in widths if w) - YM_OVERLAP * (len([w for w in widths if w]) - 1)
        x = -total / 2
        for (kid, c, n), w in zip(shown, widths):
            ti = find(224, lambda p: p.startswith(f'\n--- !u!224 &{kid}\n'))[0]
            kid_go = field(parts[ti], 'm_GameObject')
            gi = find(1, lambda p: p.startswith(f'\n--- !u!1 &{kid_go}\n'))[0]
            parts[gi] = re.sub(r'm_IsActive: \d', f'm_IsActive: {1 if c else 0}', parts[gi])
            if not c:
                continue
            cx = x + w / 2
            x += w - YM_OVERLAP
            parts[ti] = re.sub(r'm_AnchoredPosition: \{x: [-\d.e]+, y: ([-\d.e]+)\}', lambda m: f'm_AnchoredPosition: {{x: {cx:.4f}, y: {m.group(1)}}}', parts[ti])
            parts[ti] = re.sub(r'm_SizeDelta: \{x: [-\d.e]+, y: [-\d.e]+\}', f'm_SizeDelta: {{x: {w:.4f}, y: {YM_HEIGHT}}}', parts[ti])
            ii = find(114, lambda p: f'm_GameObject: {{fileID: {kid_go}}}' in p and 'm_Sprite:' in p)[0]
            parts[ii] = re.sub(r'm_Sprite: \{fileID: -?\d+', f'm_Sprite: {{fileID: {names[n]}', parts[ii])
    open(os.path.join(PROJECT, PREFAB), 'w', encoding='utf-8').write(''.join(parts))
    print(f'{YOU_MISSED} と {PREFAB} ← 「しっ」「ぱい」「…」')


JOBS['youmissed'] = you_missed


if __name__ == '__main__':
    for job in sys.argv[1:] or JOBS:
        JOBS[job]()
