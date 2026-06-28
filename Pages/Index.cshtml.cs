using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPayrollRepository _repository;

    public List<PayrollSlip> PayrollSlips { get; set; } = [];

    public IndexModel(ILogger<IndexModel> logger, IPayrollRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public void OnGet()
    {
        _logger.LogInformation("Dashboard page was opened.");
        PayrollSlips = _repository.GetAll().ToList();
    }
}
