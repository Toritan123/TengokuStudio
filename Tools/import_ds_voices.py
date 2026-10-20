#!/usr/bin/env python3
"""
DS 版（リズム天国ゴールド）の効果音データから日本語の声を書き出して、Heaven Studio のファイル名で Sounds/jp/ に置きます。
カエルごとに同じ声を違う高さで鳴らすゲーム（ケロケロダンス）のように、高さ・速さを ROM のとおりにしたいとき用です。

  Tools/.venv/bin/python Tools/import_ds_voices.py <リズム天国ゴールド.nds> FrogHop

- 声の高さは日本語版の効果音データのとおり（英語版とは高さが違うことがある）。
- 英語の声は共通バンドルから言語別バンドル（en）に移します。
- 音量はゲームの効果音データ（シーケンスの音量・音の強さ）のとおりに書き出すので、声どうしのバランスは原作のまま。
- 書き出したあと Tools/align_jp_voices.py <ゲーム> で頭の位置とゲーム全体の音量を英語版にそろえ
  （--per-file は使わない。英語版のファイルは声ごとの音量がばらばらなので、それを写してしまう）、
  Tools/setup_jp_bundle.py <ゲーム> で日本語用のバンドルに入れます。
"""
import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from ds_render_seq import load, render, save, sequence_notes  # noqa: E402
from import_rhre_voices import GAMES, move_to_locale  # noqa: E402

# Heaven Studio のファイル名: (シーケンス名, 音色番号, 高さの変更（半音）)
#   音色: 2 はい / 7 は / 8 はーい / 9 く / 10 る / 11 りん / 12〜15 カウント1〜4
#   _PLAYER_CUSTOM・_EXTRAS_CUSTOM は Heaven Studio が作ったもの（英語版はカウントを 5・10 半音下げたもの）
FROG = {}
for role in 'EPT':
    FROG.update({
        f'SE_NTR_FROG_EN_{role}_HAI': (f'SE_KERO_{role}_HAI', 2, 0),
        f'SE_NTR_FROG_EN_{role}_HA': (f'SE_KERO_{role}_HA', 7, 0),
        f'SE_NTR_FROG_EN_{role}_HAAI': (f'SE_KERO_{role}_HAAI', 8, 0),
        f'SE_NTR_FROG_EN_{role}_KURU_1': (f'SE_KERO_{role}_KURU', 9, 0),
        f'SE_NTR_FROG_EN_{role}_KURU_2': (f'SE_KERO_{role}_KURU', 10, 0),
        f'SE_NTR_FROG_EN_{role}_LIN': (f'SE_KERO_{role}_LIN', 11, 0),
    })
for n in range(1, 5):
    FROG[f'SE_NTR_FROG_EN_COUNT{n}'] = (f'SE_KERO_COUNT{n}', 11 + n, 0)
    FROG[f'SE_NTR_FROG_EN_COUNT{n}_PLAYER_CUSTOM'] = (f'SE_KERO_COUNT{n}', 11 + n, -5)
    FROG[f'SE_NTR_FROG_EN_COUNT{n}_EXTRAS_CUSTOM'] = (f'SE_KERO_COUNT{n}', 11 + n, -10)

SPECS = {
    'FrogHop': {'archive': 'SEQARC_KERO', 'bundle': 'ntrfroghop', 'files': FROG},
}


def main(rom_path, game):
    spec = SPECS[game]
    sdat, ssar = load(rom_path, spec['archive'])
    sequences = dict(ssar.sequences)
    sounds = os.path.join(GAMES, game, 'Sounds')
    dest = os.path.join(sounds, 'jp')
    os.makedirs(dest, exist_ok=True)
    for hs_name, (seq_name, inst, shift) in spec['files'].items():
        seq = sequences[seq_name]
        bank = sdat.banks[seq.bankID][1]
        waves = [sdat.waveArchives[w][1] for w in bank.waveArchiveIDs]
        notes = [(t, i, key + shift, vel, length) for t, i, key, vel, length in sequence_notes(ssar, seq) if i == inst]
        save(os.path.join(dest, hs_name + '.wav'), render(notes, bank, waves, volume=seq.volume))
        english = os.path.join(sounds, hs_name + '.wav')
        if os.path.exists(english):
            move_to_locale(english, spec['bundle'])
        print(f'{hs_name:40} ← {seq_name} 音色{inst} キー{notes[0][2]}')


if __name__ == '__main__':
    main(*sys.argv[1:])
