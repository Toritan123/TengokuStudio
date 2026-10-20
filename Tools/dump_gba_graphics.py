#!/usr/bin/env python3
"""
GBA（リズム天国）の逆アセンブル版（RhythmTengokuPlus など）の graphics フォルダから、
スプライト（部品を組み立てた絵）を PNG に書き出します。日本語版の文字を確かめる資料用です。

  python3 Tools/dump_gba_graphics.py <games/<ゲーム>/graphics> <出力フォルダ>

<名前>_obj.4bpp（絵）、<名前>_pal.c（色）、<名前>_anim_cells.inc.c（組み立て方）を使います。
"""
import glob
import os
import re
import sys

from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from dump_ds_graphics import OBJ_SIZES, sheet  # noqa: E402


def palettes(path):
    src = open(path).read()
    obj = src.split('obj_pal')[1] if 'obj_pal' in src else src
    cols = [int(h, 16) for h in re.findall(r'TO_RGB555\(0x([0-9A-Fa-f]{6})\)', obj)]
    return [((c >> 16) & 0xF8, (c >> 8) & 0xF8, c & 0xF8) for c in cols]


def cels(path):
    out = []
    for body in re.findall(r'AnimationCel \w+\[\] = \{(.*?)\};', open(path).read(), re.S):
        nums = [int(x, 16) for x in re.findall(r'0x([0-9a-fA-F]+)', body)]
        objs = []
        for i in range(0, len(nums) - 2, 3):
            a0, a1, a2 = nums[i:i + 3]
            y = a0 & 0xFF
            y = y - 256 if y >= 128 else y
            x = a1 & 0x1FF
            x = x - 512 if x >= 256 else x
            shape, size = (a0 >> 14) & 3, (a1 >> 14) & 3
            if shape == 3:
                continue
            w, h = OBJ_SIZES[shape][size]
            objs.append((x, y, w, h, a2 & 0x3FF, (a2 >> 12) & 15, bool(a1 & 0x1000), bool(a1 & 0x2000)))
        out.append(objs)
    return out


def tiles(path):
    data = open(path, 'rb').read()
    return [v for b in data for v in (b & 15, b >> 4)]


def render(values, cel_list, colors, base):
    images = []
    for objs in cel_list:
        if not objs:
            continue
        x0 = min(o[0] for o in objs); y0 = min(o[1] for o in objs)
        x1 = max(o[0] + o[2] for o in objs); y1 = max(o[1] + o[3] for o in objs)
        img = Image.new('RGBA', (x1 - x0, y1 - y0), (0, 0, 0, 0))
        for x, y, w, h, tile, pal, hf, vf in reversed(objs):
            part = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            px = part.load()
            for t in range((w // 8) * (h // 8)):
                tx, ty = (t % (w // 8)) * 8, (t // (w // 8)) * 8
                # 2D mapping: the sprite's tiles are a block of a 32-tile-wide sheet
                start = (tile - base + (t // (w // 8)) * 32 + t % (w // 8)) * 64
                for i in range(64):
                    if 0 <= start + i < len(values) and values[start + i]:
                        idx = pal * 16 + values[start + i]
                        if idx < len(colors):
                            px[tx + i % 8, ty + i // 8] = colors[idx] + (255,)
            if hf:
                part = part.transpose(Image.FLIP_LEFT_RIGHT)
            if vf:
                part = part.transpose(Image.FLIP_TOP_BOTTOM)
            img.alpha_composite(part, (x - x0, y - y0))
        images.append(img)
    return images


def main(folder, out):
    for cells_path in glob.glob(os.path.join(folder, '*_anim_cells.inc.c')):
        stem = os.path.basename(cells_path)[:-len('_anim_cells.inc.c')]
        obj, pal = os.path.join(folder, stem + '_obj.4bpp'), os.path.join(folder, stem + '_pal.c')
        if not (os.path.exists(obj) and os.path.exists(pal)):
            continue
        cl = cels(cells_path)
        used = [o[4] for objs in cl for o in objs]
        values, colors = tiles(obj), palettes(pal)
        base = 0
        os.makedirs(out, exist_ok=True)
        sheet(render(values, cl, colors, base)).save(os.path.join(out, stem + '_cells.png'))
        print(stem, len(cl), 'cels, base', hex(base))


if __name__ == '__main__':
    main(*sys.argv[1:])
