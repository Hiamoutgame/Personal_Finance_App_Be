using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;

namespace Personal_Finance_Management.Service.ocr;

public class ReceiptParserService : IReceiptParserService
{
    private static readonly Regex MoneyRegex = new(@"[\d,\.]+", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(
        @"\b(?<day>\d{1,2})[\/\-.](?<month>\d{1,2})[\/\-.](?<year>\d{2,4})\b|\b(?<yearIso>\d{4})[\/\-.](?<monthIso>\d{1,2})[\/\-.](?<dayIso>\d{1,2})\b",
        RegexOptions.Compiled);

    private static readonly string[] TotalPriorityKeywords =
    [
        "tong tien phai tra",
        "thanh toan",
        "amount due",
        "grand total",
        "total amount",
        "tong cong",
        "total"
    ];

    private static readonly string[] IgnoredTotalKeywords =
    [
        "khach dua",
        "tien khach",
        "tien thoi",
        "thoi lai",
        "no lai",
        "vat",
        "thue",
        "phi",
        "chiet khau",
        "giam gia",
        "subtotal",
        "tong so"
    ];

    private static readonly string[] MerchantNoiseKeywords =
    [
        "hoa don",
        "phieu tinh tien",
        "phieu thanh toan",
        "invoice",
        "receipt",
        "tel",
        "hotline",
        "ma so thue",
        "mst",
        "dia chi",
        "address",
        "ngay",
        "gio",
        "thu ngan"
    ];

    private static readonly Dictionary<string, string[]> MerchantCategoryAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["an uong"] = ["familymart", "circle k", "ministop", "gs25", "highlands", "phuc long", "the coffee house", "starbucks", "kfc", "lotteria", "jollibee", "palla", "halaldsaigon", "halaidsaigon"],
        ["mua sam"] = ["winmart", "coopmart", "big c", "go!", "lotte mart", "bach hoa xanh", "aeon"],
        ["di chuyen"] = ["grab", "be", "gojek", "xanh sm", "taxi", "mai linh", "vinasun"],
        ["giai tri"] = ["cgv", "lotte cinema", "galaxy cinema", "bhd"]
    };

    private readonly AppDbContext _dbContext;

    public ReceiptParserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ReceiptExtractionResult> ExtractReceiptAsync(
        OCRResult ocrResult,
        CancellationToken cancellationToken = default)
    {
        var lines = ocrResult.Lines.Count > 0
            ? ocrResult.Lines
            : GroupBlocksIntoLines(ocrResult.Blocks);

        var result = new ReceiptExtractionResult
        {
            Lines = lines,
            TotalLine = null
        };

        if (lines.Count == 0)
        {
            result.Warnings.Add("OCR did not return bounding boxes, receipt parsing was skipped.");
            result.IsSuccess = false;
            return result;
        }

        ApplyTotal(result, lines);
        ApplyTransactionDate(result, lines);
        result.MerchantName = ExtractMerchantName(lines);
        await ApplyCategorySuggestion(result, lines, cancellationToken);

        result.IsSuccess = result.TotalAmount.HasValue || !string.IsNullOrWhiteSpace(result.MerchantName);
        return result;
    }

    public static List<OcrTextLine> GroupBlocksIntoLines(IEnumerable<OcrTextBlock> blocks)
    {
        var orderedBlocks = blocks
            .Where(block => !string.IsNullOrWhiteSpace(block.Text))
            .OrderBy(block => block.PageNumber)
            .ThenBy(block => block.CenterY)
            .ThenBy(block => block.X)
            .ToList();

        var lines = new List<List<OcrTextBlock>>();
        foreach (var block in orderedBlocks)
        {
            var tolerance = Math.Max(8m, block.Height * 0.65m);
            var line = lines.FirstOrDefault(existing =>
                existing[0].PageNumber == block.PageNumber
                && Math.Abs(existing.Average(item => item.CenterY) - block.CenterY) <= tolerance);

            if (line is null)
            {
                lines.Add([block]);
            }
            else
            {
                line.Add(block);
            }
        }

        return lines
            .Select((lineBlocks, index) => MapLine(index, lineBlocks))
            .OrderBy(line => line.Blocks.Min(block => block.PageNumber))
            .ThenBy(line => line.Y)
            .ThenBy(line => line.X)
            .Select((line, index) =>
            {
                line.Index = index;
                return line;
            })
            .ToList();
    }

    private static OcrTextLine MapLine(int index, IEnumerable<OcrTextBlock> blocks)
    {
        var sortedBlocks = blocks.OrderBy(block => block.X).ToList();
        var x = sortedBlocks.Min(block => block.X);
        var y = sortedBlocks.Min(block => block.Y);
        var right = sortedBlocks.Max(block => block.Right);
        var bottom = sortedBlocks.Max(block => block.Bottom);

        return new OcrTextLine
        {
            Index = index,
            Text = string.Join(" ", sortedBlocks.Select(block => block.Text.Trim())),
            X = x,
            Y = y,
            Width = right - x,
            Height = bottom - y,
            Blocks = sortedBlocks
        };
    }

    private static void ApplyTotal(ReceiptExtractionResult result, IReadOnlyList<OcrTextLine> lines)
    {
        var bestCandidate = lines
            .Select(line => TryCreateTotalCandidate(line))
            .Where(candidate => candidate is not null)
            .OrderByDescending(candidate => candidate!.Priority)
            .ThenByDescending(candidate => candidate!.Line.Y)
            .FirstOrDefault();

        if (bestCandidate is null)
        {
            result.Warnings.Add("Could not extract total amount from receipt OCR lines.");
            return;
        }

        result.TotalAmount = bestCandidate.Amount;
        result.TotalRawText = bestCandidate.RawText;
        result.TotalLine = bestCandidate.Line;
    }

    private static void ApplyTransactionDate(ReceiptExtractionResult result, IReadOnlyList<OcrTextLine> lines)
    {
        var candidates = lines
            .SelectMany(line => ExtractDateValues(line.Text)
                .Select(date => new
                {
                    Date = date,
                    Line = line,
                    IsDateKeywordLine = ContainsDateKeyword(line.Text)
                }))
            .OrderByDescending(candidate => candidate.IsDateKeywordLine)
            .ThenByDescending(candidate => candidate.Line.Y)
            .ToList();

        var selected = candidates.FirstOrDefault();
        if (selected is null)
        {
            result.Warnings.Add("Could not extract transaction date from receipt OCR lines.");
            return;
        }

        result.TransactionDate = selected.Date.Value;
        result.TransactionDateRawText = selected.Date.RawText;
    }

    private static TotalCandidate? TryCreateTotalCandidate(OcrTextLine line)
    {
        var normalizedLineText = NormalizeText(line.Text);
        if (IgnoredTotalKeywords.Any(normalizedLineText.Contains))
        {
            return null;
        }

        var keywordIndex = Array.FindIndex(TotalPriorityKeywords, normalizedLineText.Contains);
        if (keywordIndex < 0)
        {
            return null;
        }

        var keywordBlocks = line.Blocks
            .Where(block => TotalPriorityKeywords.Any(keyword => NormalizeText(block.Text).Contains(keyword.Split(' ')[0])))
            .ToList();
        var keywordRight = keywordBlocks.Count > 0
            ? keywordBlocks.Max(block => block.Right)
            : line.X;

        var rightSideAmount = line.Blocks
            .Where(block => block.X > keywordRight)
            .SelectMany(block => ExtractMoneyValues(block.Text))
            .OrderByDescending(value => value.Amount)
            .FirstOrDefault();

        var fallbackAmount = ExtractMoneyValues(line.Text)
            .OrderByDescending(value => value.Amount)
            .FirstOrDefault();

        var selected = rightSideAmount is { Amount: > 0 }
            ? rightSideAmount
            : fallbackAmount;
        if (selected is null || selected.Amount <= 0)
        {
            return null;
        }

        return new TotalCandidate(
            line,
            selected.Amount,
            selected.RawText,
            TotalPriorityKeywords.Length - keywordIndex);
    }

    private static IEnumerable<MoneyValue> ExtractMoneyValues(string text)
    {
        foreach (Match match in MoneyRegex.Matches(text))
        {
            var raw = match.Value;
            if (raw.Count(char.IsDigit) < 2)
            {
                continue;
            }

            var normalized = raw.Replace(",", string.Empty).Replace(".", string.Empty);
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            {
                yield return new MoneyValue(amount, raw);
            }
        }
    }

    private static IEnumerable<DateValue> ExtractDateValues(string text)
    {
        foreach (Match match in DateRegex.Matches(text))
        {
            var raw = match.Value;
            var dayText = match.Groups["day"].Success
                ? match.Groups["day"].Value
                : match.Groups["dayIso"].Value;
            var monthText = match.Groups["month"].Success
                ? match.Groups["month"].Value
                : match.Groups["monthIso"].Value;
            var yearText = match.Groups["year"].Success
                ? match.Groups["year"].Value
                : match.Groups["yearIso"].Value;

            if (!int.TryParse(dayText, out var day)
                || !int.TryParse(monthText, out var month)
                || !int.TryParse(yearText, out var year))
            {
                continue;
            }

            if (year < 100)
            {
                year += year >= 70 ? 1900 : 2000;
            }

            DateTimeOffset parsedDate;
            try
            {
                parsedDate = new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero);
            }
            catch (ArgumentOutOfRangeException)
            {
                // Ignore OCR fragments that look like dates but are not valid calendar dates.
                continue;
            }

            yield return new DateValue(parsedDate, raw);
        }
    }

    private static bool ContainsDateKeyword(string text)
    {
        var normalized = NormalizeText(text);
        return normalized.Contains("ngay")
               || normalized.Contains("date")
               || normalized.Contains("in luc")
               || normalized.Contains("time");
    }

    private static string? ExtractMerchantName(IReadOnlyList<OcrTextLine> lines)
    {
        var topBoundary = lines.Max(line => line.Y + line.Height) * 0.35m;
        return lines
            .Where(line => line.Y <= topBoundary)
            .Select(line => line.Text.Trim())
            .Where(text => text.Length >= 3)
            .Where(text => !text.Any(char.IsDigit))
            .Where(text => !MerchantNoiseKeywords.Any(keyword => NormalizeText(text).Contains(keyword)))
            .OrderBy(text => text.Length > 35)
            .ThenBy(text => text.Length)
            .FirstOrDefault();
    }

    private async Task ApplyCategorySuggestion(
        ReceiptExtractionResult result,
        IReadOnlyList<OcrTextLine> lines,
        CancellationToken cancellationToken)
    {
        var searchableText = NormalizeText(string.Join(" ", lines.Take(8).Select(line => line.Text)));
        var categoryNameHint = MerchantCategoryAliases
            .FirstOrDefault(pair => pair.Value.Any(alias => searchableText.Contains(NormalizeText(alias))))
            .Key;

        var activeCategories = await _dbContext.Categories
            .AsNoTracking()
            .Where(category => category.IsActive && category.DeletedAt == null)
            .Select(category => new
            {
                category.Id,
                category.Name
            })
            .ToListAsync(cancellationToken);

        var directCategory = activeCategories
            .FirstOrDefault(category => searchableText.Contains(NormalizeText(category.Name)));

        var hintedCategory = string.IsNullOrWhiteSpace(categoryNameHint)
            ? null
            : activeCategories.FirstOrDefault(category =>
                NormalizeText(category.Name).Contains(categoryNameHint)
                || categoryNameHint.Contains(NormalizeText(category.Name)));

        var selected = directCategory ?? hintedCategory;
        if (selected is null)
        {
            result.Warnings.Add("Could not suggest category from merchant dictionary.");
            return;
        }

        result.SuggestedCategoryId = selected.Id;
        result.SuggestedCategoryName = selected.Name;
        result.CategoryMatchedBy = directCategory is not null ? "category-name" : "merchant-alias";
    }

    private static string NormalizeText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace('đ', 'd');
    }

    private sealed record MoneyValue(decimal Amount, string RawText);

    private sealed record DateValue(DateTimeOffset Value, string RawText);

    private sealed record TotalCandidate(
        OcrTextLine Line,
        decimal Amount,
        string RawText,
        int Priority);
}
