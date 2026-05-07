using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CategoryService = Personal_Finance_Management.Service.category;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/admin/categories")]
    // [Authorize(Roles = "Admin")]
    public class AdminCategoryController : ControllerBase
    {
        private readonly CategoryService.IService _categoryService;

        public AdminCategoryController(CategoryService.IService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] bool? isActive)
        {
            var result = await _categoryService.GetAdminCategories(isActive);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryService.Request.CreateAdminCategoryRequest request)
        {
            var result = await _categoryService.CreateAdminCategory(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateCategory(
            Guid id,
            [FromBody] CategoryService.Request.UpdateAdminCategoryRequest request)
        {
            var result = await _categoryService.UpdateAdminCategory(id, request);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.DeleteAdminCategory(id);
            return Ok(result);
        }
    }
}
