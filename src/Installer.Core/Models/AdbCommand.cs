namespace Installer.Core.Models;

public sealed record AdbCommand(IReadOnlyList<string> Arguments, string DisplayName)
{
    public string ArgumentString => string.Join(' ', Arguments.Select(QuoteIfNeeded));

    private static string QuoteIfNeeded(string value) =>
        value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
}
