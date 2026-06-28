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

  /// <summary>
  /// 全ての給与明細を取得する
  /// </summary>
  /// <returns>取込済み給与明細のリスト</returns>
  [HttpGet("slips")]
  public ActionResult<IEnumerable<PayrollSlip>> GetAllSlips()
  {
    _logger.LogInformation("API: GetAllSlips called");
    var slips = _repository.GetAll();
    return Ok(slips);
  }

  /// <summary>
  /// 指定した ID の給与明細を取得する
  /// </summary>
  /// <param name="id">給与明細 ID</param>
  /// <returns>給与明細情報</returns>
  [HttpGet("slips/{id}")]
  public ActionResult<PayrollSlip> GetSlipById(int id)
  {
    _logger.LogInformation("API: GetSlipById called with id={Id}", id);
    var slip = _repository.GetAll().FirstOrDefault(s => s.Id == id);

    if (slip == null)
    {
      _logger.LogWarning("Slip not found with id={Id}", id);
      return NotFound();
    }

    return Ok(slip);
  }
}
