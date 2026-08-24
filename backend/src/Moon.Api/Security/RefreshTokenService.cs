using System.Security.Cryptography;
using System.Text;

namespace Moon.Api.Security;

public interface IRefreshTokenService
{
    /// <summary>Gera o token cru (alta entropia) entregue ao cliente via cookie.</summary>
    string GenerateRawToken();

    /// <summary>
    /// Hash determinístico (SHA-256) usado pra guardar/consultar o token no banco.
    /// Não é bcrypt de propósito: aqui precisamos de lookup por igualdade, e o token
    /// já tem entropia suficiente pra não precisar de um hash lento e salgado.
    /// </summary>
    string Hash(string rawToken);
}

public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRawToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    public string Hash(string rawToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
}
