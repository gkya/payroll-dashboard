using Microsoft.AspNetCore.Mvc.RazorPages;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IPayrollRepository _repository;
    private readonly IAnnualIncomeRepository _annualIncomeRepository;

    public List<PayrollSlip> PayrollSlips { get; set; } = [];
    public List<PayrollSlip> SalarySlips { get; set; } = [];
    public List<PayrollSlip> BonusSlips { get; set; } = [];
    public List<AnnualIncomeSlip> AnnualIncomeSlips { get; set; } = [];

    public IndexModel(ILogger<IndexModel> logger, IPayrollRepository repository, IAnnualIncomeRepository annualIncomeRepository)
    {
        _logger = logger;
        _repository = repository;
        _annualIncomeRepository = annualIncomeRepository;
    }

    public void OnGet()
    {
        _logger.LogInformation("Dashboard page was opened.");
        PayrollSlips = _repository.GetAll().ToList();
        SalarySlips = [.. PayrollSlips.Where(s => s.SlipType == PayrollSlipType.Salary)];
        BonusSlips  = [.. PayrollSlips.Where(s => s.SlipType == PayrollSlipType.Bonus)];
        AnnualIncomeSlips = [.. _annualIncomeRepository.GetAll()];
    }
}
