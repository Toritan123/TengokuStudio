#!/usr/bin/env python3
"""
DS（リズム天国ゴールド / Rhythm Heaven）の効果音（SDAT のシーケンスアーカイブ）を WAV に書き出します。
同じサンプルをカエルごとに違う高さで鳴らすような声を、ゲームと同じ高さ・タイミングで取り出す用です。

  Tools/.venv/bin/python Tools/ds_render_seq.py <ROM.nds> <アーカイブ名> <出力フォルダ> [--inst 2,7,8] [名前の一部 ...]
  例: ... "リズム天国ゴールド.nds" SEQARC_KERO out --inst 2,7,8,9,10,11,12,13,14,15 SE_KERO_P

- 音の高さ = サンプルの再生レート × 2^((キー + 移調 − 音色の基準キー) / 12)。テンポは 1拍 48 tick。
- --inst を付けると、その音色番号の音だけ鳴らします（効果音を外して声だけにする）。
- 長さ 0 の音符はサンプルの最後まで鳴らします。ほかは音符の長さで切ります（10ms でフェード）。
- 音量はシーケンスの音量と音の強さ（どちらも DS と同じくおよそ2乗のカーブ）を反映します。
- エンベロープ・パン・ピッチスイープ・ビブラートは省いています。声の取り出し用の簡易版です。
"""
import os
import struct
import sys
import wave

import ndspy.rom
import ndspy.soundArchive
import numpy as np

RATE = 44100
IMA_STEPS = [7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 19, 21, 23, 25, 28, 31, 34, 37, 41, 45, 50, 55, 60, 66, 73, 80, 88,
             97, 107, 118, 130, 143, 157, 173, 190, 209, 230, 253, 279, 307, 337, 371, 408, 449, 494, 544, 598, 658,
             724, 796, 876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066, 2272, 2499, 2749, 3024, 3327, 3660,
             4026, 4428, 4871, 5358, 5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899, 15289, 16818,
             18500, 20350, 22385, 24623, 27086, 29794, 32767]
IMA_INDEX = [-1, -1, -1, -1, 2, 4, 6, 8]


def decode_wave(swav):
    """SWAV → float samples (PCM8, PCM16 or IMA-ADPCM)"""
    data = bytes(swav.data)
    kind = int(swav.waveType)
    if kind == 0:
        return np.frombuffer(data, np.int8).astype(np.float32) / 128
    if kind == 1:
        return np.frombuffer(data[:len(data) // 2 * 2], '<i2').astype(np.float32) / 32768
    # IMA-ADPCM: 4-byte header (first sample, step index), then 4-bit codes, low nibble first
    pred, index = struct.unpack_from('<hh', data, 0)
    out = []
    for byte in data[4:]:
        for code in (byte & 15, byte >> 4):
            step = IMA_STEPS[index]
            diff = step >> 3
            if code & 1:
                diff += step >> 2
            if code & 2:
                diff += step >> 1
            if code & 4:
                diff += step
            pred = max(-32768, min(32767, pred - diff if code & 8 else pred + diff))
            index = max(0, min(88, index + IMA_INDEX[code & 7]))
            out.append(pred)
    return np.array(out, np.float32) / 32768


def track_notes(events, start, tempo=120):
    """events from `start` → [(seconds, instrument, key, velocity, seconds long or None)]"""
    i = events.index(start)
    t, inst, transpose, out, stack = 0.0, 0, 0, [], []
    for _ in range(5000):
        e = events[i]
        kind = type(e).__name__
        tick = 60 / tempo / 48
        if kind == 'TempoSequenceEvent':
            tempo = e.value
        elif kind == 'InstrumentSwitchSequenceEvent':
            inst = e.instrumentID
        elif kind == 'TransposeSequenceEvent':
            transpose = e.value
        elif kind == 'RestSequenceEvent':
            t += e.duration * tick
        elif kind == 'NoteSequenceEvent':
            out.append((t, inst, e.pitch + transpose, e.velocity, e.duration * tick if e.duration else None))
            t += e.duration * tick
        elif kind == 'JumpSequenceEvent':
            i = events.index(e.destination)
            continue
        elif kind == 'CallSequenceEvent':
            stack.append(i + 1)
            i = events.index(e.destination)
            continue
        elif kind == 'ReturnSequenceEvent':
            if not stack:
                break
            i = stack.pop()
            continue
        elif kind == 'EndTrackSequenceEvent':
            break
        i += 1
        if i >= len(events):
            break
    return out


def sequence_notes(ssar, seq):
    events = ssar.events
    first = seq.firstEvent
    if type(first).__name__ != 'DefineTracksSequenceEvent':
        return track_notes(events, first)
    # multi-track: track 0 continues after the BeginTrack events; the others start where they point.
    # The tempo is the sequence's (set on track 0), so every track uses it.
    i = events.index(first) + 1
    starts = []
    while type(events[i]).__name__ == 'BeginTrackSequenceEvent':
        starts.append(events[i].firstEvent)
        i += 1
    tempo = next((e.value for e in events[i:i + 20] if type(e).__name__ == 'TempoSequenceEvent'), 120)
    notes = track_notes(events, events[i], tempo)
    for s in starts:
        notes += track_notes(events, s, tempo)
    # tracks that play the very same note (doubling for loudness) count once
    return sorted(set(notes), key=lambda n: (n[0], n[1], n[2]))


def render(notes, bank, waves, instruments=None, volume=127):
    """volume: the sequence's volume. The DS turns volume and velocity into decibels, about (v / 127) squared each."""
    parts = []
    for t, inst, key, vel, length in notes:
        if instruments is not None and inst not in instruments:
            continue
        nd = bank.instruments[inst].noteDefinition
        swav = waves[nd.waveArchiveIDID if hasattr(nd, 'waveArchiveIDID') else 0].waves[nd.waveID]
        pcm = decode_wave(swav)
        speed = swav.sampleRate / RATE * 2 ** ((key - nd.pitch) / 12)
        n = int(len(pcm) / speed)
        if length is not None:
            n = min(n, int(length * RATE))
        y = np.interp(np.arange(n) * speed, np.arange(len(pcm)), pcm).astype(np.float32) * (vel / 127) ** 2 * (volume / 127) ** 2
        fade = min(len(y), int(0.01 * RATE))
        y[len(y) - fade:] *= np.linspace(1, 0, fade)
        parts.append((t, y))
    if not parts:
        return np.zeros(1, np.float32)
    first = min(t for t, _ in parts)
    end = max(int((t - first) * RATE) + len(y) for t, y in parts)
    out = np.zeros(end, np.float32)
    for t, y in parts:
        a = int((t - first) * RATE)
        out[a:a + len(y)] += y
    return out


def load(rom_path, archive):
    rom = ndspy.rom.NintendoDSRom.fromFile(rom_path)
    sdat = ndspy.soundArchive.SDAT(rom.files[rom.filenames.idOf('soundarc/RIQ.sdat')])
    ssar = dict(sdat.sequenceArchives)[archive]
    ssar.parse()
    return sdat, ssar


def save(path, audio):
    """16-bit WAV, at the level it was rendered (only turned down if it would clip)"""
    peak = np.abs(audio).max() or 1
    pcm = (audio / max(peak, 1.0) * 32767).astype('<i2')
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(RATE)
        w.writeframes(pcm.tobytes())


def main(args):
    instruments = None
    if '--inst' in args:
        k = args.index('--inst')
        instruments = {int(x) for x in args[k + 1].split(',')}
        args = args[:k] + args[k + 2:]
    rom_path, archive, dest, *wanted = args
    sdat, ssar = load(rom_path, archive)
    os.makedirs(dest, exist_ok=True)
    for name, seq in ssar.sequences:
        if seq is None or (wanted and not any(w in name for w in wanted)):
            continue
        bank = sdat.banks[seq.bankID][1]
        waves = [sdat.waveArchives[w][1] for w in bank.waveArchiveIDs]
        notes = sequence_notes(ssar, seq)
        save(os.path.join(dest, name + '.wav'), render(notes, bank, waves, instruments, seq.volume))
        print(name, ' '.join(f'{t:.3f}s 音色{i} キー{k}' for t, i, k, _, _ in notes
                             if instruments is None or i in instruments))


if __name__ == '__main__':
    main(sys.argv[1:])
