using Microsoft.Data.Sqlite;
using PayrollDashboard.Models;

namespace PayrollDashboard.Repositories;

public class SqlitePayrollRepository : IPayrollRepository
{
    private readonly string _connectionString;

    public SqlitePayrollRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Sqlite")
            ?? "Data Source=payroll.db";

        InitializeDatabase();
    }

    public void Save(PayrollSlip slip)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO PayrollSlips (SlipType, PayrollMonth, SourceFileName, SourceFilePath, ImportStatus, GrossAmount, DeductionAmount, NetAmount, BasicAmount, OvertimeHours, ParseMessage, ImportedAt)
            VALUES ($slipType, $month, $fileName, $filePath, $status, $gross, $deduction, $net, $basic, $overtimeHours, $message, $importedAt)
            """;

        command.Parameters.AddWithValue("$slipType", slip.SlipType.ToString());
        command.Parameters.AddWithValue("$month", slip.PayrollMonth);
        command.Parameters.AddWithValue("$fileName", slip.SourceFileName);
        command.Parameters.AddWithValue("$filePath", slip.SourceFilePath);
        command.Parameters.AddWithValue("$status", slip.ImportStatus.ToString());
        command.Parameters.AddWithValue("$gross", slip.GrossAmount.HasValue ? (object)slip.GrossAmount.Value : DBNull.Value);
        command.Parameters.AddWithValue("$deduction", slip.DeductionAmount.HasValue ? (object)slip.DeductionAmount.Value : DBNull.Value);
        command.Parameters.AddWithValue("$net", slip.NetAmount.HasValue ? (object)slip.NetAmount.Value : DBNull.Value);
        command.Parameters.AddWithValue("$basic", slip.BasicAmount.HasValue ? (object)slip.BasicAmount.Value : DBNull.Value);
        command.Parameters.AddWithValue("$overtimeHours", slip.OvertimeHours.HasValue ? (object)slip.OvertimeHours.Value : DBNull.Value);
        command.Parameters.AddWithValue("$message", slip.ParseMessage ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$importedAt", slip.ImportedAt.ToString("O"));

        command.ExecuteNonQuery();
    }

    public IEnumerable<PayrollSlip> GetAll()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, SlipType, PayrollMonth, SourceFileName, SourceFilePath, ImportStatus, GrossAmount, DeductionAmount, NetAmount, BasicAmount, OvertimeHours, ParseMessage, ImportedAt
            FROM PayrollSlips
            ORDER BY PayrollMonth ASC
            """;

        var slips = new List<PayrollSlip>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            slips.Add(new PayrollSlip
            {
                Id = reader.GetInt32(0),
                SlipType = Enum.Parse<PayrollSlipType>(reader.GetString(1)),
                PayrollMonth = reader.GetString(2),
                SourceFileName = reader.GetString(3),
                SourceFilePath = reader.GetString(4),
                ImportStatus = Enum.Parse<PayrollImportStatus>(reader.GetString(5)),
                GrossAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                DeductionAmount = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                NetAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                BasicAmount = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                OvertimeHours = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
                ParseMessage = reader.IsDBNull(11) ? null : reader.GetString(11),
                ImportedAt = DateTimeOffset.Parse(reader.GetString(12)),
            });
        }

        return slips;
    }

    public PayrollSlip? GetById(int id)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, SlipType, PayrollMonth, SourceFileName, SourceFilePath, ImportStatus, GrossAmount, DeductionAmount, NetAmount, BasicAmount, OvertimeHours, ParseMessage, ImportedAt
            FROM PayrollSlips
            WHERE Id = $id
            """;
        command.Parameters.AddWithValue("$id", id);

        using var reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return new PayrollSlip
        {
            Id = reader.GetInt32(0),
            SlipType = Enum.Parse<PayrollSlipType>(reader.GetString(1)),
            PayrollMonth = reader.GetString(2),
            SourceFileName = reader.GetString(3),
            SourceFilePath = reader.GetString(4),
            ImportStatus = Enum.Parse<PayrollImportStatus>(reader.GetString(5)),
            GrossAmount = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
            DeductionAmount = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
            NetAmount = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
            BasicAmount = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
            OvertimeHours = reader.IsDBNull(10) ? null : reader.GetDecimal(10),
            ParseMessage = reader.IsDBNull(11) ? null : reader.GetString(11),
            ImportedAt = DateTimeOffset.Parse(reader.GetString(12)),
        };
    }

    public bool ExistsByFileName(string fileName)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM PayrollSlips WHERE SourceFileName = $fileName";
        command.Parameters.AddWithValue("$fileName", fileName);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS PayrollSlips (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                SlipType        TEXT    NOT NULL DEFAULT 'Salary',
                PayrollMonth    TEXT    NOT NULL,
                SourceFileName  TEXT    NOT NULL,
                SourceFilePath  TEXT    NOT NULL,
                ImportStatus    TEXT    NOT NULL,
                GrossAmount     REAL,
                DeductionAmount REAL,
                NetAmount       REAL,
                BasicAmount     REAL,
                OvertimeHours   REAL,
                ParseMessage    TEXT,
                ImportedAt      TEXT    NOT NULL
            )
            """;

        command.ExecuteNonQuery();
    }
}
