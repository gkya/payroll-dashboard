# PayrollDashboard 全体構成メモ

> C# / ASP.NET Core Razor Pages の学習用まとめ。  
> 1年目エンジニア向けに、このプロジェクトでどんな概念を使っているかを整理する。

---

## 目次

1. [鳥瞰図](#1-鳥瞰図)
2. [フォルダ構成](#2-フォルダ構成)
3. [Models — データの型を定義する](#3-models--データの型を定義する)
4. [Program.cs — 誰が誰の仕事をするかを登録する](#4-programcs--誰が誰の仕事をするかを登録する)
5. [Repositories — DB とのやり取りだけを担当](#5-repositories--db-とのやり取りだけを担当)
6. [Services — ビジネスロジックをここに集める](#6-services--ビジネスロジックをここに集める)
7. [Pages — 画面の構成（Razor Pages）](#7-pages--画面の構成razor-pages)
8. [Controllers — JSON や PDF を返す API](#8-controllers--json-や-pdf-を返す-api)
9. [データが画面に表示されるまでの流れ](#9-データが画面に表示されるまでの流れ)
10. [C# の重要概念まとめ](#10-c-の重要概念まとめ)

---

## 1. 鳥瞰図

```
ブラウザのリクエスト
        ↓
  Program.cs（アプリ起動・設定）
        ↓
  Pages/ または Controllers/（受付窓口）
        ↓
  Services/（仕事をする人）
        ↓
  Repositories/（データの倉庫番）
        ↓
  SQLite（データベース）
```

---

## 2. フォルダ構成

```
PayrollDashboard/
├── Program.cs                      ← アプリの「設計図」を登録する
│
├── Models/                         ← データの「型」を定義する
│   ├── PayrollSlip.cs              ← 給与・賞与明細
│   ├── AnnualIncomeSlip.cs         ← 源泉徴収票
│   ├── PayrollSlipType.cs          ← enum（給与 or 賞与）
│   └── PayrollImportStatus.cs      ← enum（取込済 or パース失敗）
│
├── Repositories/                   ← DBとのやり取りだけを担当
│   ├── IPayrollRepository.cs       ← インターフェース（約束書）
│   ├── SqlitePayrollRepository.cs  ← 実装（実際に SQL を書く）
│   ├── IAnnualIncomeRepository.cs
│   └── SqliteAnnualIncomeRepository.cs
│
├── Services/                       ← ビジネスロジック
│   ├── PayrollPdfParser.cs         ← PDF から金額を抽出
│   ├── PayrollIngestionService.cs  ← 取込の一連の流れを管理
│   ├── IFileStorageService.cs      ← ファイル保存のインターフェース
│   └── LocalFileStorageService.cs  ← ローカルディスクに保存
│
├── Pages/                          ← 画面ごとのファイル（ペアになっている）
│   ├── Index.cshtml                ← ダッシュボードの HTML
│   ├── Index.cshtml.cs             ← ダッシュボードの C# ロジック
│   ├── Import.cshtml               ← PDF 取込画面の HTML
│   ├── Import.cshtml.cs            ← PDF 取込画面の C# ロジック
│   ├── SlipDetail.cshtml           ← 明細詳細の HTML
│   ├── SlipDetail.cshtml.cs        ← 明細詳細の C# ロジック
│   └── Shared/_Layout.cshtml       ← 全ページ共通のレイアウト
│
└── Controllers/
    └── PayrollController.cs        ← API（PDF 配信など）
```

---

## 3. Models — データの型を定義する

```csharp
// PayrollSlip.cs
public class PayrollSlip
{
    public int     Id           { get; set; }
    public string  PayrollMonth { get; set; } = string.Empty;
    public decimal? GrossAmount  { get; set; }   // ? = null になり得る
    public decimal? NetAmount    { get; set; }
    // ...
}
```

### ポイント：Nullable 型（`?`）

`decimal?` の `?` は「Nullable 型」。  
「値がない可能性がある」ことを型で表現できる。

```csharp
decimal  amount = null;  // ❌ コンパイルエラー
decimal? amount = null;  // ✅ OK
```

PDF のパースに失敗した場合、金額が取れないので `null` になる。  
`null` かどうかは `HasValue` で確認できる。

```csharp
if (slip.GrossAmount.HasValue)
{
    Console.WriteLine(slip.GrossAmount.Value);
}

// または短く書く
Console.WriteLine(slip.GrossAmount?.ToString("N0") ?? "-");
//                               ↑ null なら              ↑ こっちを使う
```

### ポイント：enum

「給与か賞与か」のような決まった選択肢は `enum` で表す。  
文字列 `"Salary"` を直接使うとタイプミスで気づきにくいバグが起きる。

```csharp
public enum PayrollSlipType
{
    Salary,  // 給与
    Bonus    // 賞与
}

// 使う側
if (slip.SlipType == PayrollSlipType.Salary) { ... }
```

---

## 4. Program.cs — 誰が誰の仕事をするかを登録する

```csharp
// Program.cs
builder.Services.AddScoped<IPayrollRepository, SqlitePayrollRepository>();
//                          ↑ インターフェース   ↑ 実際のクラス
```

これが **DI（Dependency Injection / 依存性の注入）**。

### たとえ話

会社で「経理担当者が必要」と言うとき、  
「鈴木さんを指名」するのではなく「経理ができる人」と依頼する感覚。

| 概念 | たとえ | このプロジェクト |
|------|--------|---------------|
| インターフェース | 「経理ができる人」という条件 | `IPayrollRepository` |
| 実装クラス | 実際に仕事をする鈴木さん | `SqlitePayrollRepository` |

### なぜ DI を使うのか

将来 SQLite を PostgreSQL に変えるとき、  
`SqlitePayrollRepository` を差し替えるだけで他のコードは変えずに済む。

```csharp
// SQLite から PostgreSQL に変えるときはここだけ書き換える
builder.Services.AddScoped<IPayrollRepository, PostgresPayrollRepository>();
//                                              ↑ここだけ変える
```

### `AddScoped` とは

リクエストごとに 1 つのインスタンスを作る設定。  
他にも `AddSingleton`（アプリ全体で 1 つ）、`AddTransient`（都度作る）がある。

---

## 5. Repositories — DB とのやり取りだけを担当

### インターフェース（約束書）

```csharp
// IPayrollRepository.cs
public interface IPayrollRepository
{
    void Save(PayrollSlip slip);              // 保存
    IEnumerable<PayrollSlip> GetAll();        // 全件取得
    PayrollSlip? GetById(int id);             // 1件取得
    bool ExistsByHash(string hash);           // 重複チェック
}
```

インターフェースはメソッドの「名前」と「引数・戻り値の型」だけを定義する。  
**中身（実装）は書かない**。実装は別クラスに任せる。

### 実装クラス

```csharp
// SqlitePayrollRepository.cs
public void Save(PayrollSlip slip)
{
    using var connection = new SqliteConnection(_connectionString);
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = """
        INSERT INTO PayrollSlips (PayrollMonth, GrossAmount, ...)
        VALUES ($month, $gross, ...)
        """;

    command.Parameters.AddWithValue("$month", slip.PayrollMonth);
    command.Parameters.AddWithValue("$gross", slip.GrossAmount ?? (object)DBNull.Value);

    command.ExecuteNonQuery();
}
```

### ポイント：`using var`

```csharp
using var connection = new SqliteConnection(...);
```

`using` は「このブロックを抜けたら自動的にリソースを閉じる」仕組み。  
DB 接続は使い終わったら必ず閉じないとメモリリークが起きるため必須。

### ポイント：SQL インジェクション対策

```csharp
// ❌ 危険：文字列を直接埋め込む
command.CommandText = $"SELECT * FROM PayrollSlips WHERE Id = {id}";

// ✅ 安全：パラメータを使う
command.CommandText = "SELECT * FROM PayrollSlips WHERE Id = $id";
command.Parameters.AddWithValue("$id", id);
```

---

## 6. Services — ビジネスロジックをここに集める

### PayrollIngestionService — 取込の流れを管理

```csharp
public class PayrollIngestionService
{
    // コンストラクタで DI から部品を受け取る
    public PayrollIngestionService(
        IPayrollRepository  repository,
        IFileStorageService fileStorage,
        PayrollPdfParser    parser)
    {
        _repository  = repository;
        _fileStorage = fileStorage;
        _parser      = parser;
    }

    public PayrollSlip Import(IFormFile file, string payrollMonth)
    {
        var filePath    = _fileStorage.SaveFile(file);   // ① ファイル保存
        var hash        = ComputeFileHash(filePath);      // ② ハッシュ計算
        var parseResult = _parser.Parse(filePath);        // ③ PDF 解析

        var slip = new PayrollSlip                        // ④ オブジェクト作成
        {
            PayrollMonth  = payrollMonth,
            GrossAmount   = parseResult.GrossAmount,
            ImportStatus  = parseResult.Success
                            ? PayrollImportStatus.Parsed
                            : PayrollImportStatus.ParseFailed,
        };

        _repository.Save(slip);                          // ⑤ DB 保存
        return slip;
    }
}
```

1ファイルのインポートで ① → ② → ③ → ④ → ⑤ の順で処理が進む。  
各ステップを別クラスに分けているので、「パース処理だけ変えたい」が簡単にできる。

### PayrollPdfParser — PDF から金額を取り出す

```csharp
public PayrollParseResult Parse(string filePath)
{
    using var document = PdfDocument.Open(filePath);
    var page  = document.GetPage(1);
    var words = page.GetWords().ToList();

    // 「【総支給額】」というラベルの下にある数字を座標で探す
    var grossAmount = FindAmountBelowLabel(words, "【総支給額】");
    // ...
}
```

PdfPig というライブラリで PDF をテキストと座標に変換し、  
ラベルの近くにある数字を座標の範囲で絞り込んで取得している。

---

## 7. Pages — 画面の構成（Razor Pages）

各画面は **`.cshtml`（HTML）** と **`.cshtml.cs`（C# ロジック）** のペア。

### PageModel（C# 側）

```csharp
// Index.cshtml.cs
public class IndexModel : PageModel
{
    private readonly IPayrollRepository _repository;

    // DI でリポジトリが自動で渡される
    public IndexModel(IPayrollRepository repository)
    {
        _repository = repository;
    }

    // プロパティ = cshtml から参照できるデータ
    public List<PayrollSlip> SalarySlips { get; set; } = [];

    // GET リクエスト時に呼ばれる
    public void OnGet()
    {
        var all     = _repository.GetAll().ToList();
        SalarySlips = all.Where(s => s.SlipType == PayrollSlipType.Salary).ToList();
    }
}
```

### cshtml（HTML 側）

```html
@* C# の値を @で埋め込む *@
@foreach (var slip in Model.SalarySlips)
{
    <tr onclick="goToSlip(@slip.Id, 'salary')">
        <td>@slip.PayrollMonth</td>
        <td>@(slip.GrossAmount?.ToString("N0") ?? "-")</td>
    </tr>
}
```

### POST（フォーム送信）

```csharp
// Import.cshtml.cs
[BindProperty]                        // フォームの input と自動で紐付く
public IFormFile?  UploadFile   { get; set; }

[BindProperty]
public string?     PayrollMonth { get; set; }

// POST リクエスト時に呼ばれる
public IActionResult OnPost()
{
    var slip = _ingestionService.Import(UploadFile!, PayrollMonth!);
    return Page();  // 同じページを再描画
}
```

`[BindProperty]` をつけると、フォームの `name` 属性と一致するプロパティに  
値が自動でセットされる。

### Handler 名（複数の POST ボタン）

```html
<form method="post" asp-page-handler="ImportAll">
    <button type="submit">一括取込</button>
</form>
```

```csharp
// "ImportAll" → OnPost**ImportAll**() が呼ばれる
public IActionResult OnPostImportAll() { ... }
public IActionResult OnPost()         { ... }  // handler なし → 通常の POST
```

---

## 8. Controllers — JSON や PDF を返す API

Razor Pages と違い、HTML ではなくデータ（JSON / バイナリ）を返す。

```csharp
[ApiController]
[Route("api/[controller]")]          // → /api/payroll/...
public class PayrollController : ControllerBase
{
    // GET /api/payroll/slips/3/pdf
    [HttpGet("slips/{id}/pdf")]
    public IActionResult GetSlipPdf([FromRoute] int id)
    {
        var slip  = _repository.GetById(id);
        var bytes = File.ReadAllBytes(slip.SourceFilePath);

        Response.Headers["Content-Disposition"] = "inline";
        return File(bytes, "application/pdf");   // PDF バイトをそのまま返す
    }
}
```

これを画面側の `<object>` タグで埋め込む。

```html
<object data="/api/payroll/slips/3/pdf" type="application/pdf" width="100%" height="900">
</object>
```

---

## 9. データが画面に表示されるまでの流れ

### ダッシュボード表示

```
ブラウザ: GET /
    ↓
IndexModel.OnGet() が呼ばれる
    ↓
IPayrollRepository.GetAll() を呼ぶ
    ↓
SqlitePayrollRepository が SQL を実行して List<PayrollSlip> を返す
    ↓
SalarySlips / BonusSlips に振り分け
    ↓
Index.cshtml が @Model.SalarySlips を foreach でループして HTML に変換
    ↓
ブラウザに HTML が届く
```

### PDF 取込

```
ブラウザ: POST /Import（ファイル添付）
    ↓
ImportModel.OnPost() が呼ばれる
    ↓
PayrollIngestionService.Import()
    ├── LocalFileStorageService.SaveFile()  → ディスクに保存
    ├── SHA256 でハッシュ計算（重複防止）
    ├── PayrollPdfParser.Parse()            → PDF から金額を抽出
    └── IPayrollRepository.Save()           → DB に保存
    ↓
ページに結果を表示
```

---

## 10. C# の重要概念まとめ

| 概念 | 説明 | このプロジェクトでの例 |
|------|------|---------------------|
| **interface** | メソッドの約束書。実装は別クラスに任せる | `IPayrollRepository` |
| **DI** | 必要な部品をコンストラクタで受け取る仕組み | `Program.cs` で登録 |
| **Nullable 型** | `?` をつけると null になれる | `decimal? GrossAmount` |
| **LINQ** | リストをクエリっぽく操作できる | `.Where()` `.OrderBy()` `.Select()` |
| **enum** | 決まった選択肢を型で表す | `PayrollSlipType.Salary` |
| **using** | スコープを抜けたらリソースを自動で閉じる | DB 接続・ファイル操作 |
| **PageModel** | Razor Pages の C# 側クラス | `IndexModel`, `ImportModel` |
| **[BindProperty]** | フォームの値を自動でプロパティにマッピング | `UploadFile`, `PayrollMonth` |
| **OnGet / OnPost** | GET・POST リクエストに対応するメソッド | `OnGet()`, `OnPostImportAll()` |

### LINQ のよく使うメソッド

```csharp
var slips = _repository.GetAll().ToList();

// 絞り込み
var salary = slips.Where(s => s.SlipType == PayrollSlipType.Salary);

// 並び替え
var sorted = slips.OrderBy(s => s.PayrollMonth);

// 変換
var months = slips.Select(s => s.PayrollMonth);

// 最初の1件（なければ null）
var latest = slips.LastOrDefault();

// 存在チェック
bool hasBonus = slips.Any(s => s.SlipType == PayrollSlipType.Bonus);

// 平均
double avg = slips.Where(s => s.GrossAmount.HasValue)
                  .Average(s => (double)s.GrossAmount!.Value);
```

---

> **次に学ぶとよいこと**
> - Entity Framework Core（Repository を自分で書かずに ORM に任せる）
> - async / await（非同期処理で DB 待ちをブロックしない）
> - xUnit によるユニットテスト（インターフェースのおかげでモックが使える）
