namespace Moon.Api.Tests;

/// <summary>Gera CPFs sintéticos com dígito verificador válido, um diferente por chamada.</summary>
internal static class TestCpf
{
    private static readonly Random Random = new();

    public static string Generate()
    {
        var digits = Enumerable.Range(0, 9).Select(_ => Random.Next(0, 10)).ToList();
        digits.Add(CheckDigit(digits, 9));
        digits.Add(CheckDigit(digits, 10));

        return string.Concat(digits);
    }

    private static int CheckDigit(List<int> numbers, int length)
    {
        var sum = 0;
        for (var i = 0; i < length; i++)
        {
            sum += numbers[i] * (length + 1 - i);
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
