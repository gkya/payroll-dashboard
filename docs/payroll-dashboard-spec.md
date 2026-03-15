# 仕様書: Payroll Dashboard

## 1. 文書の目的

この文書は、学習用アプリ `Payroll Dashboard` の初期仕様を整理するためのものです。

このアプリの目的:
- C# の書き方を学ぶ
- MSI アプリケーションに近い構造で、小さな業務アプリを作る
- PDF 取込、保存、API、画面表示の流れを一通り学ぶ

---

## 2. 背景

参考にする既存コード:
- `MSI` リポジトリの `Program.cs`
- `MSI` リポジトリの `SensorTopicController.cs`
- `MSI` リポジトリの `Services/Payroll/PayrollIngestionService.cs`
- `MSI` リポジトリの `Services/Payroll/SqlitePayrollRepository.cs`
- `MSI` リポジトリの `Pages/Payroll.cshtml`

既存 MSI コードの学習ポイント:
- `Program.cs` で DI を組む
- `Controller` で API を公開する
- `Service` に処理を寄せる
- `Repository` に保存処理を寄せる
- `PageModel` と Razor で画面を表示する

今回の題材では、センサデータの代わりに給与明細 PDF を扱います。

---

## 3. プロダクト概要

`Payroll Dashboard` は、給与明細 PDF を取り込み、一覧・集計・推移を見える化する個人向けダッシュボードです。

最初に扱う主要データ:
- 支給月
- 社員名
- 総支給額
- 控除額
- 差引支給額
- 取込日時
- 取込結果

---

## 4. 学習目標

このアプリを通して学ぶこと:
- class / enum / interface の使い方
- コンストラクタ DI
- ASP.NET Core の `Program.cs`
- Razor Pages
- Controller と API
- Service と Repository の責務分割
- SQLite 永続化
- Git で段階的に実装する進め方

---

## 5. 想定ユーザー

主ユーザー:
- 開発者本人

利用シーン:
- 手元の給与明細 PDF を取り込む
- 月ごとの支給・控除・手取りを一覧で見る
- 推移をグラフで確認する
- 実装を通して C# の構造を学ぶ

---

## 6. 解決したいこと

### 6.1 ユーザー課題
- 給与明細 PDF を月ごとに整理したい
- 支給額や控除額の変化を見える化したい
- 後から見返しやすい形で保存したい

### 6.2 学習課題
- C# の責務分割がまだ腹落ちしていない
- MSI のような構造を小さく再現して学びたい

---

## 7. スコープ

### 7.1 v0.1 最小版
- PDF アップロード
- ファイル保存
- SQLite 保存
- 一覧表示
- 取込状態表示

### 7.2 v0.2 解析版
- PDF から必要項目を抽出
- 支給額、控除額、手取りを表示
- 月別集計を出す

### 7.3 v0.3 ダッシュボード版
- KPI カード表示
- 月別推移グラフ
- 控除率や手取り率の表示

### 7.4 今回やらないこと
- 複数ユーザー管理
- クラウド同期
- 本格認証
- 会計システムとの連携

---

## 8. ユースケース

### UC-01 PDF をアップロードする
- ユーザーは給与明細 PDF を選択してアップロードする
- システムは PDF を保存し、取込レコードを作る

### UC-02 取込済み一覧を見る
- ユーザーは取込済み給与明細一覧を見る
- 支給月、金額、状態、ファイル名を確認できる

### UC-03 月ごとの推移を見る
- ユーザーは月別の手取り推移を見る
- 増減をざっくり把握できる

### UC-04 取込失敗を確認する
- ユーザーは解析できなかった PDF の状態とメッセージを見る

---

## 9. 機能要件

### FR-01 PDF アップロード
- PDF ファイルを受け取れること
- PDF 以外は拒否すること

### FR-02 ファイル保存
- アップロードしたファイルをローカルに保存すること
- 重複ファイルをハッシュで判定すること

### FR-03 取込レコード保存
- SQLite に給与明細情報を保存すること

### FR-04 一覧表示
- 取込済み給与明細一覧を画面で表示すること

### FR-05 API
- `GET /api/payroll/slips`
- `GET /api/payroll/slips/{id}`
- 将来的に `GET /api/payroll/summary`

### FR-06 PDF 解析
- v0.2 で PDF テキストから必要項目を抽出すること

### FR-07 集計
- v0.3 で総支給、控除、手取りの月別集計を出すこと

---

## 10. 非機能要件

### NFR-01 開発環境
- Mac 上で動作すること
- .NET 8 を使うこと

### NFR-02 学習しやすさ
- クラス責務を分けること
- 既存 MSI コードとの対応が説明できること

### NFR-03 拡張性
- PDF 取込、解析、保存、表示を分離すること
- 今後の集計やグラフ追加がしやすいこと

---

## 11. システム構成案

構成:
- ASP.NET Core Web App
- Razor Pages
- Controller API
- Service
- Repository
- SQLite

役割分担の案:
- `Program.cs`
  - DI 登録
- `PayrollController`
  - API エンドポイント
- `PayrollPageModel`
  - 画面入力と画面表示
- `PayrollIngestionService`
  - 取込処理
- `PayrollPdfParser`
  - PDF 解析
- `SqlitePayrollRepository`
  - DB 保存・取得

---

## 12. MSI との対応表

| MSI 側 | 学習アプリ側 | 学べること |
|---|---|---|
| `Program.cs` | `Program.cs` | DI と起動構成 |
| `SensorTopicController` | `PayrollController` | API 設計 |
| `SensorData` | `PayrollSlip` | モデル設計 |
| `MessageBuffer` | 一時取込結果や ViewModel | データ受け渡し |
| `Worker` | 将来の自動取込 Worker | バックグラウンド処理 |
| `Repository` 相当 | `SqlitePayrollRepository` | 保存責務の分離 |

---

## 13. 画面一覧

### 13.1 Dashboard
- KPI カード
- 月別推移グラフ
- 最近取り込んだ給与明細一覧

### 13.2 Import
- PDF アップロードフォーム
- 取込結果メッセージ

### 13.3 Slip List
- 取込済み給与明細一覧
- フィルタや並び替えは将来追加

### 13.4 Slip Detail
- 1 件の給与明細の詳細表示

---

## 14. API 案

### `GET /api/payroll/slips`
- 給与明細一覧を返す

### `GET /api/payroll/slips/{id}`
- 指定 ID の給与明細詳細を返す

### `GET /api/payroll/summary`
- 将来的に月別集計を返す

---

## 15. データモデル案

### `PayrollSlip`
- `Id`
- `PayrollMonth`
- `EmployeeName`
- `GrossAmount`
- `DeductionAmount`
- `NetAmount`
- `SourceFileName`
- `SourceFilePath`
- `SourceFileHash`
- `ImportStatus`
- `ParseMessage`
- `ImportedAt`
- `UpdatedAt`

### `PayrollImportStatus`
- `Imported`
- `Parsed`
- `ParseFailed`

### 将来の集計モデル
- `PayrollMonthlySummary`
- `PayrollKpi`

---

## 16. 実装ステップ

### Step 1 仕様とデザインを固める
- この文書とデザイン文書を作る

### Step 2 一覧画面の土台を作る
- 取込フォーム
- 一覧表示
- SQLite 保存

### Step 3 API を作る
- 一覧 API
- 詳細 API

### Step 4 PDF 解析を作る
- 解析クラス追加
- 金額抽出

### Step 5 ダッシュボード化する
- KPI
- グラフ
- 月別サマリー

### Step 6 自動化やリファクタリング
- 取込監視 Worker
- テスト追加

---

## 17. MVP 完成条件

MVP 完成条件:
- PDF をアップロードできる
- SQLite に保存できる
- 一覧画面で見える
- API で取得できる
- 変更履歴が Git に小さく残っている

---

## 18. 次に決めること

次回までに決める項目:
- 学習用アプリ名
- 最初に作る画面
- ダッシュボードの KPI 項目
- v0.1 で本当に入れる機能

現時点のたたき台:
- アプリ名: `PayrollDashboard`
- 最初の画面: `Import + List`

