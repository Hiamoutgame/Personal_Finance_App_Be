using Microsoft.AspNetCore.Http;

namespace Personal_Finance_Management.Service.import
{
    public class Request
    {
        public class ImportData
        {
            public required IFormFile File { get; set; }
            public string? Layout { get; set; }
            public bool RunOcr { get; set; } = true;
        }
    }
}
