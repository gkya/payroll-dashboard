namespace PayrollDashboard.Services;

public class PayrollParseResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public decimal? GrossAmount { get; set; }
    public decimal? DeductionAmount { get; set; }
    public decimal? NetAmount { get; set; }
    public decimal? BasicAmount { get; set; }
    public decimal? OvertimeHours { get; set; }
}
