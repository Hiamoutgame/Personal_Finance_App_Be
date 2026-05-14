namespace Personal_Finance_Management.Service.ocr;

public interface IReceiptParserService
{
    Task<ReceiptExtractionResult> ExtractReceiptAsync(
        OCRResult ocrResult,
        CancellationToken cancellationToken = default);
}
