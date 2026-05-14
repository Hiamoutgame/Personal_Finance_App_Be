namespace Personal_Finance_Management.Service.ocr
{
    public class OCRResult
    {
        public bool IsSuccess { get; set; }
        public string? Text { get; set; }
        public string? Layout { get; set; }
        public string Engine { get; set; } = "ocr-service";
        public List<OcrTextBlock> Blocks { get; set; } = [];
        public List<OcrTextLine> Lines { get; set; } = [];
        public ReceiptExtractionResult? Receipt { get; set; }
        public string? RawJson { get; set; }
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class OcrTextBlock
    {
        public required string Text { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal? Confidence { get; set; }
        public int PageNumber { get; set; } = 1;
        public decimal Right => X + Width;
        public decimal Bottom => Y + Height;
        public decimal CenterY => Y + Height / 2;
    }

    public class OcrTextLine
    {
        public int Index { get; set; }
        public required string Text { get; set; }
        public decimal X { get; set; }
        public decimal Y { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public List<OcrTextBlock> Blocks { get; set; } = [];
    }

    public class ReceiptExtractionResult
    {
        public bool IsSuccess { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? TotalRawText { get; set; }
        public OcrTextLine? TotalLine { get; set; }
        public DateTimeOffset? TransactionDate { get; set; }
        public string? TransactionDateRawText { get; set; }
        public string? MerchantName { get; set; }
        public Guid? SuggestedCategoryId { get; set; }
        public string? SuggestedCategoryName { get; set; }
        public string? CategoryMatchedBy { get; set; }
        public List<OcrTextLine> Lines { get; set; } = [];
        public List<string> Warnings { get; set; } = [];
    }
}
