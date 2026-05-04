namespace Personal_Finance_Management.Service.ocr
{
    public interface IService
    {
        Task<OCRResult> ReadImageAsync(
            string imagePath,
            string? language = null,
            CancellationToken cancellationToken = default);
    }
}
