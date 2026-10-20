#!/usr/bin/env python3
"""
GBA（MP2K / Sappy）の ROM の曲表から、曲ごとに鳴らしているサンプル（音の素材）を一覧にします。
効果音や声は「1曲＝1つの音」になっていることが多いので、ミニゲームの声を探す手がかりになります。

  python3 Tools/gba_songs.py <ROM.gba> <曲表の先頭（16進）> <曲表の終わり（16進）>

出力: 曲番号  曲ヘッダの位置  音色表の位置  鳴らすサンプル（位置:キー）
"""
import struct
import sys

rom = open(sys.argv[1], 'rb').read()
N = len(rom)
G = 0x08000000


def ptr(p):
    v = struct.unpack_from('<I', rom, p)[0]
    return v - G if G <= v < G + N else None


def instrument(vg, prog, key):
    """voicegroup entry → sample address (follows drum kits and key splits)"""
    e = vg + 12 * prog
    if e + 12 > N:
        return None
    t = rom[e]
    if t == 0x80:  # drum kit: one instrument per key
        kit = ptr(e + 4)
        return instrument(kit, key, key) if kit is not None else None
    if t == 0x40:  # key split
        sub, table = ptr(e + 4), ptr(e + 8)
        if sub is None or table is None:
            return None
        return instrument(sub, rom[table + key], key)
    if t & 7 in (0, 0x08 & 7) and t in (0, 8, 0x10, 0x18):
        return ptr(e + 4)
    return None


ONE_ARG = {0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC8}


def track_notes(p, vg):
    out = set()
    prog, key, cmd = 0, 60, None
    ret = []
    for _ in range(20000):
        if p >= N:
            break
        b = rom[p]
        if b < 0x80:  # running status
            if cmd is None:
                break
            b = cmd
        else:
            p += 1
        if 0x80 <= b <= 0xB0:
            continue
        if b in (0xB1, 0xB6):
            if b == 0xB1:
                break
            continue
        if b == 0xB2:
            break  # loop back: everything already seen
        if b == 0xB3:
            t = ptr(p)
            ret.append(p + 4)
            p = t if t is not None else p + 4
            continue
        if b == 0xB4:
            if not ret:
                break
            p = ret.pop()
            continue
        if b == 0xB9:
            p += 3
            continue
        if b in ONE_ARG:
            cmd = b if b >= 0xBD else None
            if b == 0xBD:
                prog = rom[p]
            p += 1
            continue
        if b == 0xCD:
            p += 2
            continue
        if b == 0xCE:
            cmd = b
            if rom[p] < 0x80:
                p += 1
            continue
        if b >= 0xCF:
            cmd = b
            if rom[p] < 0x80:
                key = rom[p]
                p += 1
                for _ in range(2):  # velocity, gate time
                    if rom[p] < 0x80:
                        p += 1
            s = instrument(vg, prog, key)
            if s is not None:
                out.add((s, key))
            continue
        break
    return out


def main():
    start, end = int(sys.argv[2], 16), int(sys.argv[3], 16)
    for n, e in enumerate(range(start, end, 8)):
        h = ptr(e)
        if h is None or not (1 <= rom[h] <= 16):
            continue
        vg = ptr(h + 4)
        if vg is None:
            continue
        samples = set()
        for t in range(rom[h]):
            tp = ptr(h + 8 + 4 * t)
            if tp is not None:
                samples |= track_notes(tp, vg)
        rates = []
        for s, k in sorted(samples):
            if s + 16 <= N:
                pitch, size = struct.unpack_from('<I', rom, s + 4)[0] // 1024, struct.unpack_from('<I', rom, s + 12)[0]
                rates.append(f'{s:07X}:{k}({size / pitch:.2f}s)' if pitch else f'{s:07X}:{k}')
        print(f'{n:5} {h:07X} vg {vg:07X} tracks {rom[h]} ', ' '.join(rates))


if __name__ == '__main__':
    main()
