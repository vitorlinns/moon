namespace Moon.Api.Contracts.Addresses;

public record AddressRequest(
    string Label,
    string Recipient,
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    bool IsDefault);

public record AddressResponse(
    Guid Id,
    string Label,
    string Recipient,
    string Cep,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    bool IsDefault);
