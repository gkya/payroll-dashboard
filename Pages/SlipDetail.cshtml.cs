using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Pages;

public class SlipDetailModel : PageModel
{
    private readonly IPayrollRepository _repository;

    public PayrollSlip? Slip { get; set; }
    public PayrollSlip? PrevSlip { get; set; }
    public PayrollSlip? NextSlip { get; set; }

    public SlipDetailModel(IPayrollRepository repository)
    {
        _repository = repository;
    }

    public IActionResult OnGet(int id)
    {
        Slip = _repository.GetById(id);
        if (Slip == null) return NotFound();

        var ordered = _repository.GetAll()
            .Where(s => s.SlipType == Slip.SlipType)
            .OrderBy(s => s.PayrollMonth)
            .ToList();

        var idx = ordered.FindIndex(s => s.Id == id);
        PrevSlip = idx > 0 ? ordered[idx - 1] : null;
        NextSlip = idx < ordered.Count - 1 ? ordered[idx + 1] : null;

        return Page();
    }
}
