using System.Net.Http.Json;
using Moon.Api.Contracts.Products;

namespace Moon.Api.Tests;

/// <summary>
/// GET /api/products é público e só lê dados semeados pela migration (40 produtos, 4 deles
/// com Featured=true) — os testes não inserem nem apagam nada, então não colidem com o resto
/// da suíte compartilhando o banco.
/// </summary>
public class ProductsEndpointsTests(MoonApiFactory factory) : IClassFixture<MoonApiFactory>
{
    [Fact]
    public async Task List_SemFiltros_RetornaPrimeiraPaginaComDezesseisItens()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products");

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(16, result.PageSize);
        Assert.Equal(16, result.Items.Count);
        Assert.Equal(40, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task List_UltimaPagina_RetornaOsOitoItensRestantesSemRepetirIds()
    {
        var client = factory.CreateClient();

        var page1 = await client.GetFromJsonAsync<ProductListResponse>("/api/products?page=1");
        var page2 = await client.GetFromJsonAsync<ProductListResponse>("/api/products?page=2");
        var page3 = await client.GetFromJsonAsync<ProductListResponse>("/api/products?page=3");

        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.NotNull(page3);
        Assert.Equal(8, page3.Items.Count);

        var allIds = page1.Items.Concat(page2.Items).Concat(page3.Items).Select(p => p.Id).ToList();
        Assert.Equal(40, allIds.Distinct().Count());
    }

    [Fact]
    public async Task List_PaginaAlemDoFim_RetornaListaVaziaComStatus200()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?page=99");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(40, result.TotalCount);
    }

    [Fact]
    public async Task List_FiltraPorCategoriaSlug()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?category=aliancas&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(7, result.TotalCount);
        Assert.All(result.Items, p => Assert.Equal("aliancas", p.CategorySlug));
    }

    [Fact]
    public async Task List_CategoriaDesconhecida_RetornaListaVaziaComStatus200()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?category=nao-existe");

        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task List_OrdenaPorPrecoAscendente()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?sort=price-asc&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(result.Items.OrderBy(p => p.Price), result.Items);
    }

    [Fact]
    public async Task List_OrdenaPorPrecoDescendente()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?sort=price-desc&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(result.Items.OrderByDescending(p => p.Price), result.Items);
    }

    [Fact]
    public async Task List_OrdenaPorLancamento()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?sort=launch&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(result.Items.OrderByDescending(p => p.LaunchedAt), result.Items);
    }

    [Fact]
    public async Task List_OrdenaPorVendas()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?sort=sales&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(result.Items.OrderByDescending(p => p.SalesCount), result.Items);
    }

    [Fact]
    public async Task List_Featured_RetornaSomenteOsQuatroProdutosEmDestaque()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?featured=true&pageSize=100");

        Assert.NotNull(result);
        Assert.Equal(4, result.TotalCount);
        Assert.All(result.Items, p => Assert.True(p.Featured));
    }

    [Fact]
    public async Task List_TamanhoDePaginaAcimaDoLimite_EhLimitadoA100()
    {
        var client = factory.CreateClient();

        var result = await client.GetFromJsonAsync<ProductListResponse>("/api/products?pageSize=9999");

        Assert.NotNull(result);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task List_OrdenacaoInvalida_CaiParaOrdemPadrao()
    {
        var client = factory.CreateClient();

        var padrao = await client.GetFromJsonAsync<ProductListResponse>("/api/products");
        var comLixo = await client.GetFromJsonAsync<ProductListResponse>("/api/products?sort=lixo-qualquer");

        Assert.NotNull(padrao);
        Assert.NotNull(comLixo);
        Assert.Equal(padrao.Items.First().Id, comLixo.Items.First().Id);
    }
}
