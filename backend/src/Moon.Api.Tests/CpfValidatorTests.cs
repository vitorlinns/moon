using Moon.Api.Security;

namespace Moon.Api.Tests;

public class CpfValidatorTests
{
    [Theory]
    [InlineData("111.444.777-35")]
    [InlineData("11144477735")]
    public void IsValid_AcceitaCpfComDigitoVerificadorCorreto(string cpf)
    {
        Assert.True(CpfValidator.IsValid(cpf));
    }

    [Theory]
    [InlineData("111.444.777-36")] // dígito verificador errado
    [InlineData("111.111.111-11")] // todos os dígitos iguais (matematicamente "passaria", mas é inválido)
    [InlineData("123.456.789-00")]
    [InlineData("111.444.777")] // curto demais
    [InlineData("")]
    public void IsValid_RejeitaCpfInvalido(string cpf)
    {
        Assert.False(CpfValidator.IsValid(cpf));
    }

    [Fact]
    public void IsValid_AceitaCpfsGeradosSinteticamente()
    {
        for (var i = 0; i < 20; i++)
        {
            var cpf = TestCpf.Generate();
            Assert.True(CpfValidator.IsValid(cpf), $"CPF gerado deveria ser válido: {cpf}");
        }
    }
}
