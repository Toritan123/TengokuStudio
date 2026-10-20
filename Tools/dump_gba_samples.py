#!/usr/bin/env python3
"""
GBA の ROM から、サウンドエンジン（MP2K / いわゆる Sappy）の波形サンプルを探して WAV に書き出します。

  python3 Tools/dump_gba_samples.py <ROM.gba> <出力フォルダ> [最短の秒数]

サンプルは「種類(0 か 0x40000000)・ピッチ(Hz×1024)・ループ開始・長さ」の16バイトの頭＋8ビットの波形
という形で入っているので、それらしい頭を ROM 全体から探します。声のような長さ（指定の秒数〜10秒）のものだけを残します。
"""
import os
import struct
import sys
import wave


def main(rom_path, out, min_seconds='0.15'):
    rom = open(rom_path, 'rb').read()
    os.makedirs(out, exist_ok=True)
    found = 0
    pos = 0
    while pos + 16 < len(rom):
        kind, pitch, loop, size = struct.unpack_from('<IIII', rom, pos)
        if kind in (0, 0x40000000) and 1024 * 4000 <= pitch <= 1024 * 48000 and pitch % 1024 == 0 \
                and 64 <= size <= len(rom) - pos - 16 and loop <= size:
            rate = pitch // 1024
            # a 'sample' longer than 10 seconds is almost certainly other data that happens to look like a header
            if float(min_seconds) <= size / rate <= 10:
                pcm = rom[pos + 16:pos + 16 + size]
                # real sound has small steps between samples; random data doesn't
                diffs = sum(abs(((pcm[i] + 128) & 255) - ((pcm[i + 1] + 128) & 255)) for i in range(0, min(len(pcm) - 1, 4000)))
                if diffs / min(len(pcm) - 1, 4000) < 40:
                    with wave.open(os.path.join(out, f'{pos:07X}_{rate}Hz.wav'), 'wb') as w:
                        w.setnchannels(1)
                        w.setsampwidth(1)
                        w.setframerate(rate)
                        w.writeframes(bytes((b + 128) & 255 for b in pcm))  # signed → unsigned 8-bit
                    found += 1
                    pos += 16 + size
                    pos += (-pos) % 4
                    continue
        pos += 4
    print(found, '個')


if __name__ == '__main__':
    main(*sys.argv[1:])
