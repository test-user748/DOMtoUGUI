# DOMtoUGUI

Unity 2022.3 LTS 向けの Editor 拡張ツールです。DOM や HTML から抽出した UI 構造を中間 JSON 仕様に落とし込み、ワンクリックで uGUI の Canvas 階層を生成します。レイアウトは Unity 標準の `Vertical/Horizontal/Grid Layout Group` を活用し、JsonUtility だけでデシリアライズ可能な軽量スキーマを採用しています。

## リポジトリ構成
- `Assets/Editor/DomToCanvas/`
  - `UIJsonModels.cs` — JSON スキーマに対応する C# モデルとパース補助ユーティリティ
  - `DomToCanvasImporter.cs` — JSON から Canvas を生成する Editor 拡張
- `docs/`
  - `ui_spec.md` — UI 仕様 JSON の詳細設計とサンプル
  - `architecture.md` — 簡易アーキテクチャと責務整理
- `Samples/`
  - `sample_calendar_ui.json` — 教科書的なカレンダー UI のサンプル JSON

## 使い方
1. このリポジトリを Unity プロジェクトのルートに配置するか、`Assets/Editor` 配下へフォルダをコピーします。
2. Unity を起動し、任意のシーンを開きます。
3. メニューから **Tools ▶ DomToCanvas ▶ Import UI JSON...** を実行します。
4. `.json` ファイルを選択すると、新しい `Canvas` と `EventSystem`（存在しない場合）がシーンに生成され、JSON の階層が再現されます。

### サンプル JSON の読み込み
`Samples/sample_calendar_ui.json` を選択すると、予約カレンダー風の UI が生成されます。TextMeshPro がインストールされている場合は自動的に使用され、未導入の場合は標準 `UI.Text` にフォールバックします。

## UI 仕様 JSON
- 設計思想やフィールド仕様は `docs/ui_spec.md` を参照してください。
- DOM 抽出結果を中間表現として表現することを目的にしているため、厳密なピクセル座標ではなくレイアウト指向の情報のみを保持します。

## 制限事項と今後の拡張
- 対応レイアウト: `vertical` / `horizontal` / `grid` / `none`
- 対応ノード type: `container`, `panel`, `text`, `button`, `image`
- 画像アセットの自動解決、アニメーション、レスポンシブ挙動、リッチテキスト細部などは未対応。必要に応じて `DomToCanvasImporter` のスイッチロジックを拡張してください。

## 動作要件
- Unity **2022.3 LTS**（Editor 拡張として動作）
- 追加パッケージ不要。TextMeshPro が存在すれば自動利用、無ければ標準 UI.Text を使用
