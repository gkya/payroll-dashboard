# Git ハンズオン運用メモ

## 1. 目的

この学習では、コードを書くことだけでなく、実務に近い流れで進めることを目的にします。

扱う流れ:
- 仕様を書く
- 小さく実装する
- Git で区切って記録する
- 必要ならリモートへ push する

---

## 2. おすすめの進め方

最小開発サイクル:
1. 仕様書を更新する
2. 1 つの小さなタスクを決める
3. 実装する
4. ローカルで動作確認する
5. Git でコミットする
6. 区切りがよければ push する

---

## 3. コミット粒度の例

良い例:
- `docs: add initial payroll dashboard spec`
- `docs: add payroll dashboard design direction`
- `feat: create razor pages app skeleton`
- `feat: add payroll slip model`
- `feat: add sqlite payroll repository`
- `feat: add payroll import page`
- `feat: add payroll api controller`

避けたい例:
- 1回のコミットにモデル、画面、API、設定変更を全部入れる

---

## 4. ブランチの切り方

最初のおすすめ:
- `main`
- `feature/spec`
- `feature/setup`
- `feature/import-page`
- `feature/payroll-api`

学習初期は `main` だけでも問題ありません。

---

## 5. 1 回目のブランチ案

- `feature/spec-payroll-dashboard`

このブランチでやること:
- 仕様書作成
- デザイン文書作成
- README 整備

---

## 6. 初回コミット候補

1 回目:
- `docs: initialize payroll dashboard repository`

2 回目:
- `docs: add payroll dashboard spec and design`

3 回目:
- `feat: create razor pages app skeleton`

---

## 7. push の考え方

push するタイミング:
- 仕様がまとまったとき
- 画面や API が 1 つ動いたとき
- 今日はここまで、という区切りのとき

---

## 8. 次回の実作業候補

次のおすすめ順:
1. この文書群をコミットする
2. `.gitignore` を作る
3. `dotnet new webapp` でアプリの土台を作る
4. Import + List 画面を作り始める

