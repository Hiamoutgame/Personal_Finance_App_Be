namespace Personal_Finance_Management.Service.category
{
    public class Response
    {
        public class GetCategoriesResponse
        {
            public List<CategoryResponse> DefaultCategories { get; set; } = [];
            public List<CategoryResponse> CustomCategories { get; set; } = [];
        }

        public class CategoryResponse
        {
            public Guid Id { get; set; }
            public required string Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
        }

        public class AdminCategoriesResponse
        {
            public List<AdminCategoryResponse> Data { get; set; } = [];
        }

        public class AdminCategoryResponse
        {
            public Guid Id { get; set; }
            public required string Name { get; set; }
            public string? Icon { get; set; }
            public string? Color { get; set; }
            public int Order { get; set; }
            public bool IsActive { get; set; }
        }

        public class MessageResponse
        {
            public required string Message { get; set; }
        }
    }
}
