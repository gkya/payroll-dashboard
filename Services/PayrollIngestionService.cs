using System.Text.RegularExpressions;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Services;

public class PayrollIngestionService
{
  private readonly IPayrollRepository _repository;
  private readonly IFileStorageService _fileStorage;
  private readonly PayrollPdfParser _parser;

  public PayrollIngestionService(IPayrollRepository repository, IFileStorageService fileStorage, PayrollPdfParser parser)
  {
    _repository = repository;
    _fileStorage = fileStorage;
    _parser = parser;
  }

  public static string? ExtractMonthFromFileName(string fileName)
  {
    var match = Regex.Match(fileName, @"(\d{4})年(\d{1,2})月");
    if (!match.Success) return null;

    var year = match.Groups[1].Value;
    var month = match.Groups[2].Value.PadLeft(2, '0');
    return $"{year}-{month}";
  }

  public PayrollSlip Import(IFormFile file, string payrollMonth)
  {
    var filePath = _fileStorage.SaveFile(file);
    var parseResult = _parser.Parse(filePath);

    var slip = new PayrollSlip
    {
      PayrollMonth = payrollMonth,
      SourceFileName = file.FileName,
      SourceFilePath = filePath,
      ImportStatus = parseResult.Success ? PayrollImportStatus.Parsed : PayrollImportStatus.ParseFailed,
      GrossAmount = parseResult.GrossAmount,
      DeductionAmount = parseResult.DeductionAmount,
      NetAmount = parseResult.NetAmount,
      ParseMessage = parseResult.Message,
      ImportedAt = DateTimeOffset.UtcNow
    };

    _repository.Save(slip);

    return slip;
  }
}