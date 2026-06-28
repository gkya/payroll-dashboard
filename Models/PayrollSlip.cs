namespace PayrollDashboard.Models;

public class PayrollSlip
{
    public int Id { get; set; }

    public PayrollSlipType SlipType { get; set; } = PayrollSlipType.Salary;

    public string PayrollMonth { get; set; } = string.Empty;

    public string SourceFileName { get; set; } = string.Empty;

    public string SourceFilePath { get; set; } = string.Empty;

    public string SourceFileHash { get; set; } = string.Empty;

    public PayrollImportStatus ImportStatus { get; set; } = PayrollImportStatus.Imported;

    public decimal? GrossAmount { get; set; }

    public decimal? DeductionAmount { get; set; }

    public decimal? NetAmount { get; set; }

    public decimal? BasicAmount { get; set; }

    public decimal? OvertimeHours { get; set; }

    public string? ParseMessage { get; set; }

    public DateTimeOffset ImportedAt { get; set; }
}
