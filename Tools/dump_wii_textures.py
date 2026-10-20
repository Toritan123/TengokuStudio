#!/usr/bin/env python3
"""
Wii（みんなのリズム天国）のディスクから取り出したファイルの中の画像を PNG に書き出します。
日本語版の文字やデザインを確かめる資料用です。

  ディスクの展開: Tools/bin/nodtool extract "<.rvz>" JapaneseImageRefs/wii_raw
  python3 Tools/dump_wii_textures.py <.szs/.arc/.tpl ファイルかフォルダ ...> -- <出力フォルダ>

.szs（Yaz0 圧縮）→ U8 アーカイブ → .tpl（テクスチャ）を展開し、1枚ずつ PNG にします。
"""
import os
import struct
import sys

from PIL import Image


def yaz0(data):
    if data[:4] != b'Yaz0':
        return data
    size = struct.unpack_from('>I', data, 4)[0]
    out = bytearray()
    pos = 16
    while len(out) < size:
        flags = data[pos]
        pos += 1
        for bit in range(7, -1, -1):
            if len(out) >= size:
                break
            if flags >> bit & 1:
                out.append(data[pos])
                pos += 1
            else:
                b1, b2 = data[pos], data[pos + 1]
                pos += 2
                dist = ((b1 & 15) << 8 | b2) + 1
                n = b1 >> 4
                if n == 0:
                    n = data[pos] + 0x12
                    pos += 1
                else:
                    n += 2
                start = len(out) - dist
                for i in range(n):
                    out.append(out[start + i])
    return bytes(out)


def u8_files(data):
    """U8 archive → {path: bytes}"""
    if data[:4] != b'\x55\xAA\x38\x2D':
        return {}
    root_off = struct.unpack_from('>I', data, 4)[0]
    count = struct.unpack_from('>I', data, root_off + 8)[0]
    names = root_off + count * 12
    out = {}
    stack = []
    for i in range(1, count):
        e = root_off + i * 12
        info, off, size = struct.unpack_from('>III', data, e)
        is_dir, name_off = info >> 24, info & 0xFFFFFF
        name = data[names + name_off:data.index(b'\0', names + name_off)].decode('latin1')
        while stack and stack[-1][1] <= i:
            stack.pop()
        if is_dir:
            stack.append((name, size))
        else:
            out['/'.join(s[0] for s in stack) + '/' + name] = data[off:off + size]
    return out


def rgb5a3(v):
    if v & 0x8000:
        return ((v >> 10 & 31) * 255 // 31, (v >> 5 & 31) * 255 // 31, (v & 31) * 255 // 31, 255)
    return ((v >> 8 & 15) * 17, (v >> 4 & 15) * 17, (v & 15) * 17, (v >> 12 & 7) * 255 // 7)


def decode(fmt, w, h, d, palette=None):
    img = Image.new('RGBA', (w, h))
    px = img.load()
    # block sizes per format
    bw, bh = {0: (8, 8), 1: (8, 4), 2: (8, 4), 3: (4, 4), 4: (4, 4), 5: (4, 4), 6: (4, 4),
              8: (8, 8), 9: (8, 4), 10: (4, 4), 14: (8, 8)}[fmt]
    pos = 0

    def put(x, y, c):
        if x < w and y < h:
            px[x, y] = c

    for by in range(0, h, bh):
        for bx in range(0, w, bw):
            if fmt == 6:  # RGBA8: AR block then GB block
                ar = d[pos:pos + 32]; gb = d[pos + 32:pos + 64]; pos += 64
                for i in range(16):
                    put(bx + i % 4, by + i // 4, (ar[i * 2 + 1], gb[i * 2], gb[i * 2 + 1], ar[i * 2]))
                continue
            if fmt == 14:  # CMPR (DXT1), 2x2 sub-blocks of 4x4
                for sb in range(4):
                    c0, c1 = struct.unpack_from('>HH', d, pos)
                    bits = struct.unpack_from('>I', d, pos + 4)[0]
                    pos += 8
                    def col(v):
                        return ((v >> 11 & 31) * 255 // 31, (v >> 5 & 63) * 255 // 63, (v & 31) * 255 // 31)
                    a, b = col(c0), col(c1)
                    if c0 > c1:
                        pal = [a + (255,), b + (255,), tuple((2 * x + y) // 3 for x, y in zip(a, b)) + (255,),
                               tuple((x + 2 * y) // 3 for x, y in zip(a, b)) + (255,)]
                    else:
                        pal = [a + (255,), b + (255,), tuple((x + y) // 2 for x, y in zip(a, b)) + (255,), (0, 0, 0, 0)]
                    sx, sy = bx + (sb % 2) * 4, by + (sb // 2) * 4
                    for i in range(16):
                        put(sx + i % 4, sy + i // 4, pal[bits >> (30 - i * 2) & 3])
                continue
            for i in range(bw * bh):
                x, y = bx + i % bw, by + i // bw
                if fmt == 0:  # I4
                    v = d[pos + i // 2] >> (4 if i % 2 == 0 else 0) & 15
                    put(x, y, (v * 17,) * 3 + (255,))
                elif fmt == 1:  # I8
                    v = d[pos + i]
                    put(x, y, (v, v, v, 255))
                elif fmt == 2:  # IA4
                    v = d[pos + i]
                    put(x, y, ((v & 15) * 17,) * 3 + ((v >> 4) * 17,))
                elif fmt == 3:  # IA8
                    a, v = d[pos + i * 2], d[pos + i * 2 + 1]
                    put(x, y, (v, v, v, a))
                elif fmt == 4:  # RGB565
                    v = struct.unpack_from('>H', d, pos + i * 2)[0]
                    put(x, y, ((v >> 11 & 31) * 255 // 31, (v >> 5 & 63) * 255 // 63, (v & 31) * 255 // 31, 255))
                elif fmt == 5:  # RGB5A3
                    put(x, y, rgb5a3(struct.unpack_from('>H', d, pos + i * 2)[0]))
                elif fmt in (8, 9, 10):  # palette formats
                    if fmt == 8:
                        idx = d[pos + i // 2] >> (4 if i % 2 == 0 else 0) & 15
                    elif fmt == 9:
                        idx = d[pos + i]
                    else:
                        idx = struct.unpack_from('>H', d, pos + i * 2)[0] & 0x3FFF
                    if palette and idx < len(palette):
                        put(x, y, palette[idx])
            pos += {0: 32, 1: 32, 2: 32, 3: 32, 4: 32, 5: 32, 8: 32, 9: 32, 10: 32}[fmt]
    return img


def tpl_images(data):
    if data[:4] != b'\x00\x20\xAF\x30':
        return []
    count, table = struct.unpack_from('>II', data, 4)
    out = []
    for i in range(count):
        img_off, pal_off = struct.unpack_from('>II', data, table + i * 8)
        h, w, fmt, doff = struct.unpack_from('>HHII', data, img_off)
        palette = None
        if pal_off:
            n, _, pfmt, poff = struct.unpack_from('>HBxII', data, pal_off)
            vals = [struct.unpack_from('>H', data, poff + k * 2)[0] for k in range(n)]
            if pfmt == 0:
                palette = [((v & 0xFF),) * 3 + ((v >> 8),) for v in vals]
            elif pfmt == 1:
                palette = [((v >> 11 & 31) * 255 // 31, (v >> 5 & 63) * 255 // 63, (v & 31) * 255 // 31, 255) for v in vals]
            else:
                palette = [rgb5a3(v) for v in vals]
        try:
            out.append(decode(fmt, w, h, data[doff:], palette))
        except Exception as e:
            print('  skip format', fmt, e)
    return out


def dump(path, out):
    data = yaz0(open(path, 'rb').read())
    files = u8_files(data) if data[:4] == b'\x55\xAA\x38\x2D' else {os.path.basename(path): data}
    base = os.path.splitext(os.path.basename(path))[0]
    for name, content in files.items():
        content = yaz0(content)
        if name.endswith('.tpl') or content[:4] == b'\x00\x20\xAF\x30':
            for k, img in enumerate(tpl_images(content)):
                dest = os.path.join(out, base, os.path.basename(name).rsplit('.', 1)[0] + (f'_{k}' if k else '') + '.png')
                os.makedirs(os.path.dirname(dest), exist_ok=True)
                img.save(dest)
                print(dest)


if __name__ == '__main__':
    args = sys.argv[1:]
    split = args.index('--')
    inputs, out = args[:split], args[split + 1]
    for p in inputs:
        if os.path.isdir(p):
            for root, _, fs in os.walk(p):
                for f in fs:
                    if f.endswith(('.szs', '.arc', '.tpl')):
                        dump(os.path.join(root, f), os.path.join(out, os.path.basename(p.rstrip('/')), os.path.relpath(root, p)))
        else:
            dump(p, out)
