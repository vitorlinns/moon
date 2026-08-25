using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moon.Api.Contracts.AdminAuth;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;
using Moon.Api.Domain;
using Moon.Api.Security;

namespace Moon.Api.Tests;

/// <summary>
/// Testes de integração da auth do admin — mesma real API/Postgres de teste que
/// AuthEndpointsTests, mas cobrindo o scheme/cookies/tabela totalmente separados do cliente.
/// Reaproveita AuthTestClient (já é genérico o suficiente: só o path do CSRF é fixo em
/// /api/auth/csrf-token, que é compartilhado de propósito entre loja e admin).
/// </summary>
public class AdminAuthEndpointsTests(MoonApiFactory factory) : IClassFixture<MoonApiFactory>
{
    private const string Password = "senha1234";

    private AuthTestClient NewClient() =>
        new(factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false }));

    /// <summary>Não existe endpoint de registro de admin ainda — insere direto no banco.</summary>
    private async Task<AdminUser> CreateAdminAsync(string? email = null, string password = Password)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var admin = new AdminUser
        {
            Id = Guid.NewGuid(),
            Name = "Admin de Teste",
            Email = email ?? $"admin-{Guid.NewGuid():N}@teste.com",
            PasswordHash = hasher.Hash(password),
        };

        db.AdminUsers.Add(admin);
        await db.SaveChangesAsync();
        return admin;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaAdminEDoisCookiesProprios()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();

        var response = await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AdminUserResponse>();
        Assert.Equal(admin.Id, body!.Id);

        Assert.NotNull(client.GetCookie("moon_admin_access_token"));
        Assert.NotNull(client.GetCookie("moon_admin_refresh_token"));
        Assert.Null(client.GetCookie("moon_access_token"));
    }

    [Fact]
    public async Task Login_ComSenhaErrada_RetornaUnauthorized()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();

        var response = await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = "senhaerrada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ApposCincoTentativasErradas_BloqueiaContaMesmoComSenhaCerta()
    {
        var admin = await CreateAdminAsync();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await NewClient().PostAsync("/api/admin/auth/login", new { email = admin.Email, password = "senhaerrada" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var lockedResponse = await NewClient().PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);
    }

    [Fact]
    public async Task Me_SemCookie_RetornaUnauthorized()
    {
        var response = await NewClient().GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComSessaoDeClienteEmVezDeAdmin_RetornaUnauthorized()
    {
        // prova que os schemes JWT (Bearer x AdminBearer) não se misturam: um cookie de
        // sessão de cliente válido não deve autenticar contra a rota de admin
        var client = NewClient();
        await client.PostAsync("/api/auth/register", new
        {
            cpf = TestCpf.Generate(),
            name = "Cliente",
            email = $"cliente-{Guid.NewGuid():N}@teste.com",
            password = "senha1234",
        });

        var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComSessaoDeAdminValida_RetornaODonoDoCookie()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();
        await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });

        var response = await client.GetAsync("/api/admin/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AdminUserResponse>();
        Assert.Equal(admin.Id, body!.Id);
    }

    [Fact]
    public async Task Refresh_ComTokenValido_RotacionaERevogaOAntigoNoBanco()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();
        await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });
        var oldRefreshToken = client.GetCookie("moon_admin_refresh_token");

        var response = await client.PostAsync("/api/admin/auth/refresh");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var newRefreshToken = client.GetCookie("moon_admin_refresh_token");
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokensForAdmin = await db.AdminRefreshTokens.Where(t => t.AdminUserId == admin.Id).ToListAsync();
        Assert.Equal(2, tokensForAdmin.Count);
        Assert.Single(tokensForAdmin, t => t.RevokedAt != null);
        Assert.Single(tokensForAdmin, t => t.RevokedAt == null);
    }

    [Fact]
    public async Task Refresh_ComTokenJaRotacionado_RevogaTodasAsSessoesDoAdmin()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();
        await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });

        var staleSession = client.Fork();

        var firstRefresh = await client.PostAsync("/api/admin/auth/refresh");
        Assert.Equal(HttpStatusCode.NoContent, firstRefresh.StatusCode);

        var reuseResponse = await staleSession.PostAsync("/api/admin/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);

        var afterReuseAttempt = await client.PostAsync("/api/admin/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, afterReuseAttempt.StatusCode);
    }

    [Fact]
    public async Task Logout_RevogaORefreshTokenELimpaCookies()
    {
        var admin = await CreateAdminAsync();
        var client = NewClient();
        await client.PostAsync("/api/admin/auth/login", new { email = admin.Email, password = Password });

        var response = await client.PostAsync("/api/admin/auth/logout");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var meResponse = await client.GetAsync("/api/admin/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meResponse.StatusCode);
    }
}
