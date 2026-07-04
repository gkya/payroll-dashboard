using PayrollDashboard.Models;
using PayrollDashboard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayrollDashboard.Pages;

public class ImportModel : PageModel
{
    private readonly ILogger<ImportModel> _logger;
    private readonly PayrollIngestionService _ingestionService;
    private readonly AnnualIncomeIngestionService _annualIncomeIngestionService;

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public string? PayrollMonth { get; set; }

    [BindProperty]
    public PayrollSlipType SlipType { get; set; } = PayrollSlipType.Salary;

    public string? StatusMessage { get; private set; }
    public bool IsError { get; private set; }

    public ImportModel(ILogger<ImportModel> logger, PayrollIngestionService ingestionService, AnnualIncomeIngestionService annualIncomeIngestionService)
    {
        _logger = logger;
        _ingestionService = ingestionService;
        _annualIncomeIngestionService = annualIncomeIngestionService;
    }

    public void OnGet()
    {
        _logger.LogInformation("Import page was opened.");
    }

    public IActionResult OnPostImportAll()
    {
        var root      = Path.Combine(Directory.GetCurrentDirectory(), "datas");
        var salaryDir = Path.Combine(root, "salary");
        var bonusDir  = Path.Combine(root, "bonus");

        var count = 0;
        if (Directory.Exists(salaryDir))
            count += _ingestionService.ImportAllFromDirectory(salaryDir, PayrollSlipType.Salary).Count;
        if (Directory.Exists(bonusDir))
            count += _ingestionService.ImportAllFromDirectory(bonusDir, PayrollSlipType.Bonus).Count;

        TempData["ImportMessage"] = count == 0
            ? "新しい PDF はありませんでした。"
            : $"{count} 件取り込みました。";

        return RedirectToPage("/Index");
    }

    public IActionResult OnPostImportAnnualIncome()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "datas", "annual_income");
        if (!Directory.Exists(dir))
        {
            StatusMessage = "datas/annual_income フォルダが見つかりません。";
            IsError = true;
            return Page();
        }

        var count = _annualIncomeIngestionService.ImportAllFromDirectory(dir).Count;
        TempData["ImportMessage"] = count == 0
            ? "新しい PDF はありませんでした。"
            : $"{count} 件取り込みました。";

        return RedirectToPage("/Index");
    }

    public IActionResult OnPost()
    {
        if (UploadFile == null)
        {
            StatusMessage = "ファイルを選択してください。";
            IsError = true;
            return Page();
        }

        if (string.IsNullOrEmpty(PayrollMonth))
            PayrollMonth = PayrollIngestionService.ExtractMonthFromFileName(UploadFile.FileName) ?? string.Empty;

        if (string.IsNullOrEmpty(PayrollMonth))
        {
            StatusMessage = "支給月を入力してください。ファイル名から自動取得できませんでした。";
            IsError = true;
            return Page();
        }

        var (slip, isDuplicate) = _ingestionService.Import(UploadFile, PayrollMonth, SlipType);

        if (isDuplicate)
        {
            StatusMessage = "このファイルはすでに取り込み済みです（重複）。";
            IsError = true;
            return Page();
        }

        TempData["ImportMessage"] = $"{slip!.SourceFileName} を取り込みました。";
        return RedirectToPage("/Index");
    }
}
