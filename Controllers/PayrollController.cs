using Microsoft.AspNetCore.Mvc;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PayrollController : ControllerBase
{
    private readonly IPayrollRepository _repository;
    private readonly ILogger<PayrollController> _logger;

    public PayrollController(IPayrollRepository repository, ILogger<PayrollController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    [HttpGet("slips")]
    public ActionResult<IEnumerable<PayrollSlip>> GetAllSlips()
    {
        _logger.LogInformation("API: GetAllSlips called");
        return Ok(_repository.GetAll());
    }

    [HttpGet("slips/{id}")]
    public ActionResult<PayrollSlip> GetSlipById([FromRoute] int id)
    {
        _logger.LogInformation("API: GetSlipById called with id={Id}", id);
        var slip = _repository.GetById(id);

        if (slip == null)
        {
            _logger.LogWarning("Slip not found: id={Id}", id);
            return NotFound();
        }

        return Ok(slip);
    }
}
