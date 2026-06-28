namespace PayrollDashboard.Models;

public class AnnualIncomeSlip
{
    public int Id { get; set; }
    public string FiscalYear { get; set; } = string.Empty;
    public string SourceFileName { get; set; } = string.Empty;
    public string SourceFilePath { get; set; } = string.Empty;
    public PayrollImportStatus ImportStatus { get; set; } = PayrollImportStatus.Imported;
    public decimal? TotalIncome { get; set; }       // 支払金額
    public decimal? AfterDeduction { get; set; }    // 給与所得控除後の金額
    public decimal? TotalDeductions { get; set; }   // 所得控除の額の合計額
    public decimal? WithholdingTax { get; set; }    // 源泉徴収税額
    public string? ParseMessage { get; set; }
    public DateTimeOffset ImportedAt { get; set; }
}
