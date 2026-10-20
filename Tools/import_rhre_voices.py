#!/usr/bin/env python3
"""
RHRE3 のサウンドデータベース（~/.rhre3/sfx/master/games）から日本語音声を取り込み、
Heaven Studio のファイル名で Assets/Bundled/Games/<ゲーム>/Sounds/jp/ に書き出します。

  ~/.../venv/bin/python Tools/import_rhre_voices.py TapTroupe SeeSaw FlipperFlop

- RHRE のファイル名と Heaven Studio のファイル名の対応は、下の MAPPINGS に書きます。
- RHRE では声と効果音がくっついていることがあります。英語版で
  「RHRE のファイル − Heaven Studio のファイル」＝ 効果音 が分かる場合は、それを日本語版から差し引きます
  （効果音が日本語版でも同じ位置にあるときだけ。差し引いて音が小さくならなければ何もしません）。
- 声のあとの無音や余韻は切り落とします。
- RHRE のファイルは「ファイルの頭＝拍」で作られています。Heaven Studio のコードは英語版の録音に合わせて
  声を少し早く鳴らしている（offset）ので、その分の無音を日本語版の頭に足して、拍ぴったりに鳴るようにします。
- 書き出したあとは Tools/align_jp_voices.py --no-align で音量だけ英語版にそろえてください
  （頭の位置は上で合わせているので、英語版の録音に合わせ直さない）。
numpy が必要です。
"""
import glob
import os
import re
import subprocess
import sys

import numpy as np

PROJECT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
GAMES = os.path.join(PROJECT, 'Assets', 'Bundled', 'Games')
RHRE = os.path.expanduser('~/.rhre3/sfx/master/games')
RATE = 44100

# Heaven Studio のファイル名: 日本語版の RHRE ファイル名（拡張子なし）
#   None                 … 無音（英語版にしかない区切り）
#   (ファイル名, 0.6)     … 声の長さのその割合で切る（英語版も途中で切っている声）
#   Slice(ファイル名, n, i) … 1つのファイルに n 個の音節がつながっているとき、i 番目（0から）を切り出す
class Slice:
    def __init__(self, source, count, index):
        self.source, self.count, self.index = source, count, index
MAPPINGS = {
    'TapTroupe': {
        'rhre': 'tapTroupe', 'code': 'tapTroupe',
        'files': {
            'bamvoice1': 'tapBom1', 'bamvoice2': 'tapBom2', 'startTap': 'start', 'tapAnd': 'tapAnd',
            'tapReady1': 'tapReady1', 'tapReady2': 'tapReady2', 'tapvoice1': 'tapEnd1', 'tapvoice2': 'tapEnd2',
        },
    },
    'SeeSaw': {
        'rhre': 'seesaw', 'code': 'seeSaw',
        'files': {
            'otherVoiceLong1': 'seeLong1', 'otherVoiceLong2': 'seeLong2',
            'otherVoiceShort1': 'seeShort1', 'otherVoiceShort2': 'seeShort2',
            'playerVoiceLong1': 'sawLong1', 'playerVoiceLong2': 'sawLong2',
            'playerVoiceShort1': 'sawShort1', 'playerVoiceShort2': 'sawShort2',
            'ow': 'smallMiss',
        },
    },
    'FlipperFlop': {
        'rhre': 'flipperFlop', 'code': 'flipperFlop',
        # 英語版の RHRE には声がないので、効果音の差し引きはしない
        'no_reference': True,
        'files': {
            # 日本語版のほめ言葉は英語版と言葉が違うので、5つを順に割り当てる
            'good': 'appreciation1', 'yes': 'appreciation2', 'goodjob': 'appreciation3',
            'nice': 'appreciation4', 'welldone': 'appreciation5',
            'thatsit1': 'thatsit1', 'thatsit2': 'thatsit2',
            # 「ずっとい はじめ」は5つ。日本語の6つ目は隊員の返事で、共通の attention7 と同じ音なので取り込まない。
            # 英語の6つ目に当たる声はないので無音にする（voice_timing.json で鳴らさない）
            **{f'attention{i}': f'attention{i}' for i in range(1, 6)},
            'attention6': None,
            **{f'flipperRollCount{i}': f'flipperRollCount{i}' for i in range(1, 11)},
            'flipperRollCount7B': 'flipperRollCount7',
            'flipperRollCountA': 'flipperRollCountA', 'flipperRollCountB': 'flipperRollCountB',
            'flipperRollCountC': 'flipperRollCountC', 'flipperRollCountS': 'flipperRollCountC',
            'flipperRollCountNow': 'flipperRollCountPractice',
            **{f'flopCount{n}{v}': f'rollCount{n}{v}' for n, vs in [(1, 'BC'), (2, 'BC'), (3, 'B'), (4, 'B')] for v in [''] + list(vs)},
            # 失敗したときの数え声は日本語版にないので、普通の数え声で代用する
            **{f'flopCountFail{n}': f'rollCount{n}' for n in range(1, 5)},
            **{f'uh{i}': f'hoi{i}' for i in range(1, 4)},
            # 英語版の uhfail は途中で切れた「uh」なので、同じように切る
            **{f'uhfail{i}': (f'hoi{i}', 0.6) for i in range(1, 4)},
        },
    },
    'CheerReaders': {
        'rhre': 'cheerReaders', 'code': 'cheerReaders',
        'files': {
            # 「ワン・ツー・スリー」
            **{f'oneTwoThreeS{i}': f'oneTwoThree{i}_1' for i in range(1, 4)},
            'onegirls': 'oneTwoThreeGirls1_1', 'twogirls': 'oneTwoThreeGirls2_1', 'threegirls': 'oneTwoThreeGirls3_1',
            # It's up to you!（0, 0.5, 0.75, 1.5, 2拍）→「ヘイ・ユー・キャン・ドゥ」（0, 0.75, 1.5, 2拍）
            'itsUpToYouS1': 'heyYouCanDo1', 'itsUpToYouS2': None, 'itsUpToYouS3': 'heyYouCanDo2',
            'itsUpToYouS4': 'heyYouCanDo3', 'itsUpToYouS5': 'heyYouCanDo4',
            'itgirls': 'heyYouCanDoGirls1_1', 'sgirls': None, 'upgirls': 'heyYouCanDoGirls2_1',
            'togirls': 'heyYouCanDoGirls3_1', 'yougirls': 'heyYouCanDoGirls4_1',
            # Rah rah sis boom ba boom!（6つ）→「ネバー・ギブ・アップ」（6つ、同じリズム）
            **{f'rahRahSisBoomBaBoomS{i}': f'neverGiveUp{i}' for i in range(1, 6)},
            'rahRahSisBoomBaBoomS6': 'neverGiveUp6_1',
            **{f'rahRahSisBoomBaBoomgirls{i}': f'neverGiveUpGirls{i}_1' for i in range(1, 7)},
            # Let's go read a buncha books!（0.25拍ごとに9つ）→「レッツ・エヴィバディ・ゴー」（0.25, 0.75, 1, 1.25, 1.5, 1.75, 2拍）
            'bunchaBooksS1': 'letsEverybodyGo1', 'bunchaBooksS2': None,
            **{f'bunchaBooksS{i}': f'letsEverybodyGo{i - 1}' for i in range(3, 9)},
            'bunchaBooksS9': None,
            # 女の子たちの「ツエヴィバディ」は1つのファイルなので、5つに切って 0.75〜1.75拍に置く
            'bunchaBooksgirls1': 'letsEveryBodyGoGirls1_1', 'bunchaBooksgirls2': None,
            **{f'bunchaBooksgirls{i}': Slice('letsEveryBodyGoGirls2_1', 5, i - 3) for i in range(3, 8)},
            'bunchaBooksgirls8': 'letsEveryBodyGoGirls3_1', 'bunchaBooksgirls9': None,
            # OK, it's on!（0, 1, 2, 2.75, 3拍）→「オーケー・ドント・マインド」（0, 1, 2, 3拍）
            **{f'okItsOnS{i}': f'okayDontMind{i}_1' for i in range(1, 4)},
            'okItsOnS4': None, 'okItsOnS5': 'okayDontMind4_1',
            **{f'okItsOngirls{i}': f'okayDontMindGirls{i}_1' for i in range(1, 4)},
            'okItsOngirls4': None, 'okItsOngirls5': 'okayDontMindGirls4_1',
            'yayS': 'yay1', 'yayGirls': 'yayGirls1', 'yay': 'yayAll1',
        },
    },
    'Ringside': {
        'rhre': 'ringside', 'code': 'ringside',
        # Heaven Studio の英語版は RHRE の英語版と別の録音なので、効果音の差し引きはしない
        'no_reference': True,
        # コードの中で変数になっている早め鳴らし（youBeat：1・2番目は0.081秒、3番目は0.027秒）
        'offsets': {**{f'bigguy_var{q}_{k}': 0.081 for q in (1, 2) for k in (1, 2)},
                    **{f'bigguy_var3_{k}': 0.027 for k in (1, 2)}},
        'files': {
            # 「ウッバ ダッバ……ザット トゥルー？」：wubba-1, wubba-2, dubba-1〜4, that, true（RHRE と同じ拍）
            **{f'wubdub_var{q}_{k}': f'wubba{q}-{k}' for q in range(1, 4) for k in (1, 2)},
            **{f'wubdub_var{q}_{k + 2}': f'dubba{q}-{k}' for q in range(1, 4) for k in range(1, 5)},
            **{f'wubdub_var{q}_7': f'that{q}' for q in range(1, 4)},
            **{f'wubdub_var{q}_8': f'true{q}' for q in range(1, 4)},
            # 「ユー ゴー ビッグ ガイ」：woah, you, go, big, guy
            **{f'bigguy_var{q}_{k}': f'{w}{q}' for q in range(1, 4) for k, w in enumerate(['woah', 'you', 'go', 'big', 'guy'], 1)},
            # 「ポーズ フォー ザ ファンズ」：pose, (英語だけの区切り), for, the, fans, (英語だけの区切り)
            **{f'pose_var{p}_{k}': (f'{w}{p}' if w else None) for p in (1, 2) for k, w in enumerate(['pose', None, 'for', 'the', 'fans', None], 1)},
            'pose_and': 'poseAnd',
        },
    },

    # ---- 声が共通バンドルに入っていたゲーム（'split': 英語の声を <bundle>/locale の en に移してから日本語版を足す）
    'Airboarder': {'rhre': 'airboarder', 'code': 'airboarder', 'split': 'ntrairboarder',
                   'files': {'crouchvox': 'crouch', 'jumpvox': 'jump', 'start2': 'start2'}},
    'CatchOfTheDay': {'rhre': 'catchOfTheDay', 'code': 'catchOfTheDay', 'split': 'rvlfishing',
                      'files': {f'threefish{i}': f'threefish{i}' for i in range(1, 6)}},
    'FigureFighter': {'rhre': 'figureFighter', 'code': 'figureFighter', 'split': 'rvlmuscle',
                      'files': {n: n for n in ['and', 'and2', 'fastOneTwo1', 'fastOneTwo2', 'go1', 'go2', 'go3', 'jab', 'oneTwo1', 'oneTwo2']}},
    'BasketballGirls': {'rhre': 'basketballGirls', 'code': 'basketballGirls', 'split': 'rvlbasket',
                        'files': {'voice': 'voiceJapanese'}},
    # Here we go! →「どっこいしょ」
    'DogNinja': {'rhre': 'dogNinja', 'code': 'dogNinja', 'split': 'ntrninja',
                 'files': {'here': 'dokkoisho1', 'we': 'dokkoisho2', 'go': 'dokkoisho3'}},
    # Fill 'er up! →「はい」（最後の拍だけ）
    'Fillbots': {'rhre': 'fillbots', 'code': 'fillbots', 'split': 'ntrfillbots',
                 'files': {'fillErUp1': None, 'fillErUp2': None, 'fillErUp3': 'hai'}},
    # Double up! →「いくぜ」（リズムは voice_timing.json）
    'PackingPests': {'rhre': 'packingPests', 'code': 'packingPests', 'split': 'rvlsorting',
                     'files': {'SE_SHIWAKE_EN_VOICE_READY': 'ikuze', 'SE_SHIWAKE_EN_VOICE_READY1': 'ikuzeSplit1',
                               'SE_SHIWAKE_EN_VOICE_READY2': 'ikuzeSplit2', 'SE_SHIWAKE_EN_VOICE_READY3': 'ikuzeSplit3'}},
    'ClapTrap': {'rhre': 'clapTrap', 'code': 'clapTrap', 'split': 'rvlshirawa',
                 'files': {'SE_SIRAHA_EN_CATCH_OSII_B': 'barely1', 'SE_SIRAHA_EN_CATCH_SUCCESS_A': 'goodClap4',
                           'SE_SIRAHA_EN_CATCH_SUCCESS_B': 'goodClap2', 'SE_SIRAHA_EN_CATCH_SUCCESS_C': 'goodClap1',
                           'SE_SIRAHA_EN_CATCH_SUCCESS_D': 'goodClap3', 'barely1': 'barely1', 'barely2': 'barely2',
                           'clapAce': 'aceClap2', 'clapGood': 'goodClap2', 'goodClap2': 'goodClap2',
                           'goodClap3': 'goodClap3', 'goodClap4': 'goodClap4'}},
    # Peck your beak! / Stretch out your neck!
    'BlueBirds': {'rhre': 'blueBirds', 'code': 'blueBirds', 'split': 'ntrbluebirds',
                  'files': {'peck': 'peckyourbeak1', 'ur': 'peckyourbeak2', 'beak': 'peckyourbeak3',
                            'stretch': 'stretchoutyourneck1', 'out': 'stretchoutyourneck2',
                            'your': 'stretchoutyourneck3', 'neck': 'stretchoutyourneck4'}},
    'BossaNova': {'rhre': 'bossaNova', 'code': 'bossaNova', 'split': 'rvlreceive',
                  'files': {
                      **{f'SE_BOSSA_EN_{n}': v for n, v in {
                          4: 'maleNormalA1', 5: 'maleNormalA5', 6: 'maleAngry6', 7: 'maleNormalA4', 8: 'maleNormalB1',
                          9: 'maleNormalB2', 10: 'maleNormalB3', 11: 'maleNormalB4', 12: 'maleNormalABSpin',
                          13: 'maleEnergeticC1', 14: 'maleEnergeticC2', 15: 'maleEnergeticC6', 16: 'maleEnergeticC4',
                          17: 'maleEnergeticA1', 18: 'maleEnergeticA2', 19: 'maleEnergeticA3', 20: 'maleEnergeticA4',
                          21: 'maleEnergeticB1', 22: 'maleEnergeticC5', 23: 'maleEnergeticB3', 24: 'maleEnergeticB4',
                          25: 'maleEnergeticASpin', 26: 'maleEnergeticBSpin', 27: 'maleEnergeticCSpin',
                          28: 'femaleNormalSpinA2', 29: 'femaleNormalB2', 30: 'femaleNormalSpinB2', 31: 'femaleNormalA2',
                          34: 'femaleNormalAB4', 36: 'femaleAll1', 37: 'femaleAll3', 38: 'femaleNormalSpinAB4',
                          # 元気な声は英語版に A/B しかないが、日本語版は A/B/C ある（ゲームの voiceVariant 0/1/2）
                          39: 'femaleEnergeticA2', 40: 'femaleEnergeticA4', 41: 'femaleEnergeticB2', 42: 'femaleEnergeticB4',
                          43: 'femaleEnergeticC2', 44: 'femaleEnergeticC4', 45: 'femaleEnergeticSpinABC4',
                          47: 'maleAngry1', 48: 'maleAngry4', 49: 'maleAngryBSpin', 50: 'maleAngryASpin',
                          51: 'femaleAngryAB2', 52: 'femaleAngryA4', 53: 'femaleAngryB4', 54: 'femaleAngrySpinA4',
                          55: 'femaleAngrySpinB4',
                          # 回転するときの声：英語 Mmm/Mhm/Ahh/Ooh → 日本語 Mmm/Umm/Oh/Ugh
                          60: 'maleTurnMmm', 61: 'maleTurnUmm', 62: 'maleTurnOh', 63: 'maleTurnUgh',
                          64: 'femaleTurnMmm', 65: 'femaleTurnMhm', 66: 'femaleTurnLoudMmm', 67: 'femaleTurnNha',
                      }.items()},
                  }},
    # だいスキRAP：Heaven Studio のファイル名がもともと日本語版の言葉（DAISUKI/MAJI/HONTO/SUKINANDA）。
    # Woman = MC アドレ、Left = rapper2、Right = rapper1。日本語版で1音節多いところは jp_ で始まるファイルを
    # voice_timing.json の extra で鳴らす
    'LoveRap': {'rhre': 'loveRap', 'code': 'loveRap', 'split': 'rvlrap', 'no_reference': True,
                'files': {
                    **{f'SE_RAP_EN_{who}_DAISUKI_{i}': f'{r}daisuki{i}' for who, r in [('WOMAN', 'mcadore'), ('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 4)},
                    **{f'SE_RAP_EN_{who}_MAJI_{i}': f'{r}majidaisuki{i}' for who, r in [('WOMAN', 'mcadore'), ('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 6)},
                    **{f'SE_RAP_EN_{who}_MAJI_OFFBEAT_{i}': f'{r}majidaisukialt{i}' for who, r in [('WOMAN', 'mcadore'), ('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 6)},
                    **{f'SE_RAP_EN_{who}_HONTO_{i}': f'{r}honto{i}' for who, r in [('WOMAN', 'mcadore'), ('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 3)},
                    **{f'SE_RAP_EN_{who}_HONTO_OFFBEAT_{i}': f'{r}hontolong{i}' for who, r in [('WOMAN', 'mcadore'), ('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 4)},
                    **{f'SE_RAP_EN_WOMAN_SUKINANDA_OFFBEAT_{i}': f'mcadoresukinanda{i}' for i in range(1, 5)},
                    **{f'SE_RAP_EN_{who}_SUKINANDA_{i}': f'{r}sukinanda{i}' for who, r in [('MAN_LEFT', 'rapper2'), ('MAN_RIGHT', 'rapper1')] for i in range(1, 5)},
                    'SE_RAP_EN_MAN_RIGHT_SUKINANDA_EXTRA_4': 'rapper1sukinanda4',
                    # 日本語版だけにある音節
                    'jp_mcadoresukinanda5': 'mcadoresukinanda5',
                    'jp_rapper1majidaisukialt6': 'rapper1majidaisukialt6',
                    'jp_rapper2majidaisukialt6': 'rapper2majidaisukialt6',
                }},
}


def load(path):
    data = subprocess.run(['ffmpeg', '-v', 'error', '-i', path, '-ac', '1', '-ar', str(RATE), '-f', 'f32le', '-'],
                          capture_output=True, check=True).stdout
    return np.frombuffer(data, dtype=np.float32).copy()


def save(path, audio):
    tmp = path + '.tmp.wav'
    subprocess.run(['ffmpeg', '-v', 'error', '-y', '-f', 'f32le', '-ar', str(RATE), '-ac', '1', '-i', '-',
                    '-c:a', 'pcm_s16le', tmp], input=audio.astype(np.float32).tobytes(), check=True)
    os.replace(tmp, path)


def find(game_dir, name):
    hits = [f for f in glob.glob(os.path.join(game_dir, '**', name + '.*'), recursive=True)
            if not f.endswith('.meta') and '/jp/' not in f]
    return hits[0] if hits else None


def best_offset(short, long):
    """where `short` sits inside `long`, and how well it matches (0..1)"""
    if len(short) > len(long):
        return 0, 0.0
    n = 1 << int(np.ceil(np.log2(len(short) + len(long))))
    c = np.fft.irfft(np.fft.rfft(long, n) * np.conj(np.fft.rfft(short, n)), n)[:len(long) - len(short) + 1]
    k = int(np.argmax(np.abs(c)))
    seg = long[k:k + len(short)]
    return k, float(abs(c[k]) / (np.linalg.norm(short) * np.linalg.norm(seg) + 1e-9))


def envelope_db(x, w=441):
    frames = len(x) // w + 1
    padded = np.pad(x, (0, frames * w - len(x)))
    rms = np.sqrt(np.mean(padded.reshape(frames, w) ** 2, axis=1)) + 1e-9
    return 20 * np.log10(rms / (np.max(np.abs(x)) + 1e-9)), w


def trim_tail(x, threshold=-42, tail=0.03):
    env, w = envelope_db(x)
    loud = np.nonzero(env > threshold)[0]
    if len(loud) == 0:
        return x
    end = min(len(x), (loud[-1] + 1) * w + int(tail * RATE))
    out = x[:end].copy()
    fade = min(len(out), int(0.015 * RATE))
    out[-fade:] *= np.linspace(1, 0, fade)
    return out


def split_syllables(x, count):
    """cut points between `count` evenly spaced syllables: the quietest spot near each expected boundary"""
    env, w = envelope_db(x, 220)
    loud = np.nonzero(env > -20)[0]
    start, end = loud[0], loud[-1]
    step = (end - start) / count
    cuts = [start]
    for k in range(1, count):
        expected = start + step * k
        lo, hi = int(expected - step * 0.4), int(expected + step * 0.4)
        cuts.append(lo + int(np.argmin(env[lo:hi + 1])))
    cuts.append(len(env))
    return [c * w for c in cuts]


def remove_sfx(hs_en, rhre_en, rhre_ja):
    """subtract the sound effect that the english RHRE file has on top of the voice, if japanese has it too"""
    k, corr = best_offset(hs_en, rhre_en)
    if corr > 0.97:
        return rhre_ja, 'そのまま'
    # the english voice inside the RHRE file, scaled to match
    seg = rhre_en[k:k + len(hs_en)]
    hs_en = hs_en[:len(seg)]
    scale = float(np.dot(seg, hs_en) / (np.dot(hs_en, hs_en) + 1e-9))
    sfx = rhre_en.copy()
    sfx[k:k + len(hs_en)] -= scale * hs_en
    sfx = np.pad(sfx, (0, max(0, len(rhre_ja) - len(sfx))))[:len(rhre_ja)]
    # the effect may sit a few ms differently in the japanese file
    best = (np.sum(rhre_ja ** 2), 0)
    for shift in range(-441, 442, 11):
        moved = np.roll(sfx, shift)
        best = min(best, (np.sum((rhre_ja - moved) ** 2), shift))
    energy, shift = best
    reduction = 10 * np.log10(np.sum(rhre_ja ** 2) / (energy + 1e-9))
    if reduction < 1.0:
        return rhre_ja, f'効果音は共通でない（一致 {corr:.2f}）'
    return rhre_ja - np.roll(sfx, shift), f'効果音を差し引き（一致 {corr:.2f}、{reduction:.1f}dB 減）'


def move_to_locale(path, bundle):
    """put an english voice that was in the game's common bundle into its english locale bundle"""
    meta = path + '.meta'
    t = open(meta).read()
    t = re.sub(r'assetBundleName:.*', f'assetBundleName: {bundle}/locale', t)
    t = re.sub(r'assetBundleVariant:.*', 'assetBundleVariant: en', t)
    open(meta, 'w').write(t)


def code_offsets(game, prefix):
    """seconds each sound is played early in the game's code (MultiSound.Sound(..., offset))"""
    offsets = {}
    for path in glob.glob(os.path.join(PROJECT, 'Assets', 'Scripts', 'Games', game, '*.cs')):
        src = open(path, encoding='utf-8').read()
        for m in re.finditer(r'(?:MultiSound\.Sound|PlayOneShotGame)\(\s*\$?"%s/([^"]+)"([^;\n]*)' % prefix, src):
            name = m.group(1).split('/')[-1]
            mo = re.search(r'false,\s*([0-9.]+)f?\s*\)', m.group(2)) or re.search(r'offset:\s*([0-9.]+)f?', m.group(2))
            offsets.setdefault(name, set()).add(float(mo.group(1)) if mo else 0.0)
    # a sound played with different offsets in different places gets the largest one
    return {name: max(v) for name, v in offsets.items()}


def offset_for(offsets, name):
    """offset of a sound, also matching names built in code like wubdub_var{currentQuestion}_1"""
    if name in offsets:
        return offsets[name]
    for pattern, value in offsets.items():
        if '{' in pattern and re.fullmatch(re.sub(r'\\\{[^}]*\\\}', r'\\w+', re.escape(pattern)), name):
            return value
    return 0.0


def process(game):
    spec = MAPPINGS[game]
    game_dir = os.path.join(GAMES, game, 'Sounds')
    jp_dir = os.path.join(game_dir, 'jp')
    os.makedirs(jp_dir, exist_ok=True)
    rhre_ja = os.path.join(RHRE, spec['rhre'] + 'Ja')
    rhre_en = os.path.join(RHRE, spec['rhre'] + 'En')
    offsets = {**code_offsets(game, spec['code']), **spec.get('offsets', {})}
    print(f'== {game}')
    for hs_name, source in spec['files'].items():
        # align_jp_voices.py rebuilds from its saved original, so replace that too
        for old in glob.glob(os.path.join(PROJECT, 'JapaneseVoiceSources', game, hs_name + '.*')):
            os.remove(old)
        cut, piece = None, None
        if isinstance(source, Slice):
            piece, source = source, source.source
        if source is None:
            save(os.path.join(jp_dir, hs_name + '.wav'), np.zeros(int(0.05 * RATE), dtype=np.float32))
            if spec.get('split') and find(game_dir, hs_name):
                move_to_locale(find(game_dir, hs_name), spec['split'])
            print(f'  {hs_name:24s} 無音')
            continue
        if isinstance(source, tuple):
            source, cut = source
        ja_path = os.path.join(rhre_ja, source + '.ogg')
        if not os.path.exists(ja_path):
            print(f'  {hs_name:24s} 日本語版に {source} がない')
            continue
        audio, note = load(ja_path), ''
        en_hs = find(game_dir, hs_name)
        en_rhre = None if spec.get('no_reference') else (glob.glob(os.path.join(rhre_en, '**', source + '.ogg'), recursive=True) or [None])[0]
        if en_hs and en_rhre:
            audio, note = remove_sfx(load(en_hs), load(en_rhre), audio)
        if piece:
            cuts = split_syllables(audio, piece.count)
            a, b = cuts[piece.index], cuts[piece.index + 1]
            # keep the very start of the first syllable, and let each piece ring slightly into the next
            a = 0 if piece.index == 0 else a
            audio = audio[a:min(len(audio), b + int(0.02 * RATE))].copy()
            fade = int(0.004 * RATE)
            if piece.index > 0:
                audio[:fade] *= np.linspace(0, 1, fade)
            note = (note + ' ' if note else '') + f'{piece.count}つに切った{piece.index + 1}つ目（{a / RATE:.3f}〜{b / RATE:.3f}秒）'
        audio = trim_tail(audio)
        if cut:
            audio = audio[:int(len(audio) * cut)]
            fade = int(0.03 * RATE)
            audio[-fade:] *= np.linspace(1, 0, fade)
            note = (note + ' ' if note else '') + f'{cut:.0%}の長さで切る'
        early = offset_for(offsets, hs_name)
        if early > 0:
            audio = np.concatenate([np.zeros(int(early * RATE), dtype=np.float32), audio])
            note = (note + ' ' if note else '') + f'頭に{early * 1000:.0f}msの無音'
        for old in glob.glob(os.path.join(jp_dir, hs_name + '.*')):
            if not old.endswith('.meta') and not old.endswith('.wav'):
                os.remove(old)
        save(os.path.join(jp_dir, hs_name + '.wav'), audio)
        if spec.get('split') and en_hs:
            move_to_locale(en_hs, spec['split'])
        print(f'  {hs_name:24s} ← {source:24s} {len(audio) / RATE:.2f}秒 {note}')


if __name__ == '__main__':
    for game in sys.argv[1:]:
        process(game)
