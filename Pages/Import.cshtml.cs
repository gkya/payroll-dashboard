using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PayrollDashboard.Pages;

public class ImportModel : PageModel
{
    private readonly ILogger<ImportModel> _logger;

    public ImportModel(ILogger<ImportModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        _logger.LogInformation("Import page was opened.");
    }
}
