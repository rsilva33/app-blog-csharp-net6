using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.ViewModels;
using Blog.ViewModels.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Blog.Controllers;

[ApiController]
public class CategoryController : ControllerBase
{
    [HttpGet("v1/categories")]
    public async Task<IActionResult> GetAsync([FromServices] IMemoryCache cache,[FromServices] BlogDataContext context)
    {
        try
        {
            var categories = cache.GetOrCreate("CategoriesCache", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
                return GetCategories(context);
            });

            return Ok(new ResultViewModel<List<Category>>(categories));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<List<Category>>("05EX05 - Enternal server failure."));
        }
    }

    private List<Category> GetCategories(BlogDataContext context)
    {
        return context.Categories.ToList();
    }

    [HttpGet("v1/categories/{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound(new ResultViewModel<Category>("Content not found."));

            return Ok(new ResultViewModel<Category>(category));
        }
        catch (Exception e)
        {
            return StatusCode(500, new ResultViewModel<Category>("05EX06 - Enternal server failure."));
        }
    }

    [HttpPost("v1/categories")]
    public async Task<IActionResult> PostAsync([FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<Category>(ModelState.GetErrors()));

        try
        {
            var category = new Category
            {
                Id = 0,
                Name = model.Name,
                Slug = model.Slug.ToLower()
            };

            await context.Categories.AddAsync(category);
            await context.SaveChangesAsync();

            return Created($"v1/categories/{category.Id}", category);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "05XE9 - Unable to add category.");
        }
        catch (Exception e)
        {
            return StatusCode(500, "05EX10 - External server failure.");
        }
    }

    [HttpPut("v1/categories/{id:int}")]
    public async Task<IActionResult> PutAsync(int id, [FromBody] EditorCategoryViewModel model, [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound();

            category.Name = model.Name;
            category.Slug = model.Slug;

            context.Categories.Update(category);
            await context.SaveChangesAsync();

            return Ok();
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "05XE8 - Unable to change category.");
        }
        catch (Exception e)
        {
            return StatusCode(500, "05EX11 - External server failure.");
        }
    }

    [HttpDelete("v1/categories/{id:int}")]
    public async Task<IActionResult> DeleteAsync(int id, [FromServices] BlogDataContext context)
    {
        try
        {
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);

            if (category == null)
                return NotFound();

            context.Categories.Remove(category);
            await context.SaveChangesAsync();

            return Ok(category);
        }
        catch (DbUpdateException e)
        {
            return StatusCode(500, "05XE07 - Unable to delete category.");
        }
        catch (Exception e)
        {
            return StatusCode(500, "05EX12 - External server failure.");
        }
    }
}
