#!/usr/bin/env python3
"""
GBA（MP2K / Sappy）の曲を、ざっくり WAV に書き出します。どの曲がどのミニゲームのものかを耳で確かめる用です。
エンベロープや LFO などは省いた簡易再生です（音の高さ・タイミング・音色は合います）。

  Tools/.venv/bin/python Tools/gba_render_song.py <ROM.gba> <曲表の先頭（16進）> <出力フォルダ> <曲番号 ...>
  例: ... 18be98 out 230 231 590

曲番号は Tools/gba_songs.py の一覧の左端の番号です。
--parts を付けると、鳴らした音（サンプル・キー・時刻）も一覧で出します。
"""
import os
import struct
import sys
import wave

import numpy as np

RATE = 22050
G = 0x08000000
W = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 28, 30, 32, 36, 40, 42,
     44, 48, 52, 54, 56, 60, 64, 66, 68, 72, 76, 78, 80, 84, 88, 90, 92, 96]


class Rom:
    def __init__(self, path):
        self.d = open(path, 'rb').read()
        self.n = len(self.d)

    def ptr(self, p):
        v = struct.unpack_from('<I', self.d, p)[0]
        return v - G if G <= v < G + self.n else None


def instrument(rom, vg, prog, key):
    """→ (sample address, fixed pitch?, key used for pitch)"""
    e = vg + 12 * prog
    t = rom.d[e]
    if t == 0x80:
        kit = rom.ptr(e + 4)
        if kit is None:
            return None
        r = instrument(rom, kit, key, key)
        if r is None:
            return None
        # a drum kit plays each instrument at that instrument's own base key
        return r[0], r[1], rom.d[kit + 12 * key + 1]
    if t == 0x40:
        sub, table = rom.ptr(e + 4), rom.ptr(e + 8)
        if sub is None or table is None:
            return None
        return instrument(rom, sub, rom.d[table + key], key)
    if t in (0, 8, 0x10, 0x18):
        s = rom.ptr(e + 4)
        return (s, bool(t & 8), key) if s is not None else None
    return None


def events(rom, p, vg):
    """track bytes → [(tick, sample, fixed, key, velocity, length, volume, pan -64..63)]"""
    out = []
    t, prog, vol, pan, keysh, cmd = 0, 0, 100, 0, 0, None
    key, vel, gate = 60, 100, 0
    ret = []
    for _ in range(100000):
        b = rom.d[p]
        if b < 0x80:
            if cmd is None:
                break
            b = cmd
        else:
            p += 1
        if 0x80 <= b <= 0xB0:
            t += W[b - 0x80]
            continue
        if b in (0xB1, 0xB2):
            break  # end, or loop back (play once)
        if b == 0xB3:
            ret.append(p + 4)
            p = rom.ptr(p) or p + 4
            continue
        if b == 0xB4:
            if not ret:
                break
            p = ret.pop()
            continue
        if b == 0xB9:
            p += 3
            continue
        if b in (0xBA, 0xBB, 0xBC, 0xBD, 0xBE, 0xBF, 0xC0, 0xC1, 0xC2, 0xC3, 0xC4, 0xC5, 0xC8):
            a = rom.d[p]
            p += 1
            cmd = b if b >= 0xBD else None
            if b == 0xBD:
                prog = a
            elif b == 0xBE:
                vol = a
            elif b == 0xBF:
                pan = a - 64
            elif b == 0xBC:
                keysh = a - 256 if a > 127 else a
            elif b == 0xBB:
                out.append((t, 'tempo', a * 2))
            continue
        if b == 0xCD:
            p += 2
            continue
        if b == 0xCE:
            cmd = b
            if rom.d[p] < 0x80:
                p += 1
            continue
        if b >= 0xCF:
            cmd = b
            g = 0
            if rom.d[p] < 0x80:
                key = rom.d[p]
                p += 1
                if rom.d[p] < 0x80:
                    vel = rom.d[p]
                    p += 1
                    if rom.d[p] < 0x80:
                        g = rom.d[p]
                        p += 1
            length = 96 * 4 if b == 0xCF else W[b - 0xCF] + g
            ins = instrument(rom, vg, prog, key)
            if ins:
                out.append((t, ins[0], ins[1], ins[2] + keysh if ins[1] is False else ins[2], vel, length, vol, pan))
            continue
        break
    return out


def sample(rom, s):
    flags, pitch, loop, size = struct.unpack_from('<IIII', rom.d, s)
    pcm = np.frombuffer(rom.d[s + 16:s + 16 + size], np.int8).astype(np.float32) / 128
    return pcm, pitch / 1024, (loop if flags & 0x40000000 else None)


def render(rom, h):
    vg = rom.ptr(h + 4)
    evs = []
    for k in range(rom.d[h]):
        tp = rom.ptr(h + 8 + 4 * k)
        if tp is not None:
            evs += events(rom, tp, vg)
    evs.sort(key=lambda e: e[0])
    # ticks → seconds (tempo changes)
    tempo, last_t, last_s = 150, 0, 0.0
    notes = []
    for e in evs:
        sec = last_s + (e[0] - last_t) / 24 * 60 / tempo
        if e[1] == 'tempo':
            last_t, last_s, tempo = e[0], sec, max(1, e[2])
            continue
        notes.append((sec, e[1], e[2], e[3], e[4], e[5] / 24 * 60 / tempo, e[6]))
    end = max([n[0] + n[5] for n in notes] + [0]) + 1.5
    out = np.zeros(int(end * RATE) + RATE * 3, np.float32)
    for sec, s, fixed, key, vel, length, vol in notes:
        pcm, rate, loop = sample(rom, s)
        step = rate / RATE * (1 if fixed else 2 ** ((key - 60) / 12))
        dur = len(pcm) / (rate * (1 if fixed else 2 ** ((key - 60) / 12)))
        if loop is not None:
            dur = length  # looped instruments hold while the note is held
        n = int(min(dur, length + 0.05 if loop is not None else dur) * RATE)
        idx = np.arange(n) * step
        if loop is not None and len(pcm) > loop + 1:
            span = len(pcm) - loop
            idx = np.where(idx >= len(pcm), loop + (idx - loop) % span, idx)
        idx = idx[idx < len(pcm) - 1]
        y = np.interp(idx, np.arange(len(pcm)), pcm) * (vel / 127) * (vol / 127)
        a = int(sec * RATE)
        out[a:a + len(y)] += y[:len(out) - a]
    return out, notes


def main():
    args = [a for a in sys.argv[1:] if a != '--parts']
    rom = Rom(args[0])
    table, dest = int(args[1], 16), args[2]
    os.makedirs(dest, exist_ok=True)
    for num in args[3:]:
        h = rom.ptr(table + 8 * int(num))
        audio, notes = render(rom, h)
        peak = np.abs(audio).max() or 1
        pcm = (audio / peak * 0.9 * 32767).astype('<i2')
        path = os.path.join(dest, f'song{int(num):04}.wav')
        with wave.open(path, 'wb') as w:
            w.setnchannels(1)
            w.setsampwidth(2)
            w.setframerate(RATE)
            w.writeframes(pcm.tobytes())
        print(path, f'{len(audio) / RATE:.1f}s')
        if '--parts' in sys.argv:
            for n in notes:
                print(f'  {n[0]:7.2f}s {n[1]:07X} key{n[3]} vel{n[4]}')


if __name__ == '__main__':
    main()
