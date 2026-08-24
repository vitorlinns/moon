using Moon.Api.Security;

namespace Moon.Api.Tests;

public class EmailValidatorTests
{
    [Theory]
    [InlineData("ana@exemplo.com")]
    [InlineData("ana.beatriz+teste@exemplo.com.br")]
    [InlineData("a@b.co")]
    public void IsValid_AceitaEmailBemFormado(string email)
    {
        Assert.True(EmailValidator.IsValid(email));
    }

    [Theory]
    [InlineData("nao-e-email")]
    [InlineData("sem-arroba.com")]
    [InlineData("sem-dominio@")]
    [InlineData("@sem-usuario.com")]
    [InlineData("com espaco@exemplo.com")]
    [InlineData("")]
    [InlineData(null)]
    public void IsValid_RejeitaEmailMalFormado(string? email)
    {
        Assert.False(EmailValidator.IsValid(email));
    }
}
