namespace Personal_Finance_Management.Service.ocr
{
    public class OCRResult
    {
        public bool IsSuccess { get; set; }
        public string? Text { get; set; }
        public string? Language { get; set; }
        public string Engine { get; set; } = "tesseract";
        public string? ErrorMessage { get; set; }
    }
}
