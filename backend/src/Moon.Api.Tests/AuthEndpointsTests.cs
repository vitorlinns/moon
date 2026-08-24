using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moon.Api.Contracts.Auth;
using Moon.Api.Data;

namespace Moon.Api.Tests;

/// <summary>
/// Testes de integração que sobem a API real (WebApplicationFactory) contra um Postgres de
/// teste de verdade. Ficam todos numa classe só, sem paralelismo entre métodos (padrão do
/// xUnit dentro da mesma classe), pra não disputar o mesmo banco compartilhado.
/// </summary>
public class AuthEndpointsTests(MoonApiFactory factory) : IClassFixture<MoonApiFactory>
{
    private AuthTestClient NewClient() =>
        new(factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false }));

    private static object ValidRegisterPayload(string? email = null) => new
    {
        cpf = TestCpf.Generate(),
        name = "Usuária de Teste",
        email = email ?? $"user-{Guid.NewGuid():N}@teste.com",
        password = "senha1234",
    };

    [Fact]
    public async Task Register_ComDadosValidos_RetornaUsuarioEDoisCookies()
    {
        var client = NewClient();

        var response = await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.NotEqual(Guid.Empty, user.Id);

        Assert.NotNull(client.GetCookie("moon_access_token"));
        Assert.NotNull(client.GetCookie("moon_refresh_token"));
    }

    [Fact]
    public async Task Register_ComCpfInvalido_RetornaBadRequest()
    {
        var client = NewClient();

        var response = await client.PostAsync("/api/auth/register", new
        {
            cpf = "111.111.111-11",
            name = "Teste",
            email = $"invalido-{Guid.NewGuid():N}@teste.com",
            password = "senha1234",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ComSenhaCurta_RetornaBadRequest()
    {
        var client = NewClient();

        var response = await client.PostAsync("/api/auth/register", new
        {
            cpf = TestCpf.Generate(),
            name = "Teste",
            email = $"senhacurta-{Guid.NewGuid():N}@teste.com",
            password = "123",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_ComEmailJaCadastrado_RetornaConflict()
    {
        var client = NewClient();
        var email = $"duplicado-{Guid.NewGuid():N}@teste.com";

        var first = await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);

        var second = await NewClient().PostAsync("/api/auth/register", ValidRegisterPayload(email));
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Register_SemTokenCsrf_RetornaForbidden()
    {
        var client = NewClient();

        var response = await client.PostWithoutCsrfAsync("/api/auth/register", ValidRegisterPayload());

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComCredenciaisCorretas_RetornaOk()
    {
        var client = NewClient();
        var email = $"login-ok-{Guid.NewGuid():N}@teste.com";
        await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        var response = await NewClient().PostAsync("/api/auth/login", new { email, password = "senha1234" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComSenhaErrada_RetornaUnauthorized()
    {
        var client = NewClient();
        var email = $"login-senha-errada-{Guid.NewGuid():N}@teste.com";
        await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        var response = await NewClient().PostAsync("/api/auth/login", new { email, password = "senhaerrada" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ComEmailInexistente_RetornaUnauthorizedComMesmaMensagemDeSenhaErrada()
    {
        var client = NewClient();

        var response = await client.PostAsync("/api/auth/login", new
        {
            email = $"nao-existe-{Guid.NewGuid():N}@teste.com",
            password = "qualquer123",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("E-mail ou senha inválidos.", error?.Message);
    }

    [Fact]
    public async Task Login_ApposCincoTentativasErradas_BloqueiaContaMesmoComSenhaCerta()
    {
        var registerClient = NewClient();
        var email = $"lockout-{Guid.NewGuid():N}@teste.com";
        await registerClient.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var attemptClient = NewClient();
            var response = await attemptClient.PostAsync("/api/auth/login", new { email, password = "senhaerrada" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var lockedResponse = await NewClient().PostAsync("/api/auth/login", new { email, password = "senha1234" });

        Assert.Equal(HttpStatusCode.Locked, lockedResponse.StatusCode);
    }

    [Fact]
    public async Task Me_SemCookie_RetornaUnauthorized()
    {
        var response = await NewClient().GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_ComSessaoValida_RetornaODonoDoCookie()
    {
        var client = NewClient();
        var email = $"me-{Guid.NewGuid():N}@teste.com";
        var registered = await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));
        var registeredUser = await registered.Content.ReadFromJsonAsync<UserResponse>();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal(registeredUser!.Id, user!.Id);
    }

    [Fact]
    public async Task UpdateMe_AtualizaNomeEEmail()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var newEmail = $"atualizado-{Guid.NewGuid():N}@teste.com";

        var response = await client.PatchAsync("/api/auth/me", new { name = "Nome Novo", email = newEmail });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal("Nome Novo", user!.Name);
        Assert.Equal(newEmail, user.Email);
    }

    [Fact]
    public async Task UpdateMe_ComEmailDeOutroUsuario_RetornaConflict()
    {
        var otherEmail = $"outro-{Guid.NewGuid():N}@teste.com";
        await NewClient().PostAsync("/api/auth/register", ValidRegisterPayload(otherEmail));

        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PatchAsync("/api/auth/me", new { name = "Nome", email = otherEmail });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ComTokenValido_RotacionaERevogaOAntigoNoBanco()
    {
        var client = NewClient();
        var registerResponse = await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var user = await registerResponse.Content.ReadFromJsonAsync<UserResponse>();
        var oldRefreshToken = client.GetCookie("moon_refresh_token");

        var response = await client.PostAsync("/api/auth/refresh");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var newRefreshToken = client.GetCookie("moon_refresh_token");
        Assert.NotEqual(oldRefreshToken, newRefreshToken);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var tokensForUser = await db.RefreshTokens.Where(t => t.UserId == user!.Id).ToListAsync();
        Assert.Equal(2, tokensForUser.Count);
        Assert.Single(tokensForUser, t => t.RevokedAt != null);
        Assert.Single(tokensForUser, t => t.RevokedAt == null);
    }

    [Fact]
    public async Task Refresh_ComTokenJaRotacionado_RevogaTodasAsSessoesDoUsuario()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        // "congela" o estado com o token original antes de rotacionar
        var staleSession = client.Fork();

        var firstRefresh = await client.PostAsync("/api/auth/refresh");
        Assert.Equal(HttpStatusCode.NoContent, firstRefresh.StatusCode);

        // reusa o token antigo (já rotacionado) — simula token roubado
        var reuseResponse = await staleSession.PostAsync("/api/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, reuseResponse.StatusCode);

        // mesmo o token novo (que ainda era legítimo) deve ter sido revogado por precaução
        var retryWithRotatedToken = await client.PostAsync("/api/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, retryWithRotatedToken.StatusCode);
    }

    [Fact]
    public async Task Refresh_SemCookie_RetornaUnauthorized()
    {
        var response = await NewClient().PostAsync("/api/auth/refresh");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_RevogaORefreshTokenNoBancoEImpedeNovoRefresh()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var logoutResponse = await client.PostAsync("/api/auth/logout");
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);

        var refreshAfterLogout = await client.PostAsync("/api/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, refreshAfterLogout.StatusCode);
    }
}
