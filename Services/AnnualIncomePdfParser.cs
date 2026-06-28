using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PayrollDashboard.Services;

public class AnnualIncomePdfParser
{
    // 源泉徴収票は数字が1文字ずつ抽出されるため、座標列ごとに連結して金額を復元する。
    // 列の X 範囲は書式が国税庁標準で固定されているため定数で定義する。
    private const double Col1Left = 150, Col1Right = 265;  // 支払金額
    private const double Col2Left = 265, Col2Right = 370;  // 給与所得控除後の金額
    private const double Col3Left = 370, Col3Right = 480;  // 所得控除の額の合計額
    private const double Col4Left = 480, Col4Right = 585;  // 源泉徴収税額

    public AnnualIncomeParseResult Parse(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var page = document.GetPage(1);
            var words = page.GetWords().ToList();

            var mainBottom = FindMainRowBottom(words);
            if (!mainBottom.HasValue)
                return new AnnualIncomeParseResult { Success = false, Message = "数値行が見つかりませんでした" };

            var totalIncome     = ExtractColumn(words, mainBottom.Value, Col1Left, Col1Right);
            var afterDeduction  = ExtractColumn(words, mainBottom.Value, Col2Left, Col2Right);
            var totalDeductions = ExtractColumn(words, mainBottom.Value, Col3Left, Col3Right);
            var withholdingTax  = ExtractColumn(words, mainBottom.Value, Col4Left, Col4Right);

            var success = totalIncome.HasValue;
            return new AnnualIncomeParseResult
            {
                Success = success,
                Message = success ? null : "金額の抽出に失敗しました",
                TotalIncome = totalIncome,
                AfterDeduction = afterDeduction,
                TotalDeductions = totalDeductions,
                WithholdingTax = withholdingTax,
            };
        }
        catch (Exception ex)
        {
            return new AnnualIncomeParseResult { Success = false, Message = ex.Message };
        }
    }

    private static double? FindMainRowBottom(IList<Word> words)
    {
        var group = words
            .Where(w => Regex.IsMatch(w.Text, @"^\d+$"))
            .GroupBy(w => Math.Round(w.BoundingBox.Bottom))
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        return group?.Key;
    }

    private static decimal? ExtractColumn(IList<Word> words, double mainBottom, double leftMin, double leftMax)
    {
        var digits = words
            .Where(w => Math.Abs(w.BoundingBox.Bottom - mainBottom) <= 2)
            .Where(w => w.BoundingBox.Left >= leftMin && w.BoundingBox.Left <= leftMax)
            .Where(w => Regex.IsMatch(w.Text, @"^\d+$"))
            .OrderBy(w => w.BoundingBox.Left)
            .ToList();

        if (!digits.Any()) return null;
        var numStr = string.Concat(digits.Select(w => w.Text));
        return decimal.TryParse(numStr, out var value) && value > 0 ? value : null;
    }
}
