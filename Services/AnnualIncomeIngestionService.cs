using System.Text.RegularExpressions;
using PayrollDashboard.Models;
using PayrollDashboard.Repositories;

namespace PayrollDashboard.Services;

public class AnnualIncomeIngestionService
{
    private readonly IAnnualIncomeRepository _repository;
    private readonly AnnualIncomePdfParser _parser;

    public AnnualIncomeIngestionService(IAnnualIncomeRepository repository, AnnualIncomePdfParser parser)
    {
        _repository = repository;
        _parser = parser;
    }

    public static string ExtractYearFromFileName(string fileName)
    {
        // 令和N年 → 西暦 (令和元年=2019, 令和N年=N+2018)
        var reiwa = Regex.Match(fileName, @"令和(\d+)年");
        if (reiwa.Success)
            return (int.Parse(reiwa.Groups[1].Value) + 2018).ToString();

        var western = Regex.Match(fileName, @"(\d{4})年");
        if (western.Success)
            return western.Groups[1].Value;

        return string.Empty;
    }

    public IReadOnlyList<AnnualIncomeSlip> ImportAllFromDirectory(string directory)
    {
        var results = new List<AnnualIncomeSlip>();
        foreach (var filePath in Directory.GetFiles(directory, "*.pdf"))
        {
            var hash = PayrollIngestionService.ComputeFileHash(filePath);
            if (_repository.ExistsByHash(hash)) continue;

            var fileName = Path.GetFileName(filePath);
            var year = ExtractYearFromFileName(fileName);
            var parseResult = _parser.Parse(filePath);

            var slip = new AnnualIncomeSlip
            {
                FiscalYear = year,
                SourceFileName = fileName,
                SourceFilePath = filePath,
                SourceFileHash = hash,
                ImportStatus = parseResult.Success ? PayrollImportStatus.Parsed : PayrollImportStatus.ParseFailed,
                TotalIncome = parseResult.TotalIncome,
                AfterDeduction = parseResult.AfterDeduction,
                TotalDeductions = parseResult.TotalDeductions,
                WithholdingTax = parseResult.WithholdingTax,
                ParseMessage = parseResult.Message,
                ImportedAt = DateTimeOffset.UtcNow,
            };

            _repository.Save(slip);
            results.Add(slip);
        }
        return results;
    }
}
