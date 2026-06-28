namespace PayrollDashboard.Services;

public class AnnualIncomeParseResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public decimal? TotalIncome { get; set; }
    public decimal? AfterDeduction { get; set; }
    public decimal? TotalDeductions { get; set; }
    public decimal? WithholdingTax { get; set; }
}
