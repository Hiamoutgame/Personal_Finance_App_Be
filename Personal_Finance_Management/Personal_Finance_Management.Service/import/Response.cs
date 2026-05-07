using Personal_Finance_Management.Service.ocr;

namespace Personal_Finance_Management.Service.import
{
    public class Response
    {
        public class ImportImageResponse
        {
            public string Message { get; set; } = "Imported file successfully";
            public required string FileName { get; set; }
            public required string OriginalFileName { get; set; }
            public required string StoredFilePath { get; set; }
            public string? ContentType { get; set; }
            public long SizeInBytes { get; set; }
            public string? OcrJsonFileName { get; set; }
            public string? StoredOcrJsonPath { get; set; }
            public string? RawOcrJson { get; set; }
            public OCRResult? OcrResult { get; set; }
        }
    }
}
