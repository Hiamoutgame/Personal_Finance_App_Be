namespace Personal_Finance_Management.Service.Jar;

public class Response
{
    // GET /api/v1/jars
    public class GetJarsResult
    {
        public required string methodType { get; set; }
        public required decimal totalJarBalance { get; set; }
        public required decimal unallocatedBalance { get; set; }
        public required List<GetJarResponse> data { get; set; }
    }

    public class GetJarResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required decimal balance { get; set; }
        public required string color { get; set; }
        public required string icon { get; set; }
        public required string status { get; set; }
    }
    

    // POST /api/v1/jars
    public class CreateJarResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required decimal balance { get; set; }
        public required string status { get; set; }
    }

    // PATCH /api/v1/jars/{id}
    public class UpdateJarResponse
    {
        public required Guid id { get; set; }
        public required string name { get; set; }
        public required string color { get; set; }
        public required string icon { get; set; }
        public required string status { get; set; }
    }

    // DELETE /api/v1/jars/{id}
    public class DeleteJarResponse
    {
        public required string message { get; set; }
    }
}