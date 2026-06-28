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

  public IReadOnlyList<PayrollSlip> ImportAllFromDirectory(string directory, PayrollSlipType slipType)
  {
    var results = new List<PayrollSlip>();
    foreach (var filePath in Directory.GetFiles(directory, "*.pdf"))
    {
      var fileName = Path.GetFileName(filePath);
      if (_repository.ExistsByFileName(fileName)) continue;

      var month = ExtractMonthFromFileName(fileName) ?? string.Empty;
      var slip = ImportFromPath(filePath, fileName, month, slipType);
      results.Add(slip);
    }
    return results;
  }

  private PayrollSlip ImportFromPath(string filePath, string fileName, string payrollMonth, PayrollSlipType slipType)
  {
    var parseResult = _parser.Parse(filePath);

    var slip = new PayrollSlip
    {
      SlipType = slipType,
      PayrollMonth = payrollMonth,
      SourceFileName = fileName,
      SourceFilePath = filePath,
      ImportStatus = parseResult.Success ? PayrollImportStatus.Parsed : PayrollImportStatus.ParseFailed,
      GrossAmount = parseResult.GrossAmount,
      DeductionAmount = parseResult.DeductionAmount,
      NetAmount = parseResult.NetAmount,
      BasicAmount = parseResult.BasicAmount,
      OvertimeHours = parseResult.OvertimeHours,
      ParseMessage = parseResult.Message,
      ImportedAt = DateTimeOffset.UtcNow
    };

    _repository.Save(slip);
    return slip;
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
      BasicAmount = parseResult.BasicAmount,
      OvertimeHours = parseResult.OvertimeHours,
      ParseMessage = parseResult.Message,
      ImportedAt = DateTimeOffset.UtcNow
    };

    _repository.Save(slip);

    return slip;
  }
}