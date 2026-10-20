#!/usr/bin/env python3
"""
暗号化されていない 3DS の ROM（.3ds）から RomFS のファイルを取り出します。日本語版の画像を確かめる資料用です。

  python3 Tools/dump_3ds_romfs.py <ROM.3ds> <出力フォルダ> [パスに含まれる文字 ...]

文字を指定すると、パスにその文字を含むファイルだけを書き出します（指定しないと一覧だけ表示）。
"""
import os
import struct
import sys


def main(rom, out, *wanted):
    f = open(rom, 'rb')
    f.seek(0x120)
    ncch = struct.unpack('<I', f.read(4))[0] * 0x200  # partition 0
    f.seek(ncch + 0x1B0)
    romfs_off, romfs_size = struct.unpack('<II', f.read(8))
    romfs = ncch + romfs_off * 0x200
    f.seek(romfs)
    ivfc = f.read(0x60)
    assert ivfc[:4] == b'IVFC', 'RomFS がない（暗号化されている？）'
    # level 3 (the file system) comes after the master hash, aligned to its block size
    master_size = struct.unpack_from('<I', ivfc, 8)[0]
    l3_block = 1 << struct.unpack_from('<I', ivfc, 0x4C)[0]  # level 3 block size (log2)
    l3 = romfs + ((0x60 + master_size + l3_block - 1) // l3_block) * l3_block
    f.seek(l3)
    hdr = struct.unpack('<10I', f.read(40))
    dir_meta_off, dir_meta_size, file_meta_off, file_meta_size, data_off = hdr[3], hdr[4], hdr[7], hdr[8], hdr[9]
    f.seek(l3 + dir_meta_off); dirs = f.read(dir_meta_size)
    f.seek(l3 + file_meta_off); files = f.read(file_meta_size)

    def name(buf, off, base):
        n = struct.unpack_from('<I', buf, off + base)[0]
        return buf[off + base + 4:off + base + 4 + n].decode('utf-16le')

    result = []

    def walk_dir(off, path):
        parent, sibling, child, first_file, _, n = struct.unpack_from('<6I', dirs, off)
        f_off = first_file
        while f_off != 0xFFFFFFFF:
            _, next_file, data, size, _, fn = struct.unpack_from('<IIQQII', files, f_off)
            fname = files[f_off + 0x20:f_off + 0x20 + fn].decode('utf-16le')
            result.append((path + fname, l3 + data_off + data, size))
            f_off = next_file
        c = child
        while c != 0xFFFFFFFF:
            _, sib, _, _, _, cn = struct.unpack_from('<6I', dirs, c)
            walk_dir(c, path + dirs[c + 0x18:c + 0x18 + cn].decode('utf-16le') + '/')
            c = sib

    walk_dir(0, '')
    if not wanted:
        for p, _, size in result:
            print(f'{size:10d} {p}')
        return
    for p, off, size in result:
        if any(w in p for w in wanted):
            dest = os.path.join(out, p)
            os.makedirs(os.path.dirname(dest), exist_ok=True)
            f.seek(off)
            open(dest, 'wb').write(f.read(size))
            print(p)


if __name__ == '__main__':
    main(*sys.argv[1:])
