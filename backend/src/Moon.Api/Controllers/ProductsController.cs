using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moon.Api.Contracts.Products;
using Moon.Api.Data;

namespace Moon.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(AppDbContext dbContext) : ControllerBase
{
    private const int DefaultPageSize = 16;
    private const int MaxPageSize = 100;

    [HttpGet]
    public async Task<IActionResult> List(
        string? category,
        string? sort,
        bool? featured,
        int page = 1,
        int pageSize = DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query =
            from p in dbContext.Products
            join c in dbContext.Categories on p.CategoryId equals c.Id
            where p.IsActive
            select new { Product = p, Category = c };

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category.Slug == category);
        }

        if (featured == true)
        {
            query = query.Where(x => x.Product.Featured);
        }

        // desempate por Id em todo ramo pra paginação não reordenar entre requisições
        query = sort switch
        {
            "price-asc" => query.OrderBy(x => x.Product.Price).ThenBy(x => x.Product.Id),
            "price-desc" => query.OrderByDescending(x => x.Product.Price).ThenBy(x => x.Product.Id),
            "launch" => query.OrderByDescending(x => x.Product.LaunchedAt).ThenBy(x => x.Product.Id),
            "sales" => query.OrderByDescending(x => x.Product.SalesCount).ThenBy(x => x.Product.Id),
            _ => query.OrderBy(x => x.Product.DisplayOrder),
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ProductResponse(
                x.Product.Id,
                x.Product.Name,
                x.Product.Slug,
                x.Category.Id,
                x.Category.Name,
                x.Category.Slug,
                x.Product.Price,
                x.Product.LaunchedAt,
                x.Product.SalesCount,
                x.Product.Featured,
                x.Product.ImageUrl))
            .ToListAsync(cancellationToken);

        return Ok(new ProductListResponse(items, page, pageSize, totalCount, totalPages));
    }
}
