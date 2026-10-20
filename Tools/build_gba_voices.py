#!/usr/bin/env python3
"""
GBA の ROM の声のサンプルを、ゲームの曲データと同じ鳴らし方（エコーの重ね・音符の長さで切る）で組み立てて、
Heaven Studio のファイル名の WAV にします。

  Tools/.venv/bin/python Tools/build_gba_voices.py <ROM.gba> WarioDeMambo

- 音の位置・強さ・長さは曲データ（tick）のとおり。テンポは曲のテンポ（BPM）で秒に直します。
- ゲームでは音符の長さで声が切れますが（音色の余韻 release が 0）、切れ目が目立つので最後まで鳴らします。
  ループ付きのサンプル（音符のあいだ伸ばす声）は、音符の長さまでループさせます。
- 曲にかかっている GBA の音源のリバーブ（残響）も同じ仕組みでかけます（音節の隙間がこれで埋まります）。
- 同じ声の2回目以降（エコー）は、英語版の録音に近づけてかなり小さく（強さの比の3乗）し、左右に振ってステレオにします
  （日本語版の曲データではエコーも中央で、音量の差が小さく聞こえるため）。
- Heaven Studio のコードが声を早めに鳴らしている分（offset）だけ、頭に無音を足します。
- 音量はここで英語版にそろえます（リミッターはエコーと残響を足したあとの全体に、ゆっくり効く設定でかけ、潰すのは最大 3dB）。
  Tools/align_jp_voices.py は使わないでください（ファイル全体にリミッターがかかってエコーが大きく聞こえます）。
- 書き出したあと Tools/setup_jp_bundle.py で日本語用のバンドルに入れます。
"""
import glob
import os
import re
import struct
import subprocess
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from align_jp_voices import PEAK_LIMIT, lufs  # noqa: E402

SQUASH_MAX = 3.0  # dB the limiter may squash short voices to keep them as loud as the english ones
from import_rhre_voices import GAMES, RATE, code_offsets, offset_for  # noqa: E402

class Concat:
    """samples joined one after another, no gaps and no echo"""
    def __init__(self, samples):
        self.samples = samples


# Heaven Studio のファイル名: [(サンプルの位置, tick, 強さ 0〜127, 長さ tick), ...] か Concat([サンプルの位置, ...])
# 位置・tick・強さ・長さは Tools/gba_render_song.py --parts と曲データの読み取りで確かめた値
SPECS = {
    # まわるメイドインワリオ「おどるワリオ」の曲（566〜574番。カウントは 570番の強さ）
    # reverb: 曲ヘッダーのリバーブ（0x9E → 有効・レベル 30）。声と声の隙間を埋める残響
    # echo_curve: エコーの小ささ（強さの比の何乗か。ふつうは3）。「おぼえて」はエコーが音節の隙間をつなぐので原作寄りに
    'WarioDeMambo': {'code': 'WarioDeMambo', 'prefix': 'warioDeMambo', 'tempo': 140, 'reverb': 30,
                     'echo_curve': {'memorize': 1.5}, 'notes': {
        'four': [(0x593418, 0, 92, 6), (0x593418, 6, 52, 6), (0x593418, 12, 32, 6)],
        'three': [(0x593C48, 0, 92, 6), (0x593C48, 6, 52, 6), (0x593C48, 12, 32, 6)],
        'two': [(0x5945FC, 0, 100, 6), (0x5945FC, 6, 60, 6), (0x5945FC, 12, 40, 6)],
        'one': [(0x594C70, 0, 100, 6), (0x594C70, 6, 60, 6), (0x594C70, 12, 40, 6)],
        # 「おぼえて」は曲データでは音節がばらばらに並ぶので、サンプルを隙間なくそのままつなぐ（エコーなし）
        'memorize': Concat([0x595108, 0x5953E4, 0x595C44]),
        'memorize1': Concat([0x595108]),
        'memorize2': Concat([0x5953E4]),
        'memorize3': Concat([0x595C44]),
        # 始まりのアナウンス（565番。曲の頭から 36 tick＝1.5拍後。Heaven Studio は 0.5拍後に鳴らすので
        # voice_timing.json で 1拍遅らせる）
        'ladiesandgentlemen': [(0x5851B4, 0, 92, 48), (0x5851B4, 12, 52, 48), (0x5851B4, 24, 32, 48),
                               (0x587934, 48, 92, 24), (0x587934, 60, 52, 24), (0x587934, 72, 32, 24),
                               (0x58898C, 72, 92, 24), (0x58898C, 84, 52, 24), (0x58898C, 96, 32, 24),
                               (0x58A5B8, 99, 92, 42), (0x58A5B8, 111, 52, 42), (0x58A5B8, 123, 32, 42)],
        # 204 tick＝8.5拍後（Heaven Studio と同じ）
        'wariodemambo': [(0x58CB44, 0, 92, 72), (0x58CB44, 12, 52, 72), (0x58CB44, 24, 32, 72),
                         (0x58F6D0, 72, 92, 72), (0x58F6D0, 84, 52, 72), (0x58F6D0, 96, 32, 72)],
    }},
}


def sample(rom, addr, seconds=None):
    """sample → RATE. A looping sample (an instrument held by the note) is looped until `seconds`."""
    flags, pitch, loop, size = struct.unpack_from('<IIII', rom, addr)
    pcm = np.frombuffer(rom[addr + 16:addr + 16 + size], np.int8).astype(np.float32) / 128
    if flags & 0x40000000 and seconds and size > loop:
        rate = pitch / 1024
        need = int(seconds * rate)
        if need > size:
            reps = (need - size) // (size - loop) + 1
            pcm = np.concatenate([pcm] + [pcm[loop:]] * reps)[:need]
    rate = pitch / 1024
    # to RATE (plays at the sample's own rate: these voices are played at their base key)
    n = int(len(pcm) * RATE / rate)
    return np.interp(np.arange(n) * rate / RATE, np.arange(len(pcm)), pcm).astype(np.float32)


def build_concat(rom, concat, lead):
    joined = np.concatenate([sample(rom, a) for a in concat.samples])
    voice = np.zeros((int(lead * RATE) + len(joined), 2), np.float32)
    voice[int(lead * RATE):] = joined[:, None]
    return voice, np.zeros_like(voice)


def build(rom, notes, tempo, lead, curve=3):
    if isinstance(notes, Concat):
        return build_concat(rom, notes, lead)
    """stereo (voice, echo). The first note of each sample is the voice (centre); its repeats are the echo,
    made much quieter and spread left / right like the english recordings."""
    tick = 60 / tempo / 24
    end = max(t * tick + len(sample(rom, addr, length * tick)) / RATE for addr, t, _, length in notes) + lead
    voice = np.zeros((int(end * RATE) + 1, 2), np.float32)
    echo = np.zeros_like(voice)
    first = {}
    for addr, t, vel, length in sorted(notes, key=lambda n: n[1]):
        n = sum(1 for k in first if k[0] == addr)
        first[(addr, t)] = vel
        main_vel = first[min((k for k in first if k[0] == addr), key=lambda k: k[1])]
        # echo: (its velocity / the voice's velocity) to the power `curve`; 3 makes 92 → 52 → 32 -15dB, -27dB
        level = vel / 127 if n == 0 else main_vel / 127 * (vel / main_vel) ** curve
        # the game cuts each note at its length (release 0), but with the echo this quiet the cut is audible:
        # let the voice play to the end of the sample
        y = sample(rom, addr, length * tick) * level
        fade = min(len(y), int(0.01 * RATE))  # short fade so the sample (or the end of a held loop) does not click
        y[-fade:] *= np.linspace(1, 0, fade)
        pan = 0.0 if n == 0 else (-0.7 if n % 2 else 0.7)
        left, right = np.cos((pan + 1) * np.pi / 4), np.sin((pan + 1) * np.pi / 4)
        a = int((lead + t * tick) * RATE)
        out = voice if n == 0 else echo
        out[a:a + len(y), 0] += y * left * np.sqrt(2)
        out[a:a + len(y), 1] += y * right * np.sqrt(2)
    return voice, echo


def gba_reverb(audio, level):
    """the GBA sound engine's reverb (song header byte & 0x7F): the mix of about 6 and 7 frames ago
    (13379Hz mode, 224 samples a frame) is fed back into both channels at level/128"""
    frame = 224 / 13379
    d1, d2 = int(6 * frame * RATE), int(7 * frame * RATE)
    mono = audio.mean(axis=1)
    out = np.concatenate([mono, np.zeros(int(1.5 * RATE), np.float32)])
    wet = np.zeros_like(out)
    g = level / 128 / 2
    for i in range(d1, len(out), d1):  # block-wise feedback (each block only depends on earlier ones)
        j = min(i + d1, len(out))
        src1 = out[i - d1:j - d1] + wet[i - d1:j - d1]
        src2 = np.zeros(j - i, np.float32)
        lo = max(0, i - d2)
        seg = out[lo:j - d2] + wet[lo:j - d2]
        src2[len(src2) - len(seg):] = seg
        wet[i:j] = g * (src1 + src2)
    tail = np.stack([wet, wet], axis=1)
    tail[:len(audio)] += audio
    # drop the silent end
    loud = np.nonzero(np.abs(tail).max(axis=1) > 10 ** (-60 / 20))[0]
    return tail[:loud[-1] + 1 if len(loud) else len(audio)]


def ffmpeg_filter(audio, af):
    data = subprocess.run(['ffmpeg', '-v', 'error', '-f', 'f32le', '-ar', str(RATE), '-ac', '2', '-i', '-', '-af', af,
                           '-f', 'f32le', '-'], input=audio.astype(np.float32).tobytes(), capture_output=True, check=True).stdout
    return np.frombuffer(data, np.float32).reshape(-1, 2)[:len(audio)]


def loudness(audio):
    out = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-f', 'f32le', '-ar', str(RATE), '-ac', '2', '-i', '-',
                          '-af', 'aresample=48000,apad=pad_dur=0.5,ebur128', '-f', 'null', '-'],
                         input=audio.astype(np.float32).tobytes(), capture_output=True).stderr.decode()
    m = re.findall(r'^\s+I:\s+(-?[\d.]+) LUFS', out, re.M)
    return float(m[-1]) if m else -70.0


def match_english(voice, echo, target, reverb=0):
    """as loud as the english file. The limiter goes over the whole mix (voice, echo, reverb) so nothing
    clips when they are added up; it is a slow, gentle one and may squash at most SQUASH_MAX dB."""
    dry = voice + echo
    if reverb:
        dry = gba_reverb(dry, reverb)
    peak = np.abs(dry).max() or 1
    safe = PEAK_LIMIT - 20 * np.log10(peak)  # gain that needs no limiter
    cap = safe + SQUASH_MAX
    gain = min(target - loudness(dry), cap)
    for _ in range(6):
        mix = dry * 10 ** (gain / 20)
        if gain > safe:
            mix = ffmpeg_filter(mix, f'alimiter=limit={10 ** (PEAK_LIMIT / 20):.3f}:attack=5:release=80:level=0:latency=1')
        lost = target - loudness(mix)
        if abs(lost) < 0.3 or (lost > 0 and gain >= cap):
            break
        gain = min(gain + lost, cap)
    return mix, gain, max(0.0, gain - safe)


def save_stereo(path, audio):
    tmp = path + '.tmp.wav'
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-f', 'f32le', '-ar', str(RATE), '-ac', '2', '-i', '-',
                    '-c:a', 'pcm_s16le', tmp], input=audio.astype(np.float32).tobytes(), check=True)
    os.replace(tmp, path)


def main(rom_path, game):
    rom = open(rom_path, 'rb').read()
    spec = SPECS[game]
    offsets = code_offsets(spec['code'], spec['prefix'])
    sounds = os.path.join(GAMES, game, 'Sounds')
    dest = os.path.join(sounds, 'jp')
    os.makedirs(dest, exist_ok=True)
    gains = {}
    for name, notes in spec['notes'].items():
        lead = offset_for(offsets, name)
        curve = next((c for k, c in spec.get('echo_curve', {}).items() if name.startswith(k)), 3)
        voice, echo = build(rom, notes, spec['tempo'], lead, curve)
        english = [f for f in glob.glob(os.path.join(sounds, name + '.*')) if not f.endswith('.meta')]
        # syllables cut out of a longer voice get the same gain as the whole voice
        whole = next((g for k, g in gains.items() if name.startswith(k)), None)
        if whole is not None:
            gain, squash = whole, 0.0
            mix = voice + echo
            if spec.get('reverb'):
                mix = gba_reverb(mix, spec['reverb'])
            mix = mix * 10 ** (gain / 20)
            if np.abs(mix).max() > 10 ** (PEAK_LIMIT / 20):
                mix = ffmpeg_filter(mix, f'alimiter=limit={10 ** (PEAK_LIMIT / 20):.3f}:attack=5:release=80:level=0:latency=1')
        else:
            mix, gain, squash = match_english(voice, echo, lufs(english[0]), spec.get('reverb', 0))
        gains[name] = gain
        path = os.path.join(dest, name + '.wav')
        save_stereo(path, mix)
        print(f'{name:20} 頭の無音 {lead:.3f}s  音量 {gain:+5.1f}dB（リミッター {squash:.1f}dB）')


if __name__ == '__main__':
    main(*sys.argv[1:])
