namespace Moon.Api.Security;

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string hash);

    /// <summary>
    /// Hash inválido usado para manter o tempo de resposta do login constante
    /// quando o usuário não existe, evitando enumeração de e-mails por timing.
    /// </summary>
    string DummyHash { get; }
}

public class BCryptPasswordHasher : IPasswordHasher
{
    public string DummyHash { get; } = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString());

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}
