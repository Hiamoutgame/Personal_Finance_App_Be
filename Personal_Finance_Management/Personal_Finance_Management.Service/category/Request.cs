namespace Personal_Finance_Management.Service.category
{
    public class Request
    {
        public class CreateCategoryRequest
        {
            public required string Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
        }

        public class UpdateCategoryRequest
        {
            public string? Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
        }

        public class CreateAdminCategoryRequest
        {
            public required string Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
            public int Order { get; set; }
        }

        public class UpdateAdminCategoryRequest
        {
            public string? Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
            public int? Order { get; set; }
            public bool? IsActive { get; set; }
        }
    }
}
