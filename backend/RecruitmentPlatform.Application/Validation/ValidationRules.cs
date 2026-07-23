using System.Text.RegularExpressions;

namespace RecruitmentPlatform.Application.Validation;

public static class ValidationRules
{
    public const string PersonNamePattern =
        @"^[\p{L}][\p{L}\p{M}'\- ]{0,49}$";

    public const string PhonePattern =
        @"^[0-9]{10}$";

    public const string CurrencyPattern =
        @"^[A-Za-z]{3}$";

    public const string SkillPattern =
        @"^[\p{L}\p{N}][\p{L}\p{N} .+#/_\-]{0,49}$";

    public static bool IsValidOptionalHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Uri.TryCreate(
            value.Trim(),
            UriKind.Absolute,
            out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp ||
                uri.Scheme == Uri.UriSchemeHttps);
    }

    public static int CountDigits(string value)
    {
        return value.Count(char.IsDigit);
    }

    public static IEnumerable<string> NormalizeSkills(
        IEnumerable<string>? skills)
    {
        return (skills ?? [])
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }

    public static bool IsValidSkill(string skill)
    {
        return Regex.IsMatch(
            skill,
            SkillPattern,
            RegexOptions.CultureInvariant);
    }
}
