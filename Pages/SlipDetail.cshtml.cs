using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Pages;

public class SlipDetailModel : PageModel
{
    private readonly IPayrollRepository _repository;

    public PayrollSlip? Slip { get; set; }

    public SlipDetailModel(IPayrollRepository repository)
    {
        _repository = repository;
    }

    public IActionResult OnGet(int id)
    {
        Slip = _repository.GetById(id);
        if (Slip == null) return NotFound();
        return Page();
    }
}
