namespace Moon.Api.Security;

public static class CpfValidator
{
    public static bool IsValid(string cpf)
    {
        var digits = new string(cpf.Where(char.IsDigit).ToArray());

        if (digits.Length != 11 || digits.Distinct().Count() == 1)
        {
            return false;
        }

        var numbers = digits.Select(c => c - '0').ToArray();

        var firstCheckDigit = CalculateCheckDigit(numbers, 9);
        var secondCheckDigit = CalculateCheckDigit(numbers, 10);

        return numbers[9] == firstCheckDigit && numbers[10] == secondCheckDigit;
    }

    private static int CalculateCheckDigit(int[] numbers, int length)
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
