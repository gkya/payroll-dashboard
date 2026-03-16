namespace PayrollDashboard.Models;

public class PayrollSlip
{
    public string FileName { get; set; } = string.Empty;

    public string PayrollMonth { get; set; } = string.Empty;

    public DateTimeOffset ImportedAt { get; set; }
}
