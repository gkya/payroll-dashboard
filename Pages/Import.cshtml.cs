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

    public string? StatusMessage { get; private set; }

    public PayrollSlip? CurrentSlip { get; private set; }

    public ImportModel(ILogger<ImportModel> logger, PayrollIngestionService ingestionService, AnnualIncomeIngestionService annualIncomeIngestionService)
    {
        _logger = logger;
        _ingestionService = ingestionService;
        _annualIncomeIngestionService = annualIncomeIngestionService;
    }

    public string? BulkMessage { get; private set; }
    public string? AnnualBulkMessage { get; private set; }

    public void OnGet()
    {
        _logger.LogInformation("Import page was opened.");
    }

    public IActionResult OnPostImportAll()
    {
        var root = Path.Combine(Directory.GetCurrentDirectory(), "datas");
        var salaryDir = Path.Combine(root, "salary");
        var bonusDir  = Path.Combine(root, "bonus");

        var count = 0;
        if (Directory.Exists(salaryDir))
            count += _ingestionService.ImportAllFromDirectory(salaryDir, PayrollSlipType.Salary).Count;
        if (Directory.Exists(bonusDir))
            count += _ingestionService.ImportAllFromDirectory(bonusDir, PayrollSlipType.Bonus).Count;

        BulkMessage = count == 0 ? "新しい PDF はありませんでした。" : $"{count} 件取り込みました。";
        return Page();
    }

    public IActionResult OnPostImportAnnualIncome()
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "datas", "annual_income");
        if (!Directory.Exists(dir))
        {
            AnnualBulkMessage = "datas/annual_income フォルダが見つかりません。";
            return Page();
        }

        var count = _annualIncomeIngestionService.ImportAllFromDirectory(dir).Count;
        AnnualBulkMessage = count == 0 ? "新しい PDF はありませんでした。" : $"{count} 件取り込みました。";
        return Page();
    }

    public IActionResult OnPost()
    {
        if (UploadFile == null)
        {
            StatusMessage = "Please select a file.";
            return Page();
        }

        if (string.IsNullOrEmpty(PayrollMonth))
            PayrollMonth = PayrollIngestionService.ExtractMonthFromFileName(UploadFile.FileName) ?? string.Empty;

        if (string.IsNullOrEmpty(PayrollMonth))
        {
            StatusMessage = "支給月を入力してください。ファイル名から自動取得できませんでした。";
            return Page();
        }

        var slip = _ingestionService.Import(UploadFile, PayrollMonth);
        CurrentSlip = slip;
        StatusMessage = "File imported successfully.";

        return Page();
    }
}
