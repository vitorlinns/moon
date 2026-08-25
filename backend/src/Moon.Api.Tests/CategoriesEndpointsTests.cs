using System.Net.Http.Json;
using Moon.Api.Contracts.Categories;

namespace Moon.Api.Tests;

/// <summary>
/// GET /api/categories é público e só lê dados semeados pela migration — os testes não
/// inserem nem apagam nada, então não colidem com o resto da suíte compartilhando o banco.
/// </summary>
public class CategoriesEndpointsTests(MoonApiFactory factory) : IClassFixture<MoonApiFactory>
{
    [Fact]
    public async Task List_RetornaAsSeisCategoriasNaOrdemDeExibicao()
    {
        var client = factory.CreateClient();

        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Equal(
            ["Alianças", "Anéis", "Solitários", "Colares", "Brincos", "Pulseiras"],
            categories.Select(c => c.Name));
    }

    [Fact]
    public async Task List_CadaCategoriaTemSlugUnicoENaoVazio()
    {
        var client = factory.CreateClient();

        var categories = await client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories");

        Assert.NotNull(categories);
        Assert.All(categories, c => Assert.False(string.IsNullOrWhiteSpace(c.Slug)));
        Assert.Equal(categories.Count, categories.Select(c => c.Slug).Distinct().Count());
    }
}
