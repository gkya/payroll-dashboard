using Microsoft.Data.Sqlite;
using PayrollDashboard.Models;

namespace PayrollDashboard.Repositories;

public class SqliteAnnualIncomeRepository : IAnnualIncomeRepository
{
    private readonly string _connectionString;

    public SqliteAnnualIncomeRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Sqlite") ?? "Data Source=payroll.db";
        InitializeDatabase();
    }

    public void Save(AnnualIncomeSlip slip)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO AnnualIncomeSlips (FiscalYear, SourceFileName, SourceFilePath, SourceFileHash, ImportStatus, TotalIncome, AfterDeduction, TotalDeductions, WithholdingTax, ParseMessage, ImportedAt)
            VALUES ($year, $fileName, $filePath, $hash, $status, $totalIncome, $afterDeduction, $totalDeductions, $withholdingTax, $message, $importedAt)
            """;

        command.Parameters.AddWithValue("$year", slip.FiscalYear);
        command.Parameters.AddWithValue("$fileName", slip.SourceFileName);
        command.Parameters.AddWithValue("$filePath", slip.SourceFilePath);
        command.Parameters.AddWithValue("$hash", slip.SourceFileHash);
        command.Parameters.AddWithValue("$status", slip.ImportStatus.ToString());
        command.Parameters.AddWithValue("$totalIncome", slip.TotalIncome.HasValue ? (object)slip.TotalIncome.Value : DBNull.Value);
        command.Parameters.AddWithValue("$afterDeduction", slip.AfterDeduction.HasValue ? (object)slip.AfterDeduction.Value : DBNull.Value);
        command.Parameters.AddWithValue("$totalDeductions", slip.TotalDeductions.HasValue ? (object)slip.TotalDeductions.Value : DBNull.Value);
        command.Parameters.AddWithValue("$withholdingTax", slip.WithholdingTax.HasValue ? (object)slip.WithholdingTax.Value : DBNull.Value);
        command.Parameters.AddWithValue("$message", slip.ParseMessage ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("$importedAt", slip.ImportedAt.ToString("O"));

        command.ExecuteNonQuery();
    }

    public IEnumerable<AnnualIncomeSlip> GetAll()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, FiscalYear, SourceFileName, SourceFilePath, SourceFileHash, ImportStatus, TotalIncome, AfterDeduction, TotalDeductions, WithholdingTax, ParseMessage, ImportedAt
            FROM AnnualIncomeSlips
            ORDER BY FiscalYear ASC
            """;

        var slips = new List<AnnualIncomeSlip>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            slips.Add(new AnnualIncomeSlip
            {
                Id = reader.GetInt32(0),
                FiscalYear = reader.GetString(1),
                SourceFileName = reader.GetString(2),
                SourceFilePath = reader.GetString(3),
                SourceFileHash = reader.GetString(4),
                ImportStatus = Enum.Parse<PayrollImportStatus>(reader.GetString(5)),
                TotalIncome = reader.IsDBNull(6) ? null : reader.GetDecimal(6),
                AfterDeduction = reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                TotalDeductions = reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                WithholdingTax = reader.IsDBNull(9) ? null : reader.GetDecimal(9),
                ParseMessage = reader.IsDBNull(10) ? null : reader.GetString(10),
                ImportedAt = DateTimeOffset.Parse(reader.GetString(11)),
            });
        }

        return slips;
    }

    public bool ExistsByHash(string hash)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(1) FROM AnnualIncomeSlips WHERE SourceFileHash = $hash";
        command.Parameters.AddWithValue("$hash", hash);

        return Convert.ToInt32(command.ExecuteScalar()) > 0;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS AnnualIncomeSlips (
                Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                FiscalYear      TEXT    NOT NULL,
                SourceFileName  TEXT    NOT NULL,
                SourceFilePath  TEXT    NOT NULL,
                SourceFileHash  TEXT    NOT NULL DEFAULT '',
                ImportStatus    TEXT    NOT NULL,
                TotalIncome     REAL,
                AfterDeduction  REAL,
                TotalDeductions REAL,
                WithholdingTax  REAL,
                ParseMessage    TEXT,
                ImportedAt      TEXT    NOT NULL
            )
            """;
        command.ExecuteNonQuery();
    }
}
