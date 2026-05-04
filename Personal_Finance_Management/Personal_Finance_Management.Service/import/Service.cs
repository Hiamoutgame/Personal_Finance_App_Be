using Personal_Finance_Management.Repository;
using ValidationServices = Personal_Finance_Management.Service.Validations;
using OcrService = Personal_Finance_Management.Service.ocr;

namespace Personal_Finance_Management.Service.import
{
    public class Service : IServices
    {
        private readonly ValidationServices.IServices _validationService;
        private readonly OcrService.IService _ocrService;
        private readonly AppDbContext _dbContext;

        public Service(
            AppDbContext dbContext,
            ValidationServices.IServices validationService,
            OcrService.IService ocrService)
        {
            _dbContext = dbContext;
            _validationService = validationService;
            _ocrService = ocrService;
        }

        public async Task<Response.ImportImageResponse> ImportImage(Request.ImportData request)
        {
            await _validationService.ValidateImportImageRequest(request);

            var uploadFolder = GetUploadFolderPath();
            Directory.CreateDirectory(uploadFolder);

            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            var fileName = $"{DateTimeOffset.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var savePath = Path.Combine(uploadFolder, fileName);

            await using (var fileStream = new FileStream(savePath, FileMode.CreateNew, FileAccess.Write))
            {
                await request.File.CopyToAsync(fileStream);
            }

            OcrService.OCRResult? ocrResult = null;
            if (request.RunOcr)
            {
                ocrResult = await _ocrService.ReadImageAsync(savePath, request.OcrLanguage);
            }

            return new Response.ImportImageResponse
            {
                FileName = fileName,
                OriginalFileName = Path.GetFileName(request.File.FileName),
                StoredFilePath = savePath,
                ContentType = request.File.ContentType,
                SizeInBytes = request.File.Length,
                OcrResult = ocrResult
            };
        }

        private static string GetUploadFolderPath()
        {
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (currentDirectory is not null)
            {
                var directServiceProject = Path.Combine(
                    currentDirectory.FullName,
                    "Personal_Finance_Management.Service");
                if (Directory.Exists(directServiceProject))
                {
                    return Path.Combine(directServiceProject, "import", "Upload");
                }

                var nestedServiceProject = Path.Combine(
                    currentDirectory.FullName,
                    "Personal_Finance_Management",
                    "Personal_Finance_Management.Service");
                if (Directory.Exists(nestedServiceProject))
                {
                    return Path.Combine(nestedServiceProject, "import", "Upload");
                }

                currentDirectory = currentDirectory.Parent;
            }

            return Path.Combine(
                Directory.GetCurrentDirectory(),
                "Personal_Finance_Management",
                "Personal_Finance_Management.Service",
                "import",
                "Upload");
        }
    }
}
