using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon.Api.Contracts.Categories;
using Moon.Api.Data;
using Moon.Api.Domain;

namespace Moon.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        return Ok(categories.Select(ToResponse));
    }

    private static CategoryResponse ToResponse(Category category) => new(category.Id, category.Name, category.Slug);
}
