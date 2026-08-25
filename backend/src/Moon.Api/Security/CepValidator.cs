namespace Moon.Api.Security;

public static class CepValidator
{
    public static bool IsValid(string cep)
    {
        var digits = new string(cep.Where(char.IsDigit).ToArray());

        return digits.Length == 8;
    }
}
