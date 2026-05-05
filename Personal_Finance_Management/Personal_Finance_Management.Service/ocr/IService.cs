namespace Personal_Finance_Management.Service.ocr
{
    public interface IService
    {
        Task<OCRResult> ReadImageAsync(
            string filePath,
            string? layout = null,
            CancellationToken cancellationToken = default);

        Task<OCRResult> FormatResultAsync(
            string text,
            CancellationToken cancellationToken = default);
    }
}
