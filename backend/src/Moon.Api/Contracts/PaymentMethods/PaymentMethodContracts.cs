namespace Moon.Api.Contracts.PaymentMethods;

public record PaymentMethodRequest(
    string Brand,
    string LastFourDigits,
    string HolderName,
    int ExpiryMonth,
    int ExpiryYear,
    bool IsDefault);

public record PaymentMethodResponse(
    Guid Id,
    string Brand,
    string LastFourDigits,
    string HolderName,
    int ExpiryMonth,
    int ExpiryYear,
    bool IsDefault);
