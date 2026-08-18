using System.Text.RegularExpressions;

namespace Installer.Core.Utilities;

public static class EmailAddress
{
    private static readonly Regex Simple = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool TryNormalize(string? value, out string email)
    {
        email = (value ?? "").Trim();
        if (email.Length is < 6 or > 254)
        {
            return false;
        }

        return Simple.IsMatch(email);
    }

    public static bool IsPlaceholder(string? email) =>
        !string.IsNullOrWhiteSpace(email)
        && email.Contains("example.com", StringComparison.OrdinalIgnoreCase);

    public static string Default(string? lastSaved, string? manifestEmail, string? publisherEmail)
    {
        if (TryNormalize(lastSaved, out var last) && !IsPlaceholder(last))
        {
            return last;
        }

        if (TryNormalize(manifestEmail, out var manifest) && !IsPlaceholder(manifest))
        {
            return manifest;
        }

        if (TryNormalize(publisherEmail, out var publisher))
        {
            return publisher;
        }

        return "";
    }
}
