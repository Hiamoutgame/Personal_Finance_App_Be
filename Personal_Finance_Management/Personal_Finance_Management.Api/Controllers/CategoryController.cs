using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Personal_Finance_Management.Api.Extensions;
using CategoryService = Personal_Finance_Management.Service.category;

namespace Personal_Finance_Management.Api.Controllers
{
    [ApiController]
    [Route("api/v1/categories")]
    [Authorize(Policy = AuthorizationExtension.Policies.User)]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService.IService _categoryService;

        public CategoryController(CategoryService.IService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var result = await _categoryService.GetCategories();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryService.Request.CreateCategoryRequest request)
        {
            var result = await _categoryService.CreateCustomCategory(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> UpdateCategory(
            Guid id,
            [FromBody] CategoryService.Request.UpdateCategoryRequest request)
        {
            var result = await _categoryService.UpdateCustomCategory(id, request);
            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _categoryService.DeleteCustomCategory(id);
            return Ok(result);
        }
    }
}
