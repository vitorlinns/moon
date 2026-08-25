using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moon.Api.Contracts.Addresses;
using Moon.Api.Contracts.Auth;
using Moon.Api.Contracts.PaymentMethods;
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

    [Fact]
    public async Task ChangePassword_ComSenhaAtualCorreta_TrocaERevogaOutrasSessoes()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        // outra sessão do mesmo usuário (ex: outro navegador) — deve ser derrubada pela troca
        var otherSessionRefreshToken = client.GetCookie("moon_refresh_token");
        var otherSession = client.Fork();

        var response = await client.PostAsync("/api/auth/change-password", new
        {
            currentPassword = "senha1234",
            newPassword = "novaSenha5678",
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(otherSessionRefreshToken, client.GetCookie("moon_refresh_token"));

        var otherSessionRefresh = await otherSession.PostAsync("/api/auth/refresh");
        Assert.Equal(HttpStatusCode.Unauthorized, otherSessionRefresh.StatusCode);

        // a sessão atual continua válida com o novo par de tokens já emitido
        var meAfterChange = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meAfterChange.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_ComSenhaAtualErrada_RetornaBadRequestENaoAltera()
    {
        var client = NewClient();
        var email = $"change-pw-errada-{Guid.NewGuid():N}@teste.com";
        await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        var response = await client.PostAsync("/api/auth/change-password", new
        {
            currentPassword = "senhaerrada",
            newPassword = "novaSenha5678",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var loginComSenhaAntiga = await NewClient().PostAsync("/api/auth/login", new { email, password = "senha1234" });
        Assert.Equal(HttpStatusCode.OK, loginComSenhaAntiga.StatusCode);
    }

    [Fact]
    public async Task DeleteMe_ComSenhaCorreta_RemoveContaERevogaSessao()
    {
        var client = NewClient();
        var email = $"delete-me-{Guid.NewGuid():N}@teste.com";
        await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        var response = await client.DeleteAsync("/api/auth/me", new { password = "senha1234" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var meAfterDelete = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, meAfterDelete.StatusCode);

        // e-mail volta a ficar livre pra um novo cadastro
        var registerAgain = await NewClient().PostAsync("/api/auth/register", ValidRegisterPayload(email));
        Assert.Equal(HttpStatusCode.OK, registerAgain.StatusCode);
    }

    [Fact]
    public async Task DeleteMe_ComSenhaErrada_RetornaBadRequestENaoRemove()
    {
        var client = NewClient();
        var email = $"delete-me-senha-errada-{Guid.NewGuid():N}@teste.com";
        await client.PostAsync("/api/auth/register", ValidRegisterPayload(email));

        var response = await client.DeleteAsync("/api/auth/me", new { password = "senhaerrada" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var meAfter = await client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.OK, meAfter.StatusCode);
    }

    private static object ValidAddressPayload(bool isDefault = false) => new
    {
        label = "Casa",
        recipient = "Usuária de Teste",
        cep = "01310-100",
        street = "Avenida Paulista",
        number = "1000",
        complement = "Apto 12",
        neighborhood = "Bela Vista",
        city = "São Paulo",
        state = "SP",
        isDefault,
    };

    [Fact]
    public async Task Addresses_SemAutenticacao_RetornaUnauthorized()
    {
        var response = await NewClient().PostAsync("/api/addresses", ValidAddressPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Addresses_Create_PrimeiroEnderecoNasceComoPadraoMesmoSemPedir()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/addresses", ValidAddressPayload(isDefault: false));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var address = await response.Content.ReadFromJsonAsync<AddressResponse>();
        Assert.True(address!.IsDefault);
        Assert.Equal("01310100", address.Cep);
    }

    [Fact]
    public async Task Addresses_Create_ComCepInvalido_RetornaBadRequest()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/addresses", new
        {
            label = "Casa",
            recipient = "Teste",
            cep = "123",
            street = "Rua Teste",
            number = "1",
            complement = (string?)null,
            neighborhood = "Centro",
            city = "São Paulo",
            state = "SP",
            isDefault = false,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Addresses_Create_ComUfInvalida_RetornaBadRequest()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/addresses", new
        {
            label = "Casa",
            recipient = "Teste",
            cep = "01310-100",
            street = "Rua Teste",
            number = "1",
            complement = (string?)null,
            neighborhood = "Centro",
            city = "São Paulo",
            state = "XX",
            isDefault = false,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Addresses_List_RetornaSomenteEnderecosDoUsuarioAutenticado()
    {
        var ownerClient = NewClient();
        await ownerClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        await ownerClient.PostAsync("/api/addresses", ValidAddressPayload());

        var otherClient = NewClient();
        await otherClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        await otherClient.PostAsync("/api/addresses", ValidAddressPayload());

        var response = await ownerClient.GetAsync("/api/addresses");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var addresses = await response.Content.ReadFromJsonAsync<List<AddressResponse>>();
        Assert.Single(addresses!);
    }

    [Fact]
    public async Task Addresses_Update_DeEnderecoDeOutroUsuario_RetornaNotFound()
    {
        var ownerClient = NewClient();
        await ownerClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        var created = await ownerClient.PostAsync("/api/addresses", ValidAddressPayload());
        var address = await created.Content.ReadFromJsonAsync<AddressResponse>();

        var attackerClient = NewClient();
        await attackerClient.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await attackerClient.PutAsync($"/api/addresses/{address!.Id}", ValidAddressPayload());

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Addresses_Delete_DeEnderecoDeOutroUsuario_RetornaNotFoundENaoRemove()
    {
        var ownerClient = NewClient();
        await ownerClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        var created = await ownerClient.PostAsync("/api/addresses", ValidAddressPayload());
        var address = await created.Content.ReadFromJsonAsync<AddressResponse>();

        var attackerClient = NewClient();
        await attackerClient.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await attackerClient.DeleteAsync($"/api/addresses/{address!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var stillThere = await ownerClient.GetAsync("/api/addresses");
        var addresses = await stillThere.Content.ReadFromJsonAsync<List<AddressResponse>>();
        Assert.Single(addresses!);
    }

    [Fact]
    public async Task Addresses_SetDefault_DesmarcaOAntigoEMarcaOEscolhido()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var first = await client.PostAsync("/api/addresses", ValidAddressPayload());
        var firstAddress = await first.Content.ReadFromJsonAsync<AddressResponse>();
        var second = await client.PostAsync("/api/addresses", ValidAddressPayload());
        var secondAddress = await second.Content.ReadFromJsonAsync<AddressResponse>();

        var response = await client.PostAsync($"/api/addresses/{secondAddress!.Id}/default");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await client.GetAsync("/api/addresses");
        var addresses = await list.Content.ReadFromJsonAsync<List<AddressResponse>>();
        Assert.True(addresses!.Single(a => a.Id == secondAddress.Id).IsDefault);
        Assert.False(addresses!.Single(a => a.Id == firstAddress!.Id).IsDefault);
    }

    [Fact]
    public async Task Addresses_Delete_DoEnderecoPadrao_PromoveOutroComoNovoPadrao()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var first = await client.PostAsync("/api/addresses", ValidAddressPayload());
        var firstAddress = await first.Content.ReadFromJsonAsync<AddressResponse>();
        await client.PostAsync("/api/addresses", ValidAddressPayload());

        var deleteResponse = await client.DeleteAsync($"/api/addresses/{firstAddress!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var list = await client.GetAsync("/api/addresses");
        var addresses = await list.Content.ReadFromJsonAsync<List<AddressResponse>>();
        Assert.Single(addresses!);
        Assert.True(addresses!.Single().IsDefault);
    }

    private static object ValidPaymentMethodPayload(bool isDefault = false) => new
    {
        brand = "Visa",
        lastFourDigits = "4242",
        holderName = "Usuária de Teste",
        expiryMonth = 12,
        expiryYear = DateTime.UtcNow.Year + 2,
        isDefault,
    };

    [Fact]
    public async Task PaymentMethods_SemAutenticacao_RetornaUnauthorized()
    {
        var response = await NewClient().PostAsync("/api/payment-methods", ValidPaymentMethodPayload());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PaymentMethods_Create_PrimeiroCartaoNasceComoPadraoMesmoSemPedir()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/payment-methods", ValidPaymentMethodPayload(isDefault: false));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentMethod = await response.Content.ReadFromJsonAsync<PaymentMethodResponse>();
        Assert.True(paymentMethod!.IsDefault);
        Assert.Equal("4242", paymentMethod.LastFourDigits);
    }

    [Fact]
    public async Task PaymentMethods_Create_ComUltimosDigitosInvalidos_RetornaBadRequest()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/payment-methods", new
        {
            brand = "Visa",
            lastFourDigits = "42",
            holderName = "Teste",
            expiryMonth = 12,
            expiryYear = DateTime.UtcNow.Year + 2,
            isDefault = false,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PaymentMethods_Create_ComCartaoVencido_RetornaBadRequest()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await client.PostAsync("/api/payment-methods", new
        {
            brand = "Visa",
            lastFourDigits = "4242",
            holderName = "Teste",
            expiryMonth = 1,
            expiryYear = DateTime.UtcNow.Year - 1,
            isDefault = false,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PaymentMethods_List_RetornaSomenteCartoesDoUsuarioAutenticado()
    {
        var ownerClient = NewClient();
        await ownerClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        await ownerClient.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());

        var otherClient = NewClient();
        await otherClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        await otherClient.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());

        var response = await ownerClient.GetAsync("/api/payment-methods");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paymentMethods = await response.Content.ReadFromJsonAsync<List<PaymentMethodResponse>>();
        Assert.Single(paymentMethods!);
    }

    [Fact]
    public async Task PaymentMethods_Delete_DeCartaoDeOutroUsuario_RetornaNotFoundENaoRemove()
    {
        var ownerClient = NewClient();
        await ownerClient.PostAsync("/api/auth/register", ValidRegisterPayload());
        var created = await ownerClient.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());
        var paymentMethod = await created.Content.ReadFromJsonAsync<PaymentMethodResponse>();

        var attackerClient = NewClient();
        await attackerClient.PostAsync("/api/auth/register", ValidRegisterPayload());

        var response = await attackerClient.DeleteAsync($"/api/payment-methods/{paymentMethod!.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var stillThere = await ownerClient.GetAsync("/api/payment-methods");
        var paymentMethods = await stillThere.Content.ReadFromJsonAsync<List<PaymentMethodResponse>>();
        Assert.Single(paymentMethods!);
    }

    [Fact]
    public async Task PaymentMethods_SetDefault_DesmarcaOAntigoEMarcaOEscolhido()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var first = await client.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());
        var firstCard = await first.Content.ReadFromJsonAsync<PaymentMethodResponse>();
        var second = await client.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());
        var secondCard = await second.Content.ReadFromJsonAsync<PaymentMethodResponse>();

        var response = await client.PostAsync($"/api/payment-methods/{secondCard!.Id}/default");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await client.GetAsync("/api/payment-methods");
        var paymentMethods = await list.Content.ReadFromJsonAsync<List<PaymentMethodResponse>>();
        Assert.True(paymentMethods!.Single(p => p.Id == secondCard.Id).IsDefault);
        Assert.False(paymentMethods!.Single(p => p.Id == firstCard!.Id).IsDefault);
    }

    [Fact]
    public async Task PaymentMethods_Delete_DoCartaoPadrao_PromoveOutroComoNovoPadrao()
    {
        var client = NewClient();
        await client.PostAsync("/api/auth/register", ValidRegisterPayload());
        var first = await client.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());
        var firstCard = await first.Content.ReadFromJsonAsync<PaymentMethodResponse>();
        await client.PostAsync("/api/payment-methods", ValidPaymentMethodPayload());

        var deleteResponse = await client.DeleteAsync($"/api/payment-methods/{firstCard!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var list = await client.GetAsync("/api/payment-methods");
        var paymentMethods = await list.Content.ReadFromJsonAsync<List<PaymentMethodResponse>>();
        Assert.Single(paymentMethods!);
        Assert.True(paymentMethods!.Single().IsDefault);
    }
}
