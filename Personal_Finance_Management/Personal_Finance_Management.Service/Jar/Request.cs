namespace Personal_Finance_Management.Service.Jar;

public class Request
{
    // POST /api/v1/jars/setup
    // public class SetupJarsRequest
    // {
    //     public string methodType { get; set; }
    //     public List<SetupCustomJarRequest> customJars { get; set; }
    // }

    // public class SetupCustomJarRequest
    // {
    //     public string name { get; set; }
    //     public string color { get; set; }
    //     public string icon { get; set; }
    // }

    // POST /api/v1/jars
    public class CreateJarRequest
    {
        public string name { get; set; }
        public string color { get; set; }
        public string icon { get; set; }
    }

    // PATCH /api/v1/jars/{id}
    public class UpdateJarRequest
    {
        public string? name { get; set; }
        public string? color { get; set; }
        public string? icon { get; set; }
    }
}