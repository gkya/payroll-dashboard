# C# プロジェクト構造ガイド - エンジニア1年目向け

このガイドは、給与明細ダッシュボードのC#プロジェクト構造を理解するためのものです。

## 📊 全体構造図

```
ユーザーのリクエスト (URLをクリック)
         ↓
┌─────────────────────────────────┐
│   Pages/ (UI - cshtml.cs)       │  ← ユーザーが見る画面
│   例: Import.cshtml.cs          │
└─────────────────┬───────────────┘
                  ↓
┌─────────────────────────────────┐
│ Services/ (ビジネスロジック)     │  ← "何をするか"の処理
│ 例: PayrollIngestionService    │
└─────────────────┬───────────────┘
                  ↓
┌─────────────────────────────────┐
│ Repositories/ (DB操作)          │  ← データベースへの出し入れ
│ 例: SqlitePayrollRepository      │
└─────────────────┬───────────────┘
                  ↓
┌─────────────────────────────────┐
│ Model/ (データ構造)              │  ← データの「形」
│ 例: PayrollSlip                  │
└─────────────────────────────────┘
                  ↓
            [データベース]
```

---

## 🏗️ 各フォルダの役割（具体例付き）

### 1️⃣ **Models/** - データの「ひな形」

**何をする？** ファイルのデータを入れる箱の「形」を定義する

```csharp
// Models/PayrollSlip.cs
public class PayrollSlip
{
    public int Id { get; set; }              // ID
    public string PayrollMonth { get; set; } // 給与月
    public decimal? GrossAmount { get; set; } // 総支給額
    public decimal? NetAmount { get; set; }   // 手取り額
}
```

**使い方：** 「給与明細には何の情報が必要か」を定義
- ID
- 給与月
- 総支給額
- 手取り額
- など

---

### 2️⃣ **Services/** - ビジネスロジック（仕事の中身）

**何をする？** PDFファイルを読む、データを処理する、など実際の「作業」を行う

```csharp
// Services/PayrollPdfParser.cs
public class PayrollPdfParser
{
    public PayrollParseResult Parse(string filePath)
    {
        // PDFファイルを開く
        using var document = PdfDocument.Open(filePath);
        var page = document.GetPage(1);
        
        // ページから「総支給額」「控除合計」などを探す
        var grossAmount = FindAmountBelowLabel(words, "【総支給額】");
        var deductionAmount = FindAmountBelowLabel(words, "【控除合計】");
        var netAmount = FindAmountBelowLabel(words, "【差引支給額】");
        
        // 結果をまとめて返す
        return new PayrollParseResult { ... };
    }
}
```

**例：何を処理している？**
- PDFを読む
- 数字を抽出する
- データを計算する
- エラーチェックをする

---

### 3️⃣ **Repositories/** - データベースへの出し入れ

**何をする？** データベースに「保存する」「読み込む」という操作だけ

```csharp
// Repositories/SqlitePayrollRepository.cs
public class SqlitePayrollRepository : IPayrollRepository
{
    // データを保存する
    public void Save(PayrollSlip slip)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        
        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO PayrollSlips (SlipType, PayrollMonth, SourceFileName, ...)
            VALUES ($slipType, $month, $fileName, ...)
            """;
        
        command.ExecuteNonQuery();
    }
    
    // データを取得する
    public PayrollSlip? GetById(int id)
    {
        // SQLを実行してデータを読む
        // ...
    }
}
```

**何をしていない？**
- ❌ PDFを読む
- ❌ データを計算する
- ❌ エラーハンドリング（それはServiceがやる）

**何をしている？**
- ✅ INSERT（保存）
- ✅ SELECT（読み込み）
- ✅ UPDATE（更新）
- ✅ DELETE（削除）

---

### 4️⃣ **Pages/** - ユーザーが見る画面

**何をする？** Webページを表示して、ユーザーとやり取りする

```csharp
// Pages/Import.cshtml.cs
public class ImportModel : PageModel
{
    private readonly PayrollIngestionService _service;
    
    public ImportModel(PayrollIngestionService service)
    {
        _service = service;
    }
    
    public async Task OnPostAsync(IFormFile uploadedFile)
    {
        // ファイルアップロード処理
        // → Services を呼ぶ
        // → 結果をユーザーに表示
    }
}
```

```html
<!-- Pages/Import.cshtml -->
<form method="post" enctype="multipart/form-data">
    <input type="file" name="uploadedFile" />
    <button type="submit">アップロード</button>
</form>
```

---

### 5️⃣ **Controllers/** - APIとしてデータを返す

**何をする？** JSON形式でデータを返す（スマートフォンアプリなどから使われる）

```csharp
// Controllers/PayrollController.cs
[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollRepository _repository;
    
    [HttpGet("slips")]
    public ActionResult<IEnumerable<PayrollSlip>> GetAllSlips()
    {
        // 全ての給与明細をJSON形式で返す
        return Ok(_repository.GetAll());
    }
    
    [HttpGet("slips/{id}")]
    public ActionResult<PayrollSlip> GetSlipById(int id)
    {
        // 指定されたIDの給与明細をJSON形式で返す
        return Ok(_repository.GetById(id));
    }
}
```

---

## 🔄 データの流れ（具体的な例）

### シナリオ：給与明細PDFをアップロードする

```
1. ユーザーが Pages/Import.cshtml でファイルを選んで「送信」
       ↓
2. Import.cshtml.cs の OnPostAsync() が実行される
       ↓
3. PayrollIngestionService.ProcessAsync() を呼ぶ
   （Services層で「実際の作業」）
       ↓
4. PayrollPdfParser.Parse() でPDFから数字を抽出
   （Servicesの中で細かい処理）
       ↓
5. 抽出したデータを PayrollSlip モデルに詰め込む
   （Models - 箱に入れる）
       ↓
6. _repository.Save(payrollSlip) でDB保存
   （Repositories - DBに出し入れ）
       ↓
7. ユーザーに成功画面を表示
```

---

## 🎯 「何を書くか」のチェックリスト

### Models/ に書く
- ✅ プロパティ（データの項目）
- ✅ 型（int, string, decimal, etc）
- ✅ デフォルト値
- ❌ ロジック（if文など）

### Services/ に書く
- ✅ PDFを読む
- ✅ データを計算する
- ✅ 複雑なビジネスロジック
- ✅ エラーハンドリング
- ❌ データベース操作（Repositoryに任せる）
- ❌ Webページを返す（Pages/Controllersに任せる）

### Repositories/ に書く
- ✅ INSERT, SELECT, UPDATE, DELETE
- ✅ 接続文字列管理
- ✅ SQL文の実行
- ❌ 計算や加工（Servicesに任せる）
- ❌ ビジネスロジック

### Pages/ に書く
- ✅ HTMLの表示
- ✅ ユーザー入力の受け取り
- ✅ Services を呼んで結果を表示
- ❌ データベース操作（Repositoryに任せる）
- ❌ 複雑な計算（Servicesに任せる）

### Controllers/ に書く
- ✅ HTTP GETやPOSTの処理
- ✅ Servicesを呼んでデータを返す
- ✅ JSON形式でデータを返す
- ❌ HTMLを返す（Pages/に任せる）
- ❌ ビジネスロジック（Servicesに任せる）

---

## 🧪 実際に書く時の流れ

### 「新しい機能を追加したい」とき

1. **Models/ に箱を作る** → 「どんなデータが必要か」
   ```csharp
   public class NewData { ... }
   ```

2. **Repositories/ でDB操作を書く** → 「DBに保存・読む」
   ```csharp
   public void SaveNewData(NewData data) { ... }
   public NewData? GetNewData(int id) { ... }
   ```

3. **Services/ でビジネスロジックを書く** → 「実際の処理」
   ```csharp
   public void ProcessNewData() { ... }
   ```

4. **Pages/ または Controllers/ で呼び出す** → 「ユーザーに見せる」
   ```csharp
   await _service.ProcessNewData();
   ```

---

## 📝 よくある質問

### Q: 「この処理はServices? それともRepositories?」
**A:** こう考えてください
- **Services**: 「頭を使う処理」
  - 計算、判断、複数のステップ
  - 例: PDFを読んで数字を抽出して保存する
  
- **Repositories**: 「DB操作だけ」
  - INSERT/SELECT/UPDATE/DELETEのみ
  - 例: データをDBに保存する

### Q: Services内で複数のRepositoryを使ってもいい?
**A:** はい！実は多くの場合そうします
```csharp
public class PayrollIngestionService
{
    public void Process()
    {
        var slip = _payrollRepository.GetLatest();
        _reportRepository.Save(new Report());
    }
}
```

### Q: Models内に計算メソッドを書いてもいい?
**A:** 小さい計算なら OK
```csharp
public class PayrollSlip
{
    public decimal? GrossAmount { get; set; }
    public decimal? DeductionAmount { get; set; }
    
    // これはOK - Modelの責務の範囲
    public decimal? GetNetAmount() => GrossAmount - DeductionAmount;
}
```

ただし複雑な計算はServices に書く

---

## 🎓 まとめ

| 層 | 役割 | 例 | 呼ぶ側 |
|---|---|---|---|
| **Models** | データの「形」を定義 | PayrollSlip | 全部が使う |
| **Services** | ビジネスロジック（複雑な処理） | PDF読込、データ処理 | Pages, Controllers |
| **Repositories** | DB操作のみ | INSERT, SELECT | Services |
| **Pages** | Webページ表示 | HTML, フォーム | ユーザーからのリクエスト |
| **Controllers** | API（JSON返却） | GETで全データ返す | APIクライアント |

**大事なルール:**
- 上から下へ呼び出す（Modelsを呼ぶ方向）
- 下から上へ呼び出してはいけない（RepositoriesがPagesを呼んじゃダメ）
- 各層は自分の責任だけを持つ
