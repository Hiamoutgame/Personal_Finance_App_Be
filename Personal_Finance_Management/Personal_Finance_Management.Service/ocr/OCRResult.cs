namespace Personal_Finance_Management.Service.ocr
{
    public class OCRResult
    {
        public bool IsSuccess { get; set; }
        public string? Text { get; set; }
        public string? Layout { get; set; }
        public string Engine { get; set; } = "ocr-service";
        public string? RawJson { get; set; }
        public int? StatusCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
