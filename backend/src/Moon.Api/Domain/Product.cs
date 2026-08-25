namespace Moon.Api.Domain;

/// <summary>
/// SalesCount é preenchido só na seed — não existe módulo de pedidos ainda. Alimenta a
/// ordenação "mais vendidos" até que isso possa ser calculado a partir de pedidos reais.
/// </summary>
public class Product
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    public Guid CategoryId { get; set; }

    public required decimal Price { get; set; }

    // data de calendário (dia de "lançamento" no catálogo), sem semântica de fuso —
    // por isso DateOnly, diferente do DateTimeOffset usado em CreatedAt de outras entidades
    public DateOnly LaunchedAt { get; set; }

    public int SalesCount { get; set; }

    public bool Featured { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // garante paginação determinística: sem um ORDER BY explícito o Postgres não garante
    // ordem estável entre requisições, o que duplicaria/pularia itens entre páginas
    public int DisplayOrder { get; set; }
}
