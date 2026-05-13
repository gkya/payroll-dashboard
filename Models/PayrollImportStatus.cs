namespace PayrollDashboard.Models;

public enum PayrollImportStatus
{
    Imported,     // ファイルを受け取った（まだ解析していない）
    Parsed,       // 解析して金額などを取得できた
    ParseFailed,  // 解析に失敗した
}
