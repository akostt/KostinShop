using System.Text.RegularExpressions;

namespace KostinShop.Services;

public static class ValidationHelper
{
    public static string? NormalizePhone(string raw, out string normalized)
    {
        normalized = string.Empty;
        var digits = Regex.Replace(raw.Trim(), @"\D", "");
        if (digits.Length != 11 || digits[0] != '7')
            return "Телефон должен содержать 11 цифр и начинаться с +7.";
        normalized = $"+7{digits[1..]}";
        return null;
    }

    public static string? ValidatePassword(string password)
    {
        if (password.Length < 6)
            return "Пароль должен содержать минимум 6 символов.";
        if (!Regex.IsMatch(password, @"[A-ZА-ЯЁ]"))
            return "Пароль должен содержать минимум одну заглавную букву.";
        if (!Regex.IsMatch(password, @"\d"))
            return "Пароль должен содержать минимум одну цифру.";
        if (!Regex.IsMatch(password, @"[!@#$%^]"))
            return "Пароль должен содержать минимум один спецсимвол: ! @ # $ % ^";
        return null;
    }
}
