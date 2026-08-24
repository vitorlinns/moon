using System.Text.RegularExpressions;

namespace Moon.Api.Security;

public static partial class EmailValidator
{
    public static bool IsValid(string? email) => !string.IsNullOrWhiteSpace(email) && EmailRegex().IsMatch(email);

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailRegex();
}
