#!/usr/bin/env python3
"""
DS（リズム天国ゴールド）の ROM から画像を PNG に書き出します。日本語版の文字やデザインを確かめる資料用です。

  Tools/.venv/bin/python Tools/dump_ds_graphics.py <ROM.nds> <出力フォルダ> [フォルダ名 ...]
  Tools/.venv/bin/python Tools/dump_ds_graphics.py --cells <ROM.nds> <出力フォルダ> [フォルダ名 ...]   # 部品を組み立てた絵

- grapharc/<フォルダ>/ の NCGR・NCBR（絵）を、同じフォルダの NCLR（色）で塗って並べます。
  セルやアニメーションの組み立てはしないので、部品がタイル状に並んだ1枚の画像になります。
- 色が合わないときのために、同じフォルダの全パレットで書き出します（ファイル名の末尾がパレット名）。
ndspy が必要です（Tools/.venv に入っています）。
"""
import os
import struct
import sys

import ndspy.lz10
import ndspy.rom
from PIL import Image


def sections(data):
    """Nitro file sections: {magic: bytes of the section}"""
    out = {}
    pos = struct.unpack_from('<H', data, 0xC)[0]
    while pos + 8 <= len(data):
        magic = data[pos:pos + 4][::-1].decode('latin1')
        size = struct.unpack_from('<I', data, pos + 4)[0]
        if size < 8:
            break
        out[magic] = data[pos:pos + size]
        pos += size
    return out


def palette(data):
    pltt = sections(data)['PLTT']
    depth, _, size, offset = struct.unpack_from('<IIII', pltt, 8)
    raw = pltt[8 + offset:8 + offset + size] if 8 + offset + size <= len(pltt) else pltt[0x18:]
    colors = []
    for i in range(0, len(raw) - 1, 2):
        v = struct.unpack_from('<H', raw, i)[0]
        colors.append(((v & 31) * 255 // 31, ((v >> 5) & 31) * 255 // 31, ((v >> 10) & 31) * 255 // 31))
    return colors


def characters(data):
    char = sections(data)['CHAR']
    h, w, depth, _, scanned, size, offset = struct.unpack_from('<HHIIIII', char, 8)
    pixels = char[8 + offset:8 + offset + size] if offset else char[0x20:]
    bpp = 4 if depth == 3 else 8
    values = []
    for b in pixels:
        if bpp == 4:
            values += [b & 15, b >> 4]
        else:
            values.append(b)
    # both NCGR and NCBR (1D-mapped sprites) are stored as 8x8 tiles here
    return values, (w if w not in (0xFFFF, 0) else 32), False


def render(values, width_tiles, scanned, colors, bank=0):
    if scanned:
        w = width_tiles * 8
        h = (len(values) + w - 1) // w
        img = Image.new('RGBA', (w, h), (0, 0, 0, 0))
        px = img.load()
        for i, v in enumerate(values):
            if v:
                c = colors[(bank * 16 + v) % len(colors)] if len(colors) else (v, v, v)
                px[i % w, i // w] = c + (255,)
        return img
    tiles = len(values) // 64
    cols = width_tiles
    rows = (tiles + cols - 1) // cols
    img = Image.new('RGBA', (cols * 8, rows * 8), (0, 0, 0, 0))
    px = img.load()
    for t in range(tiles):
        tx, ty = (t % cols) * 8, (t // cols) * 8
        for i in range(64):
            v = values[t * 64 + i]
            if v:
                c = colors[(bank * 16 + v) % len(colors)] if len(colors) else (v, v, v)
                px[tx + i % 8, ty + i // 8] = c + (255,)
    return img


def walk(folder, prefix=''):
    for i, name in enumerate(folder.files):
        yield prefix + name, folder.firstID + i
    for name, sub in folder.folders:
        yield from walk(sub, prefix + name + '/')


def main(rom_path, out, *wanted):
    rom = ndspy.rom.NintendoDSRom.fromFile(rom_path)
    files = dict(walk(rom.filenames))

    def read(name):
        data = rom.files[files[name]]
        return ndspy.lz10.decompress(data) if name.endswith('_LZ') else data

    for name in files:
        parts = name.split('/')
        if parts[0] != 'grapharc' or (wanted and parts[1] not in wanted):
            continue
        if not any(name.endswith(e) for e in ('.NCGR', '.NCGR_LZ', '.NCBR', '.NCBR_LZ')):
            continue
        folder = os.path.dirname(name)
        pals = [n for n in files if os.path.dirname(n) == folder and n.endswith('.NCLR')]
        try:
            values, width, scanned = characters(read(name))
        except Exception as e:
            print('skip', name, e)
            continue
        for pal in pals:
            colors = palette(read(pal))
            img = render(values, width, scanned, colors)
            dest = os.path.join(out, folder.replace('grapharc/', ''),
                                f'{os.path.basename(name).split(".")[0]}__{os.path.basename(pal).split(".")[0]}.png')
            os.makedirs(os.path.dirname(dest), exist_ok=True)
            img.save(dest)
        print(name, f'{len(values)} px, パレット {len(pals)}')




# ---- sprites: put the 8x8 tiles together the way the cell data (NCER) says ----------------------

OBJ_SIZES = {0: [(8, 8), (16, 16), (32, 32), (64, 64)],
             1: [(16, 8), (32, 8), (32, 16), (64, 32)],
             2: [(8, 16), (8, 32), (16, 32), (32, 64)]}


def cells(data):
    """[(list of OAM (x, y, w, h, tile, palette, hflip, vflip, is8bpp))] for every cell"""
    sec = sections(data)['CEBK']
    count, bank_type, offset, mapping = struct.unpack_from('<HHII', sec, 8)
    base = 8 + offset
    entry = 16 if bank_type == 1 else 8
    oam_base = base + count * entry
    out = []
    for c in range(count):
        n, _, oam_off = struct.unpack_from('<HHI', sec, base + c * entry)
        objs = []
        for i in range(n):
            a0, a1, a2 = struct.unpack_from('<HHH', sec, oam_base + oam_off + i * 6)
            y = a0 & 0xFF
            y = y - 256 if y >= 128 else y
            x = a1 & 0x1FF
            x = x - 512 if x >= 256 else x
            shape, size = (a0 >> 14) & 3, (a1 >> 14) & 3
            if shape == 3:
                continue
            w, h = OBJ_SIZES[shape][size]
            objs.append((x, y, w, h, a2 & 0x3FF, (a2 >> 12) & 15, bool(a1 & 0x1000), bool(a1 & 0x2000), bool(a0 & 0x2000)))
        out.append(objs)
    return out, mapping


def t_out_of_range(k, values):
    return k >= len(values)


def render_cells(values, bpp8, cell_list, mapping, colors, bitmap=False):
    boundary = mapping  # bytes per tile number (1D mapping), from the character data's header
    tile_bytes = 64 if bpp8 else 32
    images = []
    for objs in cell_list:
        if not objs:
            continue
        x0 = min(o[0] for o in objs); y0 = min(o[1] for o in objs)
        x1 = max(o[0] + o[2] for o in objs); y1 = max(o[1] + o[3] for o in objs)
        img = Image.new('RGBA', (x1 - x0, y1 - y0), (0, 0, 0, 0))
        for x, y, w, h, tile, pal, hf, vf, _ in reversed(objs):
            first = tile * boundary // tile_bytes
            part = Image.new('RGBA', (w, h), (0, 0, 0, 0))
            px = part.load()
            for i in range(w * h):
                if bitmap:
                    # NCBR: each object's pixels are one plain bitmap, row by row
                    ox, oy, k = i % w, i // w, first * 64 + i
                else:
                    # NCGR: 8x8 tiles, row by row
                    t, j = divmod(i, 64)
                    ox, oy, k = (t % (w // 8)) * 8 + j % 8, (t // (w // 8)) * 8 + j // 8, (first + t) * 64 + j
                if t_out_of_range(k, values):
                    break
                v = values[k]
                if v:
                    idx = v if bpp8 else pal * 16 + v
                    if idx < len(colors):
                        px[ox, oy] = colors[idx] + (255,)
            if hf:
                part = part.transpose(Image.FLIP_LEFT_RIGHT)
            if vf:
                part = part.transpose(Image.FLIP_TOP_BOTTOM)
            img.alpha_composite(part, (x - x0, y - y0))
        images.append(img)
    return images


def sheet(images, width=1024, pad=6):
    x = y = row = 0
    places = []
    for im in images:
        if x + im.width > width:
            x, y, row = 0, y + row + pad, 0
        places.append((x, y))
        x += im.width + pad
        row = max(row, im.height)
    out = Image.new('RGBA', (width, max(1, y + row)), (255, 255, 255, 255))
    for im, p in zip(images, places):
        out.alpha_composite(im, p)
    return out


def main_cells(rom_path, out, *wanted):
    rom = ndspy.rom.NintendoDSRom.fromFile(rom_path)
    files = dict(walk(rom.filenames))

    def read(name):
        data = rom.files[files[name]]
        return ndspy.lz10.decompress(data) if name.endswith('_LZ') else data

    for name in files:
        parts = name.split('/')
        if parts[0] != 'grapharc' or (wanted and parts[1] not in wanted) or not name.endswith('.NCER'):
            continue
        stem = name[:-5]
        char = next((stem + e for e in ('.NCBR_LZ', '.NCBR', '.NCGR_LZ', '.NCGR') if stem + e in files), None)
        same = [n for n in files if os.path.dirname(n) == os.path.dirname(name) and n.endswith('.NCLR')]
        pal = next((stem + e for e in ('.NCLR',) if stem + e in files), None) or \
            next((n for n in same if not n.endswith('mesg.NCLR')), None) or (same[0] if same else None)
        if not char or not pal:
            continue
        values, _, _ = characters(read(char))
        depth, char_mapping = struct.unpack_from('<II', sections(read(char))['CHAR'], 12)
        cell_list, _ = cells(read(name))
        # 0x10 / 0x100010 / 0x200010 / 0x300010 = 1D mapping with 32 / 64 / 128 / 256 byte tile numbers
        mapping = 32 << ((char_mapping >> 20) & 3)
        colors = palette(read(pal))
        imgs = render_cells(values, depth == 4, cell_list, mapping, colors, bitmap='NCBR' in char)
        dest = os.path.join(out, stem.replace('grapharc/', '') + '_cells.png')
        os.makedirs(os.path.dirname(dest), exist_ok=True)
        sheet(imgs).save(dest)
        print(name, f'{len(imgs)} セル')


if __name__ == '__main__':
    if len(sys.argv) > 1 and sys.argv[1] == '--cells':
        main_cells(*sys.argv[2:])
    else:
        main(*sys.argv[1:])
