using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Personal_Finance_Management.Service.ocr
{
    public class Service : IService
    {
        private static readonly HashSet<string> AllowedLayouts = new(StringComparer.OrdinalIgnoreCase)
        {
            "none",
            "invoice",
            "document"
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public Service(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<OCRResult> FormatResultAsync(string text, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(new OCRResult
            {
                IsSuccess = true,
                Text = text,
                Layout = "none",
                Lines = text
                    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select((line, index) => new OcrTextLine
                    {
                        Index = index,
                        Text = line,
                        X = 0,
                        Y = index,
                        Width = 0,
                        Height = 0
                    })
                    .ToList()
            });
        }

        public async Task<OCRResult> ReadImageAsync(
            string filePath,
            string? layout = null,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(filePath))
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    ErrorMessage = "File was not found."
                };
            }

            var selectedLayout = NormalizeLayout(layout);
            var baseUrl = _configuration["Ocr:BaseUrl"] ?? throw new InvalidOperationException("OCR service base URL is not configured.");
            var requestUri = $"{baseUrl.TrimEnd('/')}/ocr?layout={Uri.EscapeDataString(selectedLayout)}";

            await using var fileStream = File.OpenRead(filePath);
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetContentType(filePath));
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            HttpResponseMessage response;
            string responseText;
            try
            {
                response = await _httpClient.PostAsync(requestUri, form, cancellationToken);
                responseText = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    Layout = selectedLayout,
                    ErrorMessage = "OCR service request timed out."
                };
            }
            catch (HttpRequestException ex)
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    Layout = selectedLayout,
                    ErrorMessage = $"Cannot connect to OCR service: {ex.Message}"
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    Layout = selectedLayout,
                    RawJson = responseText,
                    StatusCode = (int)response.StatusCode,
                    ErrorMessage = string.IsNullOrWhiteSpace(responseText)
                        ? "OCR service returned an error."
                        : responseText.Trim()
                };
            }

            var text = ExtractText(responseText);
            var blocks = ExtractBlocks(responseText);
            var lines = ReceiptParserService.GroupBlocksIntoLines(blocks);
            return new OCRResult
            {
                IsSuccess = true,
                Layout = selectedLayout,
                Text = text,
                Blocks = blocks,
                Lines = lines,
                RawJson = responseText,
                StatusCode = (int)response.StatusCode
            };
        }

        private string NormalizeLayout(string? layout)
        {
            var selectedLayout = string.IsNullOrWhiteSpace(layout)
                ? _configuration["Ocr:Layout"] ?? "none"
                : layout.Trim();

            return AllowedLayouts.Contains(selectedLayout)
                ? selectedLayout.ToLowerInvariant()
                : "none";
        }

        private static string GetContentType(string filePath)
        {
            return Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }

        private static string? ExtractText(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(rawJson);
                return document.RootElement.TryGetProperty("text", out var textElement)
                    ? textElement.GetString()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static List<OcrTextBlock> ExtractBlocks(string rawJson)
        {
            var blocks = new List<OcrTextBlock>();
            if (string.IsNullOrWhiteSpace(rawJson))
            {
                return blocks;
            }

            try
            {
                using var document = JsonDocument.Parse(rawJson);
                ExtractBlocks(document.RootElement, blocks, new HashSet<string>(), 1);
            }
            catch (JsonException)
            {
                return blocks;
            }

            return blocks
                .OrderBy(block => block.PageNumber)
                .ThenBy(block => block.Y)
                .ThenBy(block => block.X)
                .ToList();
        }

        private static void ExtractBlocks(
            JsonElement element,
            ICollection<OcrTextBlock> blocks,
            ISet<string> seen,
            int pageNumber)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("page", out var pageElement)
                    && pageElement.ValueKind == JsonValueKind.Number
                    && pageElement.TryGetInt32(out var parsedPage))
                {
                    pageNumber = parsedPage;
                }

                if (element.TryGetProperty("boxes", out var boxesElement)
                    && boxesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var boxElement in boxesElement.EnumerateArray())
                    {
                        if (TryParseBlock(boxElement, pageNumber, out var block))
                        {
                            var key = $"{block.PageNumber}:{block.Text}:{block.X}:{block.Y}:{block.Width}:{block.Height}";
                            if (seen.Add(key))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.NameEquals("boxes"))
                    {
                        continue;
                    }

                    ExtractBlocks(property.Value, blocks, seen, pageNumber);
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    ExtractBlocks(item, blocks, seen, pageNumber);
                }
            }
        }

        private static bool TryParseBlock(JsonElement element, int pageNumber, out OcrTextBlock block)
        {
            block = null!;
            if (element.ValueKind != JsonValueKind.Object
                || !element.TryGetProperty("text", out var textElement)
                || textElement.ValueKind != JsonValueKind.String
                || !element.TryGetProperty("bbox", out var bboxElement)
                || bboxElement.ValueKind != JsonValueKind.Array)
            {
                return false;
            }

            var bbox = bboxElement.EnumerateArray().ToList();
            if (bbox.Count < 4)
            {
                return false;
            }

            var text = textElement.GetString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var x1 = bbox[0].GetDecimal();
            var y1 = bbox[1].GetDecimal();
            var x2 = bbox[2].GetDecimal();
            var y2 = bbox[3].GetDecimal();

            decimal? confidence = null;
            if (element.TryGetProperty("score", out var scoreElement)
                && scoreElement.ValueKind == JsonValueKind.Number)
            {
                confidence = scoreElement.GetDecimal();
            }

            block = new OcrTextBlock
            {
                Text = text,
                X = x1,
                Y = y1,
                Width = Math.Max(0, x2 - x1),
                Height = Math.Max(0, y2 - y1),
                Confidence = confidence,
                PageNumber = pageNumber
            };

            return true;
        }
    }
}
