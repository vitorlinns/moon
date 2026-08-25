namespace Moon.Api.Security;

public static class BrazilianStates
{
    private static readonly HashSet<string> Codes =
    [
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
        "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
        "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO",
    ];

    public static bool IsValid(string uf) => Codes.Contains(uf.Trim().ToUpperInvariant());
}
