# 日本語化が必要な画像

## 済み
- 評価：ハイレベル／平凡／やりなおし（3コマ）… `Tools/make_jp_text_images.py`（文言は 3DS 版で確認）
- 「めざせパーフェクト！」（ゲーム中とライブラリの2枚。3DS 版で確認）
- パーフェクト失敗画面の「You Missed...」→「しっぱい…」（3DS 版で確認。文字シートを描き直し、`CamerasGame.prefab` の並びを調整）
- クレジット画面のロゴ（`appLogo.png`）→ 雲はそのままで文字を天国スタジオに
- 同梱レベルのフォルダアイコン3つ（天国スタジオのロゴ／プラスパック／ショーケース レベル）
- カラテ家の「Grr!」「Combo!」→ 画像に入っていた日本語版の「ムッ」「来る」に切り替え
- タイトルのロゴ（`heavenstudiologo.png`）は以前の作業で天国スタジオの文字に置き換え済み

- ゐあひ斬り外伝「Faster!」→「加速」（DS 版から確認。筆文字＋残像）
- しわけのポスター「Spider-Free Candy」→「高品質な商品の」（3DS 版から確認）
- パチパチ三人衆「Trio Timing!」→「均等に3つ!」（3DS 版から確認）
- ラップメンの吹き出し「UHN!」→「うん」（GBA 版から確認）
- リズム感チェックのグラフ「Test Results」「Late ▲ Early」→「タイミンググラフ」「おそい ▲ はやい」（GBA 版から確認。
  日本語版は左が「はやい」だが、Heaven Studio のグラフの向きに合わせて意味の通る側に置いた）

日本語版でも英語のままだったので変えないもの（ROM で確認）:
- トンネルの「GAME OVER」「HISCORE」（DS 版）
- レスラー会見の新聞「Nintendo Times」「Sports News」（3DS 版）
- リズム脱毛の「OK」（3DS 版）
- バイキンはかせの「GAME OVER」（GBA 版）
- カメラマンの看板「RHYTHM KART」（3DS 版）
- まんざいの「SPEED UP!」（Wii 版）
- リズムファイターVS の「Fight!」「WIN!」「KO」（Wii 版）

英語の元画像・元の設定は `JapaneseImageSources/` にあります。
ROM から書き出した日本語版の画像（資料）は `JapaneseImageRefs/` にあります。
- DS: `Tools/dump_ds_graphics.py --cells`、3DS: `Tools/dump_3ds_textures.py`（`--layout` で共通画面）
- GBA: `Tools/dump_gba_graphics.py`（RhythmTengokuPlus の graphics フォルダから）
- Wii: `Tools/bin/nodtool extract` でディスクを展開してから `Tools/dump_wii_textures.py`

macOS の文字認識（`Tools/bin/find_text_images`）と見た目で、ゲーム内で使われている英語入りの画像を探しました。
参考スクリーンショット（Refs・Screenshot など、ゲームでは使われていないもの）は除いています。

**大事な点**：Heaven Studio の画像は、元のゲームから抜き出したものではなく、高解像度で描き直したものです（多くが 2048〜4096px）。
ROM から取り出した画像は小さいドット絵なので、そのまま差し替えると画質が合いません。
ROM は「日本語版ではどんな文字・デザインか」を確かめる資料として使い、画像は同じ大きさ・同じ配置で日本語版を描き直します。
プロジェクトには日本語フォント（FOT-スランプ、マティスミノリ、ニューシネマA、Rodin）があります。

## 1. 共通の画面（文字だけの画像。描き直しやすい）

| 画像 | 今の文字 | 日本語版（ROM で要確認） |
| --- | --- | --- |
| `Resources/Sprites/UI/Ratings/Superb/superbrating.png` | Superb | ハイレベル |
| `Resources/Sprites/UI/Ratings/OK/okrating.png` | OK | 平凡 |
| `Resources/Sprites/UI/Ratings/TryAgain/tryagainrating0001〜0003.png` | Try Again | やりなおし |
| `Resources/Sprites/UI/Common/GameOverlays/GoForPerfect.png` | Go for a Perfect! | めざせパーフェクト！ |
| `Resources/Sprites/UI/Library/libraryPerfectText.png` | Go for a Perfect! | 同上 |
| `Resources/Sprites/UI/Common/GameOverlays/You Mised.png` | You missed. | しっぱい… |
| `Resources/Sprites/UI/PerfectAndPractice.png`・`Library/libraryPerfect.png` | P（パーフェクトの印） | 要確認（記号なのでそのままでもよい） |
| `Resources/Sprites/UI/appLogo.png`・`Common/Opening/heavenstudiologo.png` | HEAVEN STUDIO | 天国スタジオのロゴ |
| `StreamingAssets/Library Pack-In/*/folderIcon.png` | HEAVEN STUDIO／Plus Pack／Bobby Builds Showcase Levels | 天国スタジオ／プラスパック／… |

## 2. ミニゲームの絵の中の文字（絵の一部なので、文字の部分だけ描き直す）

| ゲーム | 画像 | 今の文字 | 元のソフト（目安） |
| --- | --- | --- | --- |
| カラテ家 | `KarateMan/Sprites/karateman_word.png` | Hit／Combo!／Grr!（「来る」「ムッ」はすでにある） | Wii・3DS |
| ゐあひ斬り外伝 | `SamuraiSliceNtr/Sprites/samurainew.png`・`ntr_samurai_speedUp.png` | Faster! | DS |
| まんざい | `Manzai/Sprites/Background_Sprites.png` | SPEED UP! | Wii |
| しわけ | `PackingPests/Sprites/packing_pests_bg.png` | Spider-Free／Candy（箱のラベル） | Wii |
| レスラー会見 | `Ringside/Sprites/ringside_bg.png` | Nintendo Times／Sports News（新聞） | Wii |
| パチパチ三人衆 | `ClappyTrio/Sprites/GameSprites.png` | Trio Timing! | GBA・DS・Wii |
| リズム感チェック | `RhythmTestGBA/Sprites/RhythmTest.png` | Test Results／Early／Late | GBA |
| トンネル | `Tunnel/Sprites/tunnelChar.png` | HIGHSCORE／GAME OVER | DS |
| バイキンはかせ | `SickBeats/Sprites/sick_main.png` | GAME OVER | DS |
| ラップメン | `RapMen/Sprites/BlueWoman.png`・`YellowRapper.png` | UHN!（吹き出し） | DS |
| リズムファイターVS | `RhythmFighter/Sprites/fighter_versus_ui.png` | Fight!／REC | 要確認 |
| リズム脱毛 | `RhythmTweezers/Sprites/face.png` | OK | GBA・Wii |
| カメラマン | `FreezeFrame/Sprites/CameraMan_BG.png` | 背景の看板（要確認） | 3DS |

## 3. 文字認識が拾ったが日本語化しなくてよさそうなもの

- `DogNinja`・`SumoBrothers`・`WarioDeMambo` の「WildBreathG4M3R」…制作者のサインなどのお遊び
- `BonOdori`・`FanClub`・`MonkeyWatch`・`Showtime`・`SpaceDance`・`SamuraiSliceRvl/demons` …模様を文字と誤認
- `Ringside/.../quiz_bg.png`（QUIZ）…ゲームで使われていない

## 進め方

1. ROM で日本語版の文字とデザインを確認する（スクリーンショットで十分）
2. 文字だけの画像（1）は、私が同じ大きさ・同じ配置でフォントから描き直す
3. 絵の中の文字（2）は、英語の部分を消して日本語を描き入れる。シンプルな看板やラベルは私が加工できます。凝った手描き文字は画像編集ソフトでの作業が向いています
4. 英語の元画像は `JapaneseImageSources/` に保存してから差し替える
