using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace Personal_Finance_Management.Service.ocr
{
    public class Service : IService
    {
        private readonly IConfiguration _configuration;

        public Service(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<OCRResult> ReadImageAsync(
            string imagePath,
            string? language = null,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(imagePath))
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Image file was not found."
                };
            }

            var executablePath = _configuration["Ocr:TesseractExecutablePath"];
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                executablePath = "tesseract";
            }

            var selectedLanguage = string.IsNullOrWhiteSpace(language)
                ? _configuration["Ocr:Language"] ?? "eng"
                : language.Trim();

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            process.StartInfo.ArgumentList.Add(imagePath);
            process.StartInfo.ArgumentList.Add("stdout");
            process.StartInfo.ArgumentList.Add("-l");
            process.StartInfo.ArgumentList.Add(selectedLanguage);

            try
            {
                process.Start();
            }
            catch (Win32Exception)
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    Language = selectedLanguage,
                    ErrorMessage = "Tesseract is not installed or is not available in PATH. Configure Ocr:TesseractExecutablePath to enable OCR."
                };
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            var output = await outputTask;
            var error = await errorTask;

            if (process.ExitCode != 0)
            {
                return new OCRResult
                {
                    IsSuccess = false,
                    Language = selectedLanguage,
                    ErrorMessage = string.IsNullOrWhiteSpace(error)
                        ? "Tesseract OCR failed."
                        : error.Trim()
                };
            }

            return new OCRResult
            {
                IsSuccess = true,
                Language = selectedLanguage,
                Text = output.Trim()
            };
        }
    }
}
