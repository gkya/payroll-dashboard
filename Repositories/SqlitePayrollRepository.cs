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
            INSERT INTO PayrollSlips (PayrollMonth, SourceFileName, SourceFilePath, ImportStatus, ParseMessage, ImportedAt)
            VALUES ($month, $fileName, $filePath, $status, $message, $importedAt)
            """;

        command.Parameters.AddWithValue("$month", slip.PayrollMonth);
        command.Parameters.AddWithValue("$fileName", slip.SourceFileName);
        command.Parameters.AddWithValue("$filePath", slip.SourceFilePath);
        command.Parameters.AddWithValue("$status", slip.ImportStatus.ToString());
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
            SELECT Id, PayrollMonth, SourceFileName, SourceFilePath, ImportStatus, ParseMessage, ImportedAt
            FROM PayrollSlips
            ORDER BY ImportedAt DESC
            """;

        var slips = new List<PayrollSlip>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            slips.Add(new PayrollSlip
            {
                Id = reader.GetInt32(0),
                PayrollMonth = reader.GetString(1),
                SourceFileName = reader.GetString(2),
                SourceFilePath = reader.GetString(3),
                ImportStatus = Enum.Parse<PayrollImportStatus>(reader.GetString(4)),
                ParseMessage = reader.IsDBNull(5) ? null : reader.GetString(5),
                ImportedAt = DateTimeOffset.Parse(reader.GetString(6)),
            });
        }

        return slips;
    }

    private void InitializeDatabase()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS PayrollSlips (
                Id           INTEGER PRIMARY KEY AUTOINCREMENT,
                PayrollMonth TEXT    NOT NULL,
                SourceFileName TEXT  NOT NULL,
                SourceFilePath TEXT  NOT NULL,
                ImportStatus TEXT    NOT NULL,
                ParseMessage TEXT,
                ImportedAt   TEXT    NOT NULL
            )
            """;

        command.ExecuteNonQuery();
    }
}
