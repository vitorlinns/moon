using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Moon.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    LaunchedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    SalesCount = table.Column<int>(type: "integer", nullable: false),
                    Featured = table.Column<bool>(type: "boolean", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "Name", "Slug" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-0000000000c1"), 1, "Alianças", "aliancas" },
                    { new Guid("00000000-0000-0000-0000-0000000000c2"), 2, "Anéis", "aneis" },
                    { new Guid("00000000-0000-0000-0000-0000000000c3"), 3, "Solitários", "solitarios" },
                    { new Guid("00000000-0000-0000-0000-0000000000c4"), 4, "Colares", "colares" },
                    { new Guid("00000000-0000-0000-0000-0000000000c5"), 5, "Brincos", "brincos" },
                    { new Guid("00000000-0000-0000-0000-0000000000c6"), 6, "Pulseiras", "pulseiras" }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "DisplayOrder", "Featured", "ImageUrl", "IsActive", "LaunchedAt", "Name", "Price", "SalesCount", "Slug" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), new Guid("00000000-0000-0000-0000-0000000000c1"), 1, true, null, true, new DateOnly(2026, 1, 10), "Aliança Clássica Ouro 18k", 1890m, 340, "alianca-classica-ouro-18k" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), new Guid("00000000-0000-0000-0000-0000000000c1"), 2, false, null, true, new DateOnly(2026, 5, 2), "Aliança Trabalhada Ouro Rosé", 2150m, 120, "alianca-trabalhada-ouro-rose" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), new Guid("00000000-0000-0000-0000-0000000000c1"), 3, false, null, true, new DateOnly(2025, 11, 20), "Aliança Anatômica Prata", 690m, 210, "alianca-anatomica-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), new Guid("00000000-0000-0000-0000-0000000000c1"), 4, false, null, true, new DateOnly(2026, 3, 12), "Aliança Diamantada Ouro Branco", 2450m, 88, "alianca-diamantada-ouro-branco" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), new Guid("00000000-0000-0000-0000-0000000000c1"), 5, false, null, true, new DateOnly(2025, 7, 22), "Aliança Lisa Abaulada Prata", 520m, 265, "alianca-lisa-abaulada-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), new Guid("00000000-0000-0000-0000-0000000000c1"), 6, false, null, true, new DateOnly(2026, 7, 10), "Aliança Compromisso Ouro 18k", 2890m, 42, "alianca-compromisso-ouro-18k" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), new Guid("00000000-0000-0000-0000-0000000000c1"), 7, false, null, true, new DateOnly(2026, 2, 1), "Aliança Textura Fosca Ouro Rosé", 2340m, 130, "alianca-textura-fosca-ouro-rose" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), new Guid("00000000-0000-0000-0000-0000000000c2"), 8, true, null, true, new DateOnly(2026, 2, 14), "Anel Solitário Diamante", 4250m, 95, "anel-solitario-diamante" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), new Guid("00000000-0000-0000-0000-0000000000c2"), 9, false, null, true, new DateOnly(2026, 6, 1), "Anel Meia Aliança Zircônias", 990m, 150, "anel-meia-alianca-zirconias" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), new Guid("00000000-0000-0000-0000-0000000000c2"), 10, false, null, true, new DateOnly(2025, 9, 15), "Anel Vintage Ouro Branco", 1680m, 80, "anel-vintage-ouro-branco" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), new Guid("00000000-0000-0000-0000-0000000000c2"), 11, false, null, true, new DateOnly(2026, 4, 5), "Anel Cravejado Zircônias", 850m, 175, "anel-cravejado-zirconias" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), new Guid("00000000-0000-0000-0000-0000000000c2"), 12, false, null, true, new DateOnly(2026, 6, 20), "Anel Empilhável Ouro", 690m, 210, "anel-empilhavel-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), new Guid("00000000-0000-0000-0000-0000000000c2"), 13, false, null, true, new DateOnly(2025, 10, 30), "Anel Signet Ouro Amarelo", 1290m, 60, "anel-signet-ouro-amarelo" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), new Guid("00000000-0000-0000-0000-0000000000c2"), 14, false, null, true, new DateOnly(2026, 1, 15), "Anel Torcido Prata", 480m, 190, "anel-torcido-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), new Guid("00000000-0000-0000-0000-0000000000c3"), 15, false, null, true, new DateOnly(2026, 3, 5), "Solitário Diamante 30pts", 6800m, 40, "solitario-diamante-30pts" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), new Guid("00000000-0000-0000-0000-0000000000c3"), 16, false, null, true, new DateOnly(2025, 12, 1), "Solitário Esmeralda", 5200m, 25, "solitario-esmeralda" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), new Guid("00000000-0000-0000-0000-0000000000c3"), 17, false, null, true, new DateOnly(2026, 5, 8), "Solitário Safira Azul", 5900m, 30, "solitario-safira-azul" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), new Guid("00000000-0000-0000-0000-0000000000c3"), 18, false, null, true, new DateOnly(2025, 9, 25), "Solitário Rubi", 6100m, 22, "solitario-rubi" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), new Guid("00000000-0000-0000-0000-0000000000c3"), 19, false, null, true, new DateOnly(2026, 6, 30), "Solitário Diamante 50pts", 8900m, 15, "solitario-diamante-50pts" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), new Guid("00000000-0000-0000-0000-0000000000c3"), 20, false, null, true, new DateOnly(2026, 3, 20), "Solitário Ouro Rosé Diamante", 4700m, 48, "solitario-ouro-rose-diamante" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), new Guid("00000000-0000-0000-0000-0000000000c4"), 21, true, null, true, new DateOnly(2026, 4, 18), "Colar Gota Prata", 590m, 300, "colar-gota-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), new Guid("00000000-0000-0000-0000-0000000000c4"), 22, false, null, true, new DateOnly(2026, 7, 1), "Colar Ponto de Luz Ouro", 1290m, 60, "colar-ponto-de-luz-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), new Guid("00000000-0000-0000-0000-0000000000c4"), 23, false, null, true, new DateOnly(2025, 10, 10), "Colar Choker Prata", 450m, 175, "colar-choker-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), new Guid("00000000-0000-0000-0000-0000000000c4"), 24, false, null, true, new DateOnly(2026, 2, 28), "Colar Corrente Veneziana Ouro", 1650m, 95, "colar-corrente-veneziana-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), new Guid("00000000-0000-0000-0000-0000000000c4"), 25, false, null, true, new DateOnly(2026, 5, 15), "Colar Coração Zircônia", 720m, 220, "colar-coracao-zirconia" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), new Guid("00000000-0000-0000-0000-0000000000c4"), 26, false, null, true, new DateOnly(2025, 12, 18), "Colar Corrente Cartier Prata", 890m, 140, "colar-corrente-cartier-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), new Guid("00000000-0000-0000-0000-0000000000c4"), 27, false, null, true, new DateOnly(2025, 8, 5), "Colar Camafeu Vintage", 980m, 55, "colar-camafeu-vintage" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), new Guid("00000000-0000-0000-0000-0000000000c5"), 28, true, null, true, new DateOnly(2026, 1, 25), "Brinco Argola Ouro", 780m, 260, "brinco-argola-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), new Guid("00000000-0000-0000-0000-0000000000c5"), 29, false, null, true, new DateOnly(2026, 5, 20), "Brinco Ponto de Luz Diamante", 2100m, 70, "brinco-ponto-de-luz-diamante" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), new Guid("00000000-0000-0000-0000-0000000000c5"), 30, false, null, true, new DateOnly(2025, 8, 30), "Brinco Pérola Clássico", 620m, 190, "brinco-perola-classico" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), new Guid("00000000-0000-0000-0000-0000000000c5"), 31, false, null, true, new DateOnly(2026, 4, 22), "Brinco Cristal Gota", 560m, 165, "brinco-cristal-gota" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), new Guid("00000000-0000-0000-0000-0000000000c5"), 32, false, null, true, new DateOnly(2026, 6, 5), "Brinco Ear Cuff Prata", 390m, 245, "brinco-ear-cuff-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000033"), new Guid("00000000-0000-0000-0000-0000000000c5"), 33, false, null, true, new DateOnly(2025, 11, 12), "Brinco Botão Ouro", 470m, 200, "brinco-botao-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000034"), new Guid("00000000-0000-0000-0000-0000000000c5"), 34, false, null, true, new DateOnly(2026, 3, 30), "Brinco Comprido Cascata", 1120m, 68, "brinco-comprido-cascata" },
                    { new Guid("00000000-0000-0000-0000-000000000035"), new Guid("00000000-0000-0000-0000-0000000000c6"), 35, false, null, true, new DateOnly(2026, 6, 15), "Pulseira Riviera Zircônias", 1450m, 55, "pulseira-riviera-zirconias" },
                    { new Guid("00000000-0000-0000-0000-000000000036"), new Guid("00000000-0000-0000-0000-0000000000c6"), 36, false, null, true, new DateOnly(2025, 11, 5), "Pulseira Berloques Prata", 540m, 230, "pulseira-berloques-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000037"), new Guid("00000000-0000-0000-0000-0000000000c6"), 37, false, null, true, new DateOnly(2026, 1, 5), "Pulseira Elos Ouro", 1580m, 78, "pulseira-elos-ouro" },
                    { new Guid("00000000-0000-0000-0000-000000000038"), new Guid("00000000-0000-0000-0000-0000000000c6"), 38, false, null, true, new DateOnly(2026, 5, 25), "Pulseira Tênis Zircônias", 2200m, 50, "pulseira-tenis-zirconias" },
                    { new Guid("00000000-0000-0000-0000-000000000039"), new Guid("00000000-0000-0000-0000-0000000000c6"), 39, false, null, true, new DateOnly(2025, 9, 8), "Pulseira Corrente Cadeado Prata", 610m, 185, "pulseira-corrente-cadeado-prata" },
                    { new Guid("00000000-0000-0000-0000-000000000040"), new Guid("00000000-0000-0000-0000-0000000000c6"), 40, false, null, true, new DateOnly(2026, 7, 5), "Pulseira Charm Coração", 490m, 160, "pulseira-charm-coracao" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
