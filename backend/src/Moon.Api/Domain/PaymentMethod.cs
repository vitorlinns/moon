namespace Moon.Api.Domain;

/// <summary>
/// Guarda só metadados do cartão (bandeira, últimos 4 dígitos, validade, titular) — nunca o
/// número completo nem o CVV. Sem gateway de pagamento integrado ainda; quando a pagar.me
/// entrar, isso vira o registro do "cartão salvo" ligado ao token que o gateway devolver.
/// </summary>
public class PaymentMethod
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Brand { get; set; }

    public required string LastFourDigits { get; set; }

    public required string HolderName { get; set; }

    public int ExpiryMonth { get; set; }

    public int ExpiryYear { get; set; }

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
