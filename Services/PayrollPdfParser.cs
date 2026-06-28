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

            var grossAmount    = FindAmountBelowLabel(words, "【総支給額】")
                              ?? FindAmountBelowLabel(words, "【総支給】");
            var deductionAmount = FindAmountBelowLabel(words, "【控除合計】");
            var netAmount      = FindAmountBelowLabel(words, "【差引支給額】");

            var abilityPay = FindAmountBelowLabel(words, "能力給");
            var jobPay     = FindAmountBelowLabel(words, "職務給");
            var basicAmount = (abilityPay.HasValue && jobPay.HasValue)
                ? abilityPay.Value + jobPay.Value
                : (decimal?)null;

            var success = grossAmount.HasValue && deductionAmount.HasValue && netAmount.HasValue;
            return new PayrollParseResult
            {
                Success = success,
                Message = success ? null : "金額の抽出に失敗しました",
                GrossAmount = grossAmount,
                DeductionAmount = deductionAmount,
                NetAmount = netAmount,
                BasicAmount = basicAmount,
            };
        }
        catch (Exception ex)
        {
            return new PayrollParseResult { Success = false, Message = ex.Message };
        }
    }

    private static decimal? FindAmountBelowLabel(IList<Word> words, string label)
    {
        var labelWord = words.FirstOrDefault(w => w.Text.Contains(label));
        if (labelWord == null) return null;

        var labelBottom = labelWord.BoundingBox.Bottom;
        var labelLeft   = labelWord.BoundingBox.Left;
        var labelRight  = labelWord.BoundingBox.Right;

        var candidates = words
            .Where(w => w.BoundingBox.Bottom < labelBottom - 2)      // ラベルより下
            .Where(w => w.BoundingBox.Bottom > labelBottom - 40)     // 離れすぎない
            .Where(w => w.BoundingBox.Left >= labelLeft - 30)        // 左端が近い
            .Where(w => w.BoundingBox.Left <= labelRight + 50)       // 右端が近い
            .Where(w => IsAmount(w.Text))
            .OrderByDescending(w => w.BoundingBox.Bottom)
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
