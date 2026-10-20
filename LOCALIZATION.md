# 天国スタジオ（Heaven Studio 日本語版）について

このプロジェクトは Heaven Studio の日本語版「天国スタジオ」です。画面はすべて日本語で表示されます（言語の切り替えはありません）。
英語の原文はソースコードにそのまま残し、起動時に日本語の翻訳表を当てる仕組みなので、本家の更新を取り込みやすくなっています。

## 本家との違い

| 項目 | 本家 Heaven Studio | 天国スタジオ |
| --- | --- | --- |
| アプリ名 | Heaven Studio | 天国スタジオ |
| バンドルID | com.RHeavenStudio.Heaven-Studio | com.RHeavenStudio.Heaven-Studio-JA |
| 設定の保存場所 | Application Support/RHeavenStudio/Heaven Studio | Application Support/RHeavenStudio/天国スタジオ |
| ライブラリのフォルダ | 書類/Heaven Studio Levels | 書類/天国スタジオ レベル |
| 同梱レベル | 英語 | 日本語（`Assets/StreamingAssets/Library Pack-In`） |

本家と同じ Mac に入れても、設定やライブラリは混ざりません。

## 翻訳ファイル

翻訳は `Assets/Resources/Localization/ja/` の JSON にあります。キーは英語の原文、値が日本語訳です。

| ファイル | 内容 | キー |
| --- | --- | --- |
| `games.json` | ミニゲーム名（公式の日本語タイトル）と並び替え用の読み | 内部名（例: `karateman`） |
| `events.json` | イベント（ブロック）名・タブ名 | 英語名、または `ゲーム名/アクション名` |
| `params.json` | イベントのプロパティ名 | 英語名、または `ゲーム名/アクション名/プロパティ名` |
| `tooltips.json` | プロパティの説明 | 英語文、または `ゲーム名/アクション名/プロパティ名:tooltip` |
| `enums.json` | ドロップダウンの選択肢 | 列挙子名、または `型名.列挙子名` |
| `ui.json` | エディタ・メニュー・ヘルプ・ローディングヒントなど | 英語文（前後の空白は無視） |

- 同じ英語でもゲームごとに訳し分けたいときは、`ゲーム名/アクション名` 形式のキーを追加します（英語キーより優先されます）。
- `{0}` などは数値や名前が入る場所です。訳文にも残してください。
- `<color=...>` などのリッチテキストタグは訳文でも維持してください。
- ビルド後でも、`{persistentDataPath}/Localization/ja/*.json` に同じ形式のファイルを置くと、同梱の訳を上書きできます。
- 英語で1行に収まっていた画面上のラベルは、日本語が長くても自動で縮小して1行に収まります。チェックボックスなどが文字のすぐ後ろに固定で置かれている行は縮小が効かないので、訳を短くしてください。

## 日本語音声

ミニゲームの声は、各ゲームの `Assets/Bundled/Games/<ゲーム>/Sounds/jp/` に英語版と同じファイル名で置きます（.ogg / .wav どちらでも可）。
置いたあと、そのゲームの `.meta` に言語別バンドル（`<バンドル名>/locale`、バリアント `jp`）を設定し、ゲーム定義の対応言語に `"jp"` を足すと日本語音声で鳴ります。残りの素材は `Japanese_Voice_Checklist.md` にまとめています。

- **音量と頭の無音をそろえる**：`python3 Tools/align_jp_voices.py <ゲームのフォルダ名>`
  ゲームは英語版の頭の無音を前提にタイミングを組んでいるので、声の立ち上がり位置と音量を英語版に合わせます。元のファイルは `JapaneseVoiceSources/` に残ります。
- **RHRE3 から取り込む**：`Tools/import_rhre_voices.py <ゲーム>`（numpy が必要）。RHRE3 の日本語版の音を Heaven Studio のファイル名に付け替えて `Sounds/jp/` に書き出します。対応表はスクリプト内の `MAPPINGS` に書きます。英語版から効果音の部分が分かる場合は、日本語版から差し引きます。取り込んだあとは上の `align_jp_voices.py` を実行してください。
- **リズムが違う掛け声**：`Assets/Resources/Localization/ja/voice_timing.json` に、日本語音声のときだけ声をずらす量を書きます。
  キーは `ゲーム名/音声ファイル名`、値は `beat`（拍でずらす。マイナスで早く）、`offset`（秒で微調整）、`volume`（音量の倍率）、`mute`（true で鳴らさない）。
  例：カラテ家は英語「Hit・3」、日本語「3・Hit」なので、数字を -0.5拍、Hit を +0.5拍ずらしています。
  日本語版のリズムは RHRE3 のデータベース（`~/.rhre3/sfx/master/games/<ゲーム>Ja/data.json`）のパターンを英語版と比べると確認できます。
- **共通のカウント**：`Assets/Resources/Sfx/count-ins/jp/` に日本語版を置いています（RHRE3 の countInJa から）。日本語版では、`count-ins/` 直下の音を鳴らすときにまず `jp/` の同名ファイルを探します（GBA・DS のカウントは英語のまま）。
  通常：ワン・ツー・スリー・フォー／ハイ、別バージョン：いち・に・さん・し／ホイ、And：ン、Ready：せー・の。
- ビルド後でも `{persistentDataPath}/Localization/ja/voice_timing.json` を置くと上書きできます（アプリの再起動で反映）。

## 本家の更新を取り込んだとき

Unity のメニューから次の2つを使います。

- **Heaven Studio → Localization → Check Japanese Coverage**：ミニゲーム定義のうち日本語になっていないものを一覧にします。
- **Heaven Studio → Localization → Export Source Strings**：シーン・プレハブを含むすべての英語文字列を `Localization/source_strings.json` に書き出します。

コマンドラインからも実行できます:

```bash
Unity -batchmode -quit -projectPath . -executeMethod HeavenStudio.EditorTools.LocalizationExporter.CheckBatch -locLang ja -locOut report.txt
```

## コードから文字列を表示する場合

- `Localization.T("英語")` … 翻訳を返します（訳がなければ英語のまま）。
- `Localization.F("Beat {0}", beat)` … 書式文字列を翻訳してから埋め込みます。
- `Localization.EnumNames(typeof(...))` … `Enum.GetNames` の代わりに使うと選択肢が翻訳されます。
- シーンやプレハブに置いた TextMeshPro のテキストは、シーン読み込み時に自動で翻訳されます。実行時に生成するプレハブは `UITextLocalizer.LocalizeHierarchy(obj)` を呼んでください。

日本語フォントは、TextMeshPro のグローバルフォールバック（`TMP Settings`）に登録した「FOT-Rodin Pro DB」で表示されます。
