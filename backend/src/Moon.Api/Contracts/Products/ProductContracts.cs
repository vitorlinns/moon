namespace Moon.Api.Contracts.Products;

public record ProductResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid CategoryId,
    string CategoryName,
    string CategorySlug,
    decimal Price,
    DateOnly LaunchedAt,
    int SalesCount,
    bool Featured,
    string? ImageUrl);

public record ProductListResponse(
    IReadOnlyList<ProductResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
