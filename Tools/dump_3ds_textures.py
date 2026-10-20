#!/usr/bin/env python3
"""
3DS（リズム天国 ザ・ベスト＋／Megamix）の ROM から、ミニゲームのテクスチャを PNG に書き出します。
日本語版の文字やデザインを確かめる資料用です。

  Tools/.venv/bin/python Tools/dump_3ds_textures.py <ROM.3ds> <出力フォルダ> <ミニゲーム名の一部 ...>
  Tools/.venv/bin/python Tools/dump_3ds_textures.py --layout <ROM.3ds> <出力フォルダ> <layout の名前の一部 ...>   # 共通画面（評価など）
  例: ... rvlSort rvlInterview   （名前は cellanim/ の .zlib ファイル名。dump_3ds_romfs.py で一覧が見られます）

cellanim/<名前>.zlib（サイズ4バイト＋zlib）→ SARC → .ctpk（テクスチャ）を展開して、1枚ずつ PNG にします。
部品の組み立て（.bccad）はしないので、テクスチャの全体（アトラス）がそのまま出ます。numpy が必要です。
"""
import os
import struct
import sys
import zlib

import numpy as np
from PIL import Image

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from dump_3ds_romfs import main as romfs_main  # noqa: E402

ETC_MOD = np.array([[2, 8, -2, -8], [5, 17, -5, -17], [9, 29, -9, -29], [13, 42, -13, -42],
                    [18, 60, -18, -60], [24, 80, -24, -80], [33, 106, -33, -106], [47, 183, -47, -183]])


def sarc_files(data):
    hl, _, _, data_off = struct.unpack_from('<HHII', data, 4)
    count = struct.unpack_from('<H', data, hl + 6)[0]
    names = hl + 12 + count * 16 + 8
    out = {}
    for i in range(count):
        _, attr, start, end = struct.unpack_from('<IIII', data, hl + 12 + i * 16)
        n = names + (attr & 0xFFFF) * 4
        out[data[n:data.index(b'\0', n)].decode()] = data[data_off + start:data_off + end]
    return out


def morton(x, y):
    """position of pixel (x, y) inside an 8x8 tile"""
    m = 0
    for i in range(3):
        m |= ((x >> i) & 1) << (2 * i) | ((y >> i) & 1) << (2 * i + 1)
    return m


MORTON = [[morton(x, y) for x in range(8)] for y in range(8)]


def etc1_block(color, alpha=None):
    """decode one 4x4 ETC1 block (64-bit value) → 4x4x4 RGBA"""
    diff = (color >> 33) & 1
    flip = (color >> 32) & 1
    t1, t2 = (color >> 37) & 7, (color >> 34) & 7
    if diff:
        def c5(shift, dshift):
            b = (color >> shift) & 31
            d = (color >> dshift) & 7
            d = d - 8 if d >= 4 else d
            b2 = max(0, min(31, b + d))
            return (b << 3 | b >> 2), (b2 << 3 | b2 >> 2)
        (r1, r2), (g1, g2), (bb1, bb2) = c5(59, 56), c5(51, 48), c5(43, 40)
    else:
        def c4(shift):
            v = (color >> shift) & 15
            return v * 17
        r1, r2, g1, g2, bb1, bb2 = c4(60), c4(56), c4(52), c4(48), c4(44), c4(40)
    out = np.zeros((4, 4, 4), np.uint8)
    for x in range(4):
        for y in range(4):
            i = x * 4 + y
            msb, lsb = (color >> (16 + i)) & 1, (color >> i) & 1
            first = (y < 2) if flip else (x < 2)
            base = (r1, g1, bb1) if first else (r2, g2, bb2)
            # ETC1 index (msb, lsb): 0 → +small, 1 → +large, 2 → -small, 3 → -large
            mod = ETC_MOD[t1 if first else t2][(msb << 1) | lsb]
            a = 255 if alpha is None else ((alpha >> (i * 4)) & 15) * 17
            out[y, x] = [max(0, min(255, c + mod)) for c in base] + [a]
    return out


def decode(fmt, w, h, data):
    img = np.zeros((h, w, 4), np.uint8)
    if fmt in (12, 13):
        pos = 0
        for ty in range(0, h, 8):
            for tx in range(0, w, 8):
                for by, bx in ((0, 0), (0, 4), (4, 0), (4, 4)):
                    alpha = None
                    if fmt == 13:
                        alpha = struct.unpack_from('<Q', data, pos)[0]
                        pos += 8
                    color = struct.unpack_from('<Q', data, pos)[0]
                    pos += 8
                    img[ty + by:ty + by + 4, tx + bx:tx + bx + 4] = etc1_block(color, alpha)
        return img
    bpp = {0: 4, 1: 3, 2: 2, 3: 2, 4: 2, 5: 2, 7: 1, 8: 1, 9: 1, 10: 0.5, 11: 0.5}[fmt]
    pos = 0
    for ty in range(0, h, 8):
        for tx in range(0, w, 8):
            tile = data[int(pos):int(pos + 64 * bpp)]
            pos += 64 * bpp
            for y in range(8):
                for x in range(8):
                    m = MORTON[y][x]
                    if fmt == 0:
                        a, b, g, r = tile[m * 4:m * 4 + 4]
                        px = (r, g, b, a)
                    elif fmt == 1:
                        b, g, r = tile[m * 3:m * 3 + 3]
                        px = (r, g, b, 255)
                    elif fmt in (2, 3, 4):
                        v = struct.unpack_from('<H', tile, m * 2)[0]
                        if fmt == 2:
                            px = ((v >> 11 & 31) * 255 // 31, (v >> 6 & 31) * 255 // 31, (v >> 1 & 31) * 255 // 31, 255 * (v & 1))
                        elif fmt == 3:
                            px = ((v >> 11 & 31) * 255 // 31, (v >> 5 & 63) * 255 // 63, (v & 31) * 255 // 31, 255)
                        else:
                            px = ((v >> 12 & 15) * 17, (v >> 8 & 15) * 17, (v >> 4 & 15) * 17, (v & 15) * 17)
                    elif fmt == 5:
                        a, l = tile[m * 2:m * 2 + 2]
                        px = (l, l, l, a)
                    elif fmt == 7:
                        l = tile[m]
                        px = (l, l, l, 255)
                    elif fmt == 8:
                        px = (255, 255, 255, tile[m])
                    elif fmt == 9:
                        v = tile[m]
                        px = ((v >> 4) * 17,) * 3 + ((v & 15) * 17,)
                    else:
                        v = (tile[m // 2] >> (4 * (m & 1))) & 15
                        px = (v * 17,) * 3 + (255,) if fmt == 10 else (255, 255, 255, v * 17)
                    img[ty + y, tx + x] = px
    return img


def ctpk_textures(data):
    count, tex_off = struct.unpack_from('<HI', data, 6)
    out = []
    for i in range(count):
        e = 0x20 + i * 0x20
        _, size, off, fmt, w, h = struct.unpack_from('<IIIIHH', data, e)
        img = decode(fmt, w, h, data[tex_off + off:tex_off + off + size])
        out.append(Image.fromarray(img))
    return out


# BCLIM format numbers → the numbers decode() uses (CTPK's)
BCLIM_FORMATS = {0: 7, 1: 8, 2: 9, 3: 5, 5: 3, 6: 1, 7: 2, 8: 4, 9: 0, 10: 12, 11: 13, 12: 10, 13: 11}


def bclim_texture(data):
    """layout images: pixel data followed by a CLIM footer with the size and format"""
    footer = data[-0x28:]
    assert footer[:4] in (b'CLIM', b'FLIM')
    if footer[:4] == b'CLIM':
        w, h, fmt = struct.unpack_from('<HHI', footer, 0x14 + 8)
        swizzle = 0
    else:  # BFLIM: width, height, alignment, format (1 byte), swizzle
        w, h, _, fmt, swizzle = struct.unpack_from('<HHHBB', footer, 0x14 + 8)
    if swizzle in (4, 8):
        # stored turned on its side: decode with width and height swapped, then turn it back
        pw, ph = (1 << max(3, (h - 1).bit_length())), (1 << max(3, (w - 1).bit_length()))
        img = Image.fromarray(decode(BCLIM_FORMATS[fmt], pw, ph, data[:-0x28]))
        img = img.transpose(Image.ROTATE_90 if swizzle == 4 else Image.TRANSPOSE)
        return img.crop((0, 0, w, h))
    pw, ph = (1 << max(3, (w - 1).bit_length())), (1 << max(3, (h - 1).bit_length()))
    img = decode(BCLIM_FORMATS[fmt], pw, ph, data[:-0x28])
    return Image.fromarray(img[:h, :w])


def main_layout(rom, out, *wanted):
    """layout/<name>.zlib → SARC → *.bclim"""
    raw = os.path.join(out, '_raw')
    import contextlib, io
    with contextlib.redirect_stdout(io.StringIO()):
        romfs_main(rom, raw, *[f'layout/{w}' for w in wanted])
    for root, _, files in os.walk(os.path.join(raw, 'layout')):
        for f in files:
            data = open(os.path.join(root, f), 'rb').read()
            sarc = zlib.decompress(data[4:])
            for name, content in sarc_files(sarc).items():
                if not name.endswith(('.bclim', '.bflim')):
                    continue
                try:
                    img = bclim_texture(content)
                except Exception as e:
                    print('skip', name, e)
                    continue
                dest = os.path.join(out, 'layout', f[:-5], os.path.basename(name)[:-6] + '.png')
                os.makedirs(os.path.dirname(dest), exist_ok=True)
                img.save(dest)
                print(dest)


def main(rom, out, *wanted):
    raw = os.path.join(out, '_raw')
    import contextlib, io
    with contextlib.redirect_stdout(io.StringIO()):
        romfs_main(rom, raw, *[f'cellanim/{w}' for w in wanted])
    for root, _, files in os.walk(os.path.join(raw, 'cellanim')):
        for f in files:
            data = open(os.path.join(root, f), 'rb').read()
            sarc = zlib.decompress(data[4:])
            for name, content in sarc_files(sarc).items():
                if not name.endswith('.ctpk'):
                    continue
                for k, img in enumerate(ctpk_textures(content)):
                    dest = os.path.join(out, f[:-5], os.path.basename(name)[:-5] + (f'_{k}' if k else '') + '.png')
                    os.makedirs(os.path.dirname(dest), exist_ok=True)
                    img.save(dest)
                    print(dest)


if __name__ == '__main__':
    if len(sys.argv) > 1 and sys.argv[1] == '--layout':
        main_layout(*sys.argv[2:])
    else:
        main(*sys.argv[1:])
