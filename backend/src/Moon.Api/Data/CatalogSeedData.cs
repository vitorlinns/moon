using Moon.Api.Domain;

namespace Moon.Api.Data;

// IDs e slugs fixos usados pelo HasData (seed embutido na migration) — não é código de
// runtime, só a fonte dos dados que o EF grava como InsertData na migration gerada.
internal static class CatalogSeedData
{
    private static readonly Guid AliancasId = Guid.Parse("00000000-0000-0000-0000-0000000000c1");
    private static readonly Guid AneisId = Guid.Parse("00000000-0000-0000-0000-0000000000c2");
    private static readonly Guid SolitariosId = Guid.Parse("00000000-0000-0000-0000-0000000000c3");
    private static readonly Guid ColaresId = Guid.Parse("00000000-0000-0000-0000-0000000000c4");
    private static readonly Guid BrincosId = Guid.Parse("00000000-0000-0000-0000-0000000000c5");
    private static readonly Guid PulseirasId = Guid.Parse("00000000-0000-0000-0000-0000000000c6");

    public static readonly Category[] Categories =
    [
        new() { Id = AliancasId, Name = "Alianças", Slug = "aliancas", DisplayOrder = 1 },
        new() { Id = AneisId, Name = "Anéis", Slug = "aneis", DisplayOrder = 2 },
        new() { Id = SolitariosId, Name = "Solitários", Slug = "solitarios", DisplayOrder = 3 },
        new() { Id = ColaresId, Name = "Colares", Slug = "colares", DisplayOrder = 4 },
        new() { Id = BrincosId, Name = "Brincos", Slug = "brincos", DisplayOrder = 5 },
        new() { Id = PulseirasId, Name = "Pulseiras", Slug = "pulseiras", DisplayOrder = 6 },
    ];

    public static readonly Product[] Products =
    [
        Product(1, "Aliança Clássica Ouro 18k", "alianca-classica-ouro-18k", AliancasId, 1890m, "2026-01-10", 340, featured: true),
        Product(2, "Aliança Trabalhada Ouro Rosé", "alianca-trabalhada-ouro-rose", AliancasId, 2150m, "2026-05-02", 120),
        Product(3, "Aliança Anatômica Prata", "alianca-anatomica-prata", AliancasId, 690m, "2025-11-20", 210),
        Product(4, "Aliança Diamantada Ouro Branco", "alianca-diamantada-ouro-branco", AliancasId, 2450m, "2026-03-12", 88),
        Product(5, "Aliança Lisa Abaulada Prata", "alianca-lisa-abaulada-prata", AliancasId, 520m, "2025-07-22", 265),
        Product(6, "Aliança Compromisso Ouro 18k", "alianca-compromisso-ouro-18k", AliancasId, 2890m, "2026-07-10", 42),
        Product(7, "Aliança Textura Fosca Ouro Rosé", "alianca-textura-fosca-ouro-rose", AliancasId, 2340m, "2026-02-01", 130),

        Product(8, "Anel Solitário Diamante", "anel-solitario-diamante", AneisId, 4250m, "2026-02-14", 95, featured: true),
        Product(9, "Anel Meia Aliança Zircônias", "anel-meia-alianca-zirconias", AneisId, 990m, "2026-06-01", 150),
        Product(10, "Anel Vintage Ouro Branco", "anel-vintage-ouro-branco", AneisId, 1680m, "2025-09-15", 80),
        Product(11, "Anel Cravejado Zircônias", "anel-cravejado-zirconias", AneisId, 850m, "2026-04-05", 175),
        Product(12, "Anel Empilhável Ouro", "anel-empilhavel-ouro", AneisId, 690m, "2026-06-20", 210),
        Product(13, "Anel Signet Ouro Amarelo", "anel-signet-ouro-amarelo", AneisId, 1290m, "2025-10-30", 60),
        Product(14, "Anel Torcido Prata", "anel-torcido-prata", AneisId, 480m, "2026-01-15", 190),

        Product(15, "Solitário Diamante 30pts", "solitario-diamante-30pts", SolitariosId, 6800m, "2026-03-05", 40),
        Product(16, "Solitário Esmeralda", "solitario-esmeralda", SolitariosId, 5200m, "2025-12-01", 25),
        Product(17, "Solitário Safira Azul", "solitario-safira-azul", SolitariosId, 5900m, "2026-05-08", 30),
        Product(18, "Solitário Rubi", "solitario-rubi", SolitariosId, 6100m, "2025-09-25", 22),
        Product(19, "Solitário Diamante 50pts", "solitario-diamante-50pts", SolitariosId, 8900m, "2026-06-30", 15),
        Product(20, "Solitário Ouro Rosé Diamante", "solitario-ouro-rose-diamante", SolitariosId, 4700m, "2026-03-20", 48),

        Product(21, "Colar Gota Prata", "colar-gota-prata", ColaresId, 590m, "2026-04-18", 300, featured: true),
        Product(22, "Colar Ponto de Luz Ouro", "colar-ponto-de-luz-ouro", ColaresId, 1290m, "2026-07-01", 60),
        Product(23, "Colar Choker Prata", "colar-choker-prata", ColaresId, 450m, "2025-10-10", 175),
        Product(24, "Colar Corrente Veneziana Ouro", "colar-corrente-veneziana-ouro", ColaresId, 1650m, "2026-02-28", 95),
        Product(25, "Colar Coração Zircônia", "colar-coracao-zirconia", ColaresId, 720m, "2026-05-15", 220),
        Product(26, "Colar Corrente Cartier Prata", "colar-corrente-cartier-prata", ColaresId, 890m, "2025-12-18", 140),
        Product(27, "Colar Camafeu Vintage", "colar-camafeu-vintage", ColaresId, 980m, "2025-08-05", 55),

        Product(28, "Brinco Argola Ouro", "brinco-argola-ouro", BrincosId, 780m, "2026-01-25", 260, featured: true),
        Product(29, "Brinco Ponto de Luz Diamante", "brinco-ponto-de-luz-diamante", BrincosId, 2100m, "2026-05-20", 70),
        Product(30, "Brinco Pérola Clássico", "brinco-perola-classico", BrincosId, 620m, "2025-08-30", 190),
        Product(31, "Brinco Cristal Gota", "brinco-cristal-gota", BrincosId, 560m, "2026-04-22", 165),
        Product(32, "Brinco Ear Cuff Prata", "brinco-ear-cuff-prata", BrincosId, 390m, "2026-06-05", 245),
        Product(33, "Brinco Botão Ouro", "brinco-botao-ouro", BrincosId, 470m, "2025-11-12", 200),
        Product(34, "Brinco Comprido Cascata", "brinco-comprido-cascata", BrincosId, 1120m, "2026-03-30", 68),

        Product(35, "Pulseira Riviera Zircônias", "pulseira-riviera-zirconias", PulseirasId, 1450m, "2026-06-15", 55),
        Product(36, "Pulseira Berloques Prata", "pulseira-berloques-prata", PulseirasId, 540m, "2025-11-05", 230),
        Product(37, "Pulseira Elos Ouro", "pulseira-elos-ouro", PulseirasId, 1580m, "2026-01-05", 78),
        Product(38, "Pulseira Tênis Zircônias", "pulseira-tenis-zirconias", PulseirasId, 2200m, "2026-05-25", 50),
        Product(39, "Pulseira Corrente Cadeado Prata", "pulseira-corrente-cadeado-prata", PulseirasId, 610m, "2025-09-08", 185),
        Product(40, "Pulseira Charm Coração", "pulseira-charm-coracao", PulseirasId, 490m, "2026-07-05", 160),
    ];

    private static Product Product(
        int displayOrder, string name, string slug, Guid categoryId, decimal price,
        string launchedAt, int salesCount, bool featured = false) => new()
    {
        Id = Guid.Parse($"00000000-0000-0000-0000-{displayOrder:D12}"),
        Name = name,
        Slug = slug,
        CategoryId = categoryId,
        Price = price,
        LaunchedAt = DateOnly.Parse(launchedAt),
        SalesCount = salesCount,
        Featured = featured,
        ImageUrl = null,
        IsActive = true,
        DisplayOrder = displayOrder,
    };
}
