# 日本語音声の素材リスト

日本語版『リズム天国』シリーズから取り出した音声を、英語版と**同じファイル名**で `Sounds/jp/` フォルダに置いてください（形式は .ogg / .wav どちらでも可）。
置いてもらえれば、あとはこちらで日本語音声として組み込みます。

言語別の音声バンドルがあるゲームは、すべて日本語化しました。

## 共通バンドルに英語の声が入っていたゲーム
声だけを言語別バンドル（en）に移し、RHRE3 から日本語版を取り込みました（`import_rhre_voices.py` の `split`）:
エアボーダー、バスケット、ブルーバーズ、ボッサレシーブ、さかな釣り、しらはドール、犬忍者、マッスル人形、ロボット工場（「はい」）、しわけ（「いくぜ」）、だいスキRAP

エアボーダーは、効果音のついていない声だけのファイルに差し替え、RHRE3 になかった声（barelyvox・missvox・ready・start1・start3）も足してもらいました（全8ファイル。元のファイルは `JapaneseVoiceSources/Airboarder/`）。

（声の見分けは `Tools/find_voices.py`、対応表の下書きは `Tools/auto_map_voices.py`）

図書ガールズは、日本語版の掛け声（ワン・ツー・スリー／ヘイ・ユー・キャン・ドゥ／ネバー・ギブ・アップ／レッツ・エヴィバディ・ゴー／オーケー・ドント・マインド）を英語版の区切りに割り当てて取り込みました。
英語版にしかない区切りは無音、女の子たちの「ツエヴィバディ」は5つに切っています。

タップスター・シーソー・コロコロたんけん隊・図書ガールズは、RHRE3 のデータベースから `Tools/import_rhre_voices.py` で取り込みました。

英語版を仮に入れているもの、代用しているもの（日本語版があれば差し替え）:
- ロッカー: `Rockers/Sounds/jp/count1〜4.wav`（カウントの声）
- コロコロたんけん隊: `flopCountFail1〜4`（失敗したときの数え声。日本語版にないので普通の数え声で代用）、`uhfail1〜3`（「ホイ」を途中で切ったもの）

## ✅ カラテ家（karateman）— 17ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/KarateMan/Sounds/jp/`

`four.ogg`, `hit.ogg`, `hitAlt.wav`, `ko.ogg`, `one.ogg`, `pow.ogg`, `punchKick1.ogg`, `punchKick2.ogg`, `punchKick3.ogg`, `punchKick4.ogg`, `punchy1.ogg`, `punchy2.ogg`, `punchy3.ogg`, `punchy4.ogg`, `three.ogg`, `threeAlt.wav`, `two.ogg`

## ✅ ボックスショー（theDazzles）— 4ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/TheDazzles/Sounds/jp/`

`hold2.wav`, `hold3.wav`, `holdDS2.wav`, `holdDS3.wav`

## ✅ コーラスメン（gleeClub）— 4ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/GleeClub/Sounds/jp/`

`togetherEN-01.wav`, `togetherEN-02.wav`, `togetherEN-03.wav`, `togetherEN-04.wav`

## ✅ シンクロ（splashdown）— 3ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/Splashdown/Sounds/jp/`

`alleyOop1.ogg`（「そー」）, `alleyOop3.ogg`（「れ」）, `yeah.ogg`
※英語の「A・lley・oop」は3つ、日本語の「そーれ」は2つなので、日本語音声のときは `alleyOop2` を鳴らさないようにしてあります。

## ✅ ロッカー（rockers）— 2ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/Rockers/Sounds/jp/`

`Cmon.wav`, `LastOne.wav`

## ✅ ゐあひ斬り外伝（samuraiSliceNtr）— 4ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/SamuraiSliceNtr/Sounds/jp/`

`faster_normal.wav`, `faster_question.wav`, `faster_weird.wav`, `ntr_samurai_speedUp.png`

## ✅ しゅぎょう（munchyMonk）— 13ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/MunchyMonk/Sounds/jp/`

`one_1.wav`, `one_2.wav`, `one_go_test.wav`, `three_1.wav`, `three_2.wav`, `three_3.wav`, `three_4.wav`, `three_go_test.wav`, `two_1.wav`, `two_2.wav`, `two_3.wav`, `two_4.wav`, `two_go_test.wav`

## ✅ リフティング（spaceSoccer）— 4ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/SpaceSoccer/Sounds/jp/`

`highkicktoe1.ogg`, `highkicktoe2.ogg`, `highkicktoe3.ogg`, `kick.ogg`

## ✅ トンネル（tunnel）— 2ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/Tunnel/Sounds/jp/`

`one.ogg`, `two.ogg`

## ✅ バドミントン（airRally）— 36ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/AirRally/Sounds/jp/`

`baBumBumBum_Close1.wav`, `baBumBumBum_Close2.wav`, `baBumBumBum_Close3.wav`, `baBumBumBum_Close4.wav`, `baBumBumBum_Far1.ogg`, `baBumBumBum_Far2.ogg`, `baBumBumBum_Far3.ogg`, `baBumBumBum_Far4.ogg`, `baBumBumBum_FarAlt1.wav`, `baBumBumBum_FarAlt2.wav`, `baBumBumBum_FarAlt3.wav`, `baBumBumBum_FarAlt4.wav`, `baBumBumBum_Farther1.wav`, `baBumBumBum_Farther2.wav`, `baBumBumBum_Farther3.wav`, `baBumBumBum_Farther4.wav`, `baBumBumBum_Farthest1.wav`, `baBumBumBum_Farthest2.wav`, `baBumBumBum_Farthest3.wav`, `baBumBumBum_Farthest4.wav`, `countIn1Far.wav`, `countIn1Farther.wav`, `countIn1Farthest.wav`, `countIn2Far.wav`, `countIn2Farther.wav`, `countIn2Farthest.wav`, `countIn3Far.wav`, `countIn3Farther.wav`, `countIn3Farthest.wav`, `countIn4Far.wav`, `countIn4Farther.wav`, `countIn4Farthest.wav`, `nya_Close.wav`, `nya_Far.wav`, `nya_Farther.wav`, `nya_Farthest.wav`

## ✅ 図書ガールズ（cheerReaders）— 59ファイル（RHRE3 から組み込み済み）
置き場所: `Assets/Bundled/Games/CheerReaders/Sounds/jp/`

`yay.wav`, `onegirls.ogg`, `threegirls.ogg`, `twogirls.ogg`, `bunchaBooksgirls1.ogg`, `bunchaBooksgirls2.ogg`, `bunchaBooksgirls3.ogg`, `bunchaBooksgirls4.ogg`, `bunchaBooksgirls5.ogg`, `bunchaBooksgirls6.ogg`, `bunchaBooksgirls7.ogg`, `bunchaBooksgirls8.ogg`, `bunchaBooksgirls9.ogg`, `okItsOngirls1.ogg`, `okItsOngirls2.ogg`, `okItsOngirls3.ogg`, `okItsOngirls4.ogg`, `okItsOngirls5.ogg`, `rahRahSisBoomBaBoomgirls1.ogg`, `rahRahSisBoomBaBoomgirls2.ogg`, `rahRahSisBoomBaBoomgirls3.ogg`, `rahRahSisBoomBaBoomgirls4.ogg`, `rahRahSisBoomBaBoomgirls5.ogg`, `rahRahSisBoomBaBoomgirls6.ogg`, `itgirls.ogg`, `sgirls.ogg`, `togirls.ogg`, `upgirls.ogg`, `yougirls.ogg`, `yayGirls.ogg`, `oneTwoThreeS1.ogg`, `oneTwoThreeS2.ogg`, `oneTwoThreeS3.ogg`, `bunchaBooksS1.ogg`, `bunchaBooksS2.ogg`, `bunchaBooksS3.ogg`, `bunchaBooksS4.ogg`, `bunchaBooksS5.ogg`, `bunchaBooksS6.ogg`, `bunchaBooksS7.ogg`, `bunchaBooksS8.ogg`, `bunchaBooksS9.ogg`, `okItsOnS1.ogg`, `okItsOnS2.ogg`, `okItsOnS3.ogg`, `okItsOnS4.ogg`, `okItsOnS5.ogg`, `rahRahSisBoomBaBoomS1.ogg`, `rahRahSisBoomBaBoomS2.ogg`, `rahRahSisBoomBaBoomS3.ogg`, `rahRahSisBoomBaBoomS4.ogg`, `rahRahSisBoomBaBoomS5.ogg`, `rahRahSisBoomBaBoomS6.ogg`, `itsUpToYouS1.ogg`, `itsUpToYouS2.ogg`, `itsUpToYouS3.ogg`, `itsUpToYouS4.ogg`, `itsUpToYouS5.ogg`, `yayS.wav`

## ✅ レスラー会見（ringside）— 52ファイル（RHRE3 から組み込み済み）
置き場所: `Assets/Bundled/Games/Ringside/Sounds/jp/`

`bigguy_var1_1.wav`, `bigguy_var1_2.wav`, `bigguy_var1_3.wav`, `bigguy_var1_4.wav`, `bigguy_var1_5.wav`, `bigguy_var2_1.wav`, `bigguy_var2_2.wav`, `bigguy_var2_3.wav`, `bigguy_var2_4.wav`, `bigguy_var2_5.wav`, `bigguy_var3_1.wav`, `bigguy_var3_2.wav`, `bigguy_var3_3.wav`, `bigguy_var3_4.wav`, `bigguy_var3_5.wav`, `pose_and.wav`, `pose_var1_1.wav`, `pose_var1_2.wav`, `pose_var1_3.wav`, `pose_var1_4.wav`, `pose_var1_5.wav`, `pose_var1_6.wav`, `pose_var2_1.wav`, `pose_var2_2.wav`, `pose_var2_3.wav`, `pose_var2_4.wav`, `pose_var2_5.wav`, `pose_var2_6.wav`, `wubdub_var1_1.wav`, `wubdub_var1_2.wav`, `wubdub_var1_3.wav`, `wubdub_var1_4.wav`, `wubdub_var1_5.wav`, `wubdub_var1_6.wav`, `wubdub_var1_7.wav`, `wubdub_var1_8.wav`, `wubdub_var2_1.wav`, `wubdub_var2_2.wav`, `wubdub_var2_3.wav`, `wubdub_var2_4.wav`, `wubdub_var2_5.wav`, `wubdub_var2_6.wav`, `wubdub_var2_7.wav`, `wubdub_var2_8.wav`, `wubdub_var3_1.wav`, `wubdub_var3_2.wav`, `wubdub_var3_3.wav`, `wubdub_var3_4.wav`, `wubdub_var3_5.wav`, `wubdub_var3_6.wav`, `wubdub_var3_7.wav`, `wubdub_var3_8.wav`

## ✅ タップスター（tapTroupe）— 8ファイル（RHRE3 から組み込み済み）
置き場所: `Assets/Bundled/Games/TapTroupe/Sounds/jp/`

`bamvoice1.wav`, `bamvoice2.wav`, `startTap.wav`, `tapAnd.ogg`, `tapReady1.ogg`, `tapReady2.ogg`, `tapvoice1.wav`, `tapvoice2.wav`

## ✅ 重役会議（boardMeeting）— 4ファイル（組み込み済み）
置き場所: `Assets/Bundled/Games/BoardMeeting/Sounds/jp/`

`one.wav`, `three.wav`, `two.wav`, `twoUra.wav`

## ✅ コロコロたんけん隊（flipperFlop）— 49ファイル（RHRE3 から組み込み済み）
置き場所: `Assets/Bundled/Games/FlipperFlop/Sounds/jp/`

`good.ogg`, `goodjob.ogg`, `nice.ogg`, `thatsit1.ogg`, `thatsit2.ogg`, `welldone.ogg`, `yes.ogg`, `attention1.ogg`, `attention2.ogg`, `attention3.ogg`, `attention4.ogg`, `attention5.ogg`, `attention6.ogg`, `flipperRollCount1.ogg`, `flipperRollCount10.ogg`, `flipperRollCount2.ogg`, `flipperRollCount3.ogg`, `flipperRollCount4.ogg`, `flipperRollCount5.ogg`, `flipperRollCount6.ogg`, `flipperRollCount7.ogg`, `flipperRollCount7B.ogg`, `flipperRollCount8.ogg`, `flipperRollCount9.ogg`, `flipperRollCountA.ogg`, `flipperRollCountB.ogg`, `flipperRollCountC.ogg`, `flipperRollCountNow.ogg`, `flipperRollCountS.ogg`, `flopCount1.ogg`, `flopCount1B.ogg`, `flopCount1C.ogg`, `flopCount2.ogg`, `flopCount2B.ogg`, `flopCount2C.ogg`, `flopCount3.ogg`, `flopCount3B.ogg`, `flopCount4.ogg`, `flopCount4B.ogg`, `flopCountFail1.wav`, `flopCountFail2.wav`, `flopCountFail3.wav`, `flopCountFail4.wav`, `uh1.ogg`, `uh2.ogg`, `uh3.ogg`, `uhfail1.ogg`, `uhfail2.ogg`, `uhfail3.ogg`

## ✅ シーソー（seeSaw）— 9ファイル（RHRE3 から組み込み済み）
置き場所: `Assets/Bundled/Games/SeeSaw/Sounds/jp/`

`otherVoiceLong1.wav`, `otherVoiceLong2.wav`, `otherVoiceShort1.wav`, `otherVoiceShort2.wav`, `ow.wav`, `playerVoiceLong1.wav`, `playerVoiceLong2.wav`, `playerVoiceShort1.wav`, `playerVoiceShort2.wav`

## ✅ ワリオでマンボ（warioDeMambo）— 10ファイル（まわるメイドインワリオの ROM から組み込み済み）
置き場所: `Assets/Bundled/Games/WarioDeMambo/Sounds/jp/`

`four.wav`, `three.wav`, `two.wav`, `one.wav`（日本語版は「Go!」）, `memorize.wav`, `memorize1.wav`, `memorize2.wav`, `memorize3.wav`, `ladiesandgentlemen.wav`, `wariodemambo.wav`

- 声は共通バンドル（pcomambo/common）に入っていたので、英語の声を pcomambo/locale の en に移しました。
  left・right（クイーカ）、jump（笛）、applause は声ではないので共通のままです。
- 曲表（`Tools/gba_songs.py`）でおどるワリオの曲（565〜578・591番）を見つけ、声のサンプルを
  曲データと同じタイミング（エコーの重ね）で組み立てました（ゲームでは音符の長さで声が切れますが、切れ目が目立つので最後まで鳴らしています）（`Tools/build_gba_voices.py`）。
  エコーは英語版の録音に近づけて小さめにし、左右に振ったステレオにしています（日本語版の曲データではエコーも中央で、
  音量の差が小さく聞こえたため）。音量はファイルごとに英語版に合わせ、リミッターは声だけにかけてからエコーを重ねています（`align_jp_voices.py` は使わない）。
  曲にかかっている GBA の音源のリバーブ（残響）も同じ仕組みでかけています。「おぼえて」は曲データでは音節がばらばらに並んで途切れて聞こえるので、「お」「ぼ」「えて」のサンプルを隙間なくそのままつないでいます（エコーなし）。
- 始まりのアナウンスは日本語版では1拍遅く始まるので、`voice_timing.json` で合わせています。
- 曲を WAV にして確かめるとき: `Tools/gba_render_song.py`

## ✅ ケロケロダンス（frogHop）— 30ファイル（リズム天国ゴールドの ROM から組み込み済み）
置き場所: `Assets/Bundled/Games/FrogHop/Sounds/jp/`（ファイル名は英語版と同じ `SE_NTR_FROG_EN_*`）

- DS 版は同じ声のサンプルを、リーダー（T）・プレイヤー（P）・ほかのカエル（E）で違う高さで鳴らしています。
  効果音データ（SDAT の SEQARC_KERO）を読んで、日本語版の高さ（T キー67・P 60・E 55・カウント 69）のとおりに書き出しました
  （`Tools/import_ds_voices.py`、再生は `Tools/ds_render_seq.py`）。英語版とは高さが違います（英語版は T 65・カウント 67）。
- `COUNT*_PLAYER_CUSTOM`・`_EXTRAS_CUSTOM` は Heaven Studio が作ったもの（カウントを 5・10 半音下げたもの）なので、日本語版も同じだけ下げました。
- 音量は効果音データの音量どおりに書き出して、ゲーム全体で1つの補正（+5.0dB）をかけています。
  英語版のカウントは「1」「4」が「2」「3」より 10dB 大きく不揃いなので、日本語版のカウントは英語版の「1」「4」より小さめです。
- 置いてもらったサンプル（WAR979_*）は `JapaneseVoiceSources/FrogHop/user_samples/` に移しました（ROM から書き出した声と一致）。
