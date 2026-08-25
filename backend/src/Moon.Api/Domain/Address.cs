namespace Moon.Api.Domain;

public class Address
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Label { get; set; }

    public required string Recipient { get; set; }

    public required string Cep { get; set; }

    public required string Street { get; set; }

    public required string Number { get; set; }

    public string? Complement { get; set; }

    public required string Neighborhood { get; set; }

    public required string City { get; set; }

    public required string State { get; set; }

    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
