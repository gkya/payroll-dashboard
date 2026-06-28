using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PayrollDashboard.Services;

public class PayrollPdfParser
{
    public PayrollParseResult Parse(string filePath)
    {
        try
        {
            using var document = PdfDocument.Open(filePath);
            var page = document.GetPage(1);
            var words = page.GetWords().ToList();

            var grossAmount = FindAmountBelowLabel(words, "【総支給額】");
            var deductionAmount = FindAmountBelowLabel(words, "【控除合計】");
            var netAmount = FindAmountBelowLabel(words, "【差引支給額】");

            var success = grossAmount.HasValue && deductionAmount.HasValue && netAmount.HasValue;
            return new PayrollParseResult
            {
                Success = success,
                Message = success ? null : "金額の抽出に失敗しました",
                GrossAmount = grossAmount,
                DeductionAmount = deductionAmount,
                NetAmount = netAmount,
            };
        }
        catch (Exception ex)
        {
            return new PayrollParseResult { Success = false, Message = ex.Message };
        }
    }

    private static decimal? FindAmountBelowLabel(IList<Word> words, string label)
    {
        // ラベル文字列を含む Word を探す
        var labelWord = words.FirstOrDefault(w => w.Text.Contains(label));
        if (labelWord == null) return null;

        var labelBottom = labelWord.BoundingBox.Bottom;
        var labelLeft = labelWord.BoundingBox.Left;
        var labelRight = labelWord.BoundingBox.Right;

        // ラベルの真下（Y が小さい）かつ水平方向が重なる数値を探す
        var candidates = words
            .Where(w => w.BoundingBox.Bottom < labelBottom - 2)   // ラベルより下
            .Where(w => w.BoundingBox.Bottom > labelBottom - 40)  // 離れすぎない
            .Where(w => w.BoundingBox.Left < labelRight + 20)     // 水平方向が重なる
            .Where(w => w.BoundingBox.Right > labelLeft - 20)
            .Where(w => IsAmount(w.Text))
            .OrderByDescending(w => w.BoundingBox.Bottom)         // 近い順
            .ToList();

        foreach (var candidate in candidates)
        {
            var numStr = candidate.Text.Replace(",", "");
            if (decimal.TryParse(numStr, out var amount) && amount > 0)
                return amount;
        }

        return null;
    }

    private static bool IsAmount(string text)
    {
        var normalized = text.Replace(",", "");
        return Regex.IsMatch(normalized, @"^\d+$");
    }
}
