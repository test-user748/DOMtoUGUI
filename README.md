# DOMtoUGUI

Unity 2022.3 LTS 向けの Editor 拡張ツールです。DOM や HTML から抽出した UI 構造を中間 JSON 仕様に落とし込み、ワンクリックで uGUI の Canvas 階層を生成します。レイアウトは Unity 標準の `Vertical/Horizontal/Grid Layout Group` を活用し、JsonUtility だけでデシリアライズ可能な軽量スキーマを採用しています。

## リポジトリ構成
- `Packages/com.testuser748.domtoug/`
  - `Editor/DomToCanvas/`
    - `UIJsonModels.cs` — JSON スキーマに対応する C# モデルとパース補助ユーティリティ
    - `DomToCanvasImporter.cs` — JSON から Canvas を生成する Editor 拡張
  - `Samples~/sample_calendar_ui.json` — 教科書的なカレンダー UI のサンプル JSON
  - `package.json` — VPM 用パッケージメタデータ
- `dist/com.testuser748.domtoug-0.1.0.zip` — VPM 配布向けビルド済みパッケージ
- `index.json` — VCC コミュニティリポジトリ用のパッケージ一覧
- `docs/`
  - `ui_spec.md` — UI 仕様 JSON の詳細設計とサンプル
  - `architecture.md` — 簡易アーキテクチャと責務整理

## VCC (VPM) からのインストール
1. VCC 2.1.0 以降で、Settings ▶ Packages ▶ **Add Repository** を開きます。 ([Community Repositories](https://vcc.docs.vrchat.com/guides/community-repositories/))
2. リポジトリ URL に以下を入力して追加します。  
   `https://raw.githubusercontent.com/test-user748/DOMtoUGUI/main/index.json`
3. 任意のプロジェクトに移動し、Available Packages から **DOMtoUGUI** を追加します。
4. インポート後、Unity メニューの **Tools ▶ DomToCanvas ▶ Import UI JSON...** を実行し、`.json` を選択すると新しい `Canvas` と `EventSystem`（未存在時）が生成され、JSON 階層が再現されます。

### サンプル JSON の読み込み
`Packages/com.testuser748.domtoug/Samples~/sample_calendar_ui.json` を選択すると、予約カレンダー風の UI が生成されます。TextMeshPro がインストールされている場合は自動的に使用され、未導入の場合は標準 `UI.Text` にフォールバックします。

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
