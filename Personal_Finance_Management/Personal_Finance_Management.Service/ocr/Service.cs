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
                Layout = "none"
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
            var baseUrl = _configuration["Ocr:BaseUrl"] ?? "http://127.0.0.1:9380";
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
            return new OCRResult
            {
                IsSuccess = true,
                Layout = selectedLayout,
                Text = text,
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
    }
}
