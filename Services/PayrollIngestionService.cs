using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Services;

public class PayrollIngestionService
{
  private readonly IPayrollRepository _repository;
  private readonly IFileStorageService _fileStorage;

  public PayrollIngestionService(IPayrollRepository repository, IFileStorageService fileStorage)
  {
    _repository = repository;
    _fileStorage = fileStorage;
  }

  public PayrollSlip Import(IFormFile file, string payrollMonth)
  {
    var filePath = _fileStorage.SaveFile(file);

    var slip = new PayrollSlip
    {
      PayrollMonth = payrollMonth,
      SourceFileName = file.FileName,
      SourceFilePath = filePath,
      ImportStatus = PayrollImportStatus.Imported,
      ImportedAt = DateTimeOffset.UtcNow
    };

    _repository.Save(slip);

    return slip;
  }
}