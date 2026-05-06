using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Personal_Finance_Management.Repository;
using Personal_Finance_Management.Repository.Entity;
using Personal_Finance_Management.Service.Validations;

namespace Personal_Finance_Management.Service.category
{
    public class Service : IService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public Service(AppDbContext appDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _appDbContext = appDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<Response.GetCategoriesResponse> GetCategories()
        {
            var userIdGuid = GetCurrentUserId();

            var defaultCategories = await _appDbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsDefault && c.IsActive && c.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new Response.CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color
                })
                .ToListAsync();

            var customCategories = await _appDbContext.Categories
                .AsNoTracking()
                .Where(c => !c.IsDefault
                            && c.OwnerUserId == userIdGuid
                            && c.IsActive
                            && c.DeletedAt == null)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new Response.CategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color
                })
                .ToListAsync();

            return new Response.GetCategoriesResponse
            {
                DefaultCategories = defaultCategories,
                CustomCategories = customCategories
            };
        }

        public async Task<Response.CategoryResponse> CreateCustomCategory(
            Request.CreateCategoryRequest request)
        {
            if (request is null)
            {
                throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
            }

            var userIdGuid = GetCurrentUserId();
            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = NormalizeRequiredName(request.Name),
                Icon = NormalizeOptionalText(request.Icon),
                Color = NormalizeOptionalText(request.Color),
                IsDefault = false,
                OwnerUserId = userIdGuid,
                IsActive = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _appDbContext.Categories.Add(category);
            await _appDbContext.SaveChangesAsync();

            return MapCategory(category);
        }

        public async Task<Response.CategoryResponse> UpdateCustomCategory(
            Guid id,
            Request.UpdateCategoryRequest request)
        {
            if (request is null)
            {
                throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
            }

            var userIdGuid = GetCurrentUserId();
            var category = await GetCustomCategoryOrThrow(id, userIdGuid);

            if (request.Name is not null)
            {
                category.Name = NormalizeRequiredName(request.Name);
            }

            if (request.Icon is not null)
            {
                category.Icon = NormalizeOptionalText(request.Icon);
            }

            if (request.Color is not null)
            {
                category.Color = NormalizeOptionalText(request.Color);
            }

            category.UpdatedAt = DateTimeOffset.UtcNow;
            await _appDbContext.SaveChangesAsync();

            return MapCategory(category);
        }

        public async Task<Response.MessageResponse> DeleteCustomCategory(Guid id)
        {
            var userIdGuid = GetCurrentUserId();
            var category = await GetCustomCategoryOrThrow(id, userIdGuid);
            var now = DateTimeOffset.UtcNow;

            category.IsActive = false;
            category.DeletedAt = now;
            category.UpdatedAt = now;

            await _appDbContext.SaveChangesAsync();

            return new Response.MessageResponse
            {
                Message = "Category deleted"
            };
        }

        public async Task<Response.AdminCategoriesResponse> GetAdminCategories(bool? isActive)
        {
            var query = _appDbContext.Categories
                .AsNoTracking()
                .Where(c => c.IsDefault && c.OwnerUserId == null);

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new Response.AdminCategoryResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Icon = c.Icon,
                    Color = c.Color,
                    Order = c.DisplayOrder,
                    IsActive = c.IsActive
                })
                .ToListAsync();

            return new Response.AdminCategoriesResponse
            {
                Data = categories
            };
        }

        public async Task<Response.AdminCategoryResponse> CreateAdminCategory(
            Request.CreateAdminCategoryRequest request)
        {
            if (request is null)
            {
                throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
            }

            var name = NormalizeRequiredName(request.Name);
            var now = DateTimeOffset.UtcNow;

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Icon = NormalizeOptionalText(request.Icon),
                Color = NormalizeOptionalText(request.Color),
                DisplayOrder = request.Order,
                IsDefault = true,
                OwnerUserId = null,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };

            _appDbContext.Categories.Add(category);
            await _appDbContext.SaveChangesAsync();

            return MapAdminCategory(category);
        }

        public async Task<Response.AdminCategoryResponse> UpdateAdminCategory(
            Guid id,
            Request.UpdateAdminCategoryRequest request)
        {
            if (request is null)
            {
                throw AppValidationException.BadRequest("Request body is required.", "body", "REQUIRED");
            }

            var category = await GetDefaultCategoryOrThrow(id);

            if (request.Name is not null)
            {
                category.Name = NormalizeRequiredName(request.Name);
            }

            if (request.Icon is not null)
            {
                category.Icon = NormalizeOptionalText(request.Icon);
            }

            if (request.Color is not null)
            {
                category.Color = NormalizeOptionalText(request.Color);
            }

            if (request.Order.HasValue)
            {
                category.DisplayOrder = request.Order.Value;
            }

            if (request.IsActive.HasValue)
            {
                category.IsActive = request.IsActive.Value;
                category.DeletedAt = request.IsActive.Value
                    ? null
                    : DateTimeOffset.UtcNow;
            }

            category.UpdatedAt = DateTimeOffset.UtcNow;
            await _appDbContext.SaveChangesAsync();

            return MapAdminCategory(category);
        }

        public async Task<Response.MessageResponse> DeleteAdminCategory(Guid id)
        {
            var category = await GetDefaultCategoryOrThrow(id);
            var now = DateTimeOffset.UtcNow;

            category.IsActive = false;
            category.DeletedAt = now;
            category.UpdatedAt = now;

            await _appDbContext.SaveChangesAsync();

            return new Response.MessageResponse
            {
                Message = "Category deleted"
            };
        }

        // hien: cai nay duoc dung de ho tro cho may cai method api 
        private async Task<Category> GetDefaultCategoryOrThrow(Guid id)
        {
            var category = await _appDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id && c.IsDefault);

            if (category is null)
            {
                throw AppValidationException.NotFound("Category not found.", "id", "CATEGORY_NOT_FOUND");
            }

            return category;
        }

        private async Task<Category> GetCustomCategoryOrThrow(Guid id, Guid userId)
        {
            var category = await _appDbContext.Categories
                .FirstOrDefaultAsync(c => c.Id == id
                                          && !c.IsDefault
                                          && c.OwnerUserId == userId
                                          && c.IsActive
                                          && c.DeletedAt == null);

            if (category is null)
            {
                throw AppValidationException.NotFound("Category not found.", "id", "CATEGORY_NOT_FOUND");
            }

            return category;
        }

        //hien: em hien code cai này de có the lay ra userId tu token, sau này có the refactor lai de dung chung cho cac service khac
        private Guid GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.Claims
                .FirstOrDefault(x => x.Type == "id")?.Value;

            if (!Guid.TryParse(userId, out var userIdGuid))
            {
                throw new UnauthorizedAccessException("UserId not found in token");
            }

            return userIdGuid;
        }

        private static string NormalizeRequiredName(string? name)
        {
            var normalizedName = name?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw AppValidationException.BadRequest("Category name is required.", "name", "REQUIRED");
            }

            return normalizedName;
        }

        private static string? NormalizeOptionalText(string? value)
        {
            var normalizedValue = value?.Trim();
            return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue;
        }

        private static Response.CategoryResponse MapCategory(Category category)
        {
            return new Response.CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                Color = category.Color
            };
        }

        private static Response.AdminCategoryResponse MapAdminCategory(Category category)
        {
            return new Response.AdminCategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Icon = category.Icon,
                Color = category.Color,
                Order = category.DisplayOrder,
                IsActive = category.IsActive
            };
        }
    }
}
