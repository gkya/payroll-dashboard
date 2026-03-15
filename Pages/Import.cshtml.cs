using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayrollDashboard.Pages;

public class ImportModel : PageModel
{
    private readonly ILogger<ImportModel> _logger;

    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    [BindProperty]
    public string? PayrollMonth { get; set; }

    public string? StatusMessage { get; private set; }

    public ImportModel(ILogger<ImportModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Import page was opened.");
    }

    public IActionResult OnPost()
    {
        if (UploadFile is null || UploadFile.Length == 0)
        {
            StatusMessage = "PDF ファイルを選択してください。";
            return Page();
        }

        _logger.LogInformation(
            "Payroll upload form posted. FileName: {FileName}, PayrollMonth: {PayrollMonth}",
            UploadFile.FileName,
            PayrollMonth ?? "(empty)"
        );

        StatusMessage = $"アップロード受信: {UploadFile.FileName}";
        return Page();
    }
}
