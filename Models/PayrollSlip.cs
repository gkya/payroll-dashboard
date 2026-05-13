namespace PayrollDashboard.Models;

public class PayrollSlip
{
    public int Id { get; set; }

    public string PayrollMonth { get; set; } = string.Empty;

    public string SourceFileName { get; set; } = string.Empty;

    public string SourceFilePath { get; set; } = string.Empty;

    public PayrollImportStatus ImportStatus { get; set; } = PayrollImportStatus.Imported;

    public string? ParseMessage { get; set; }

    public DateTimeOffset ImportedAt { get; set; }
}
