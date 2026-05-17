using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Services;

public class PayrollIngestionService
{
  private readonly IPayrollRepository _repository;

  public PayrollIngestionService(IPayrollRepository repository)
  {
    _repository = repository;
  }
  
  public PayrollSlip Import(IFormFile file, string payrollMonth)
  {
    var slip = new PayrollSlip
    {
      PayrollMonth = payrollMonth,
      SourceFileName = file.FileName,
      SourceFilePath = $"uploads/{file.FileName}",
      ImportStatus = PayrollImportStatus.Imported,
      ImportedAt = DateTimeOffset.UtcNow
    };

    _repository.Save(slip);

    return slip;
  }
}