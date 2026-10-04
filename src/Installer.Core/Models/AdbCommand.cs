namespace Installer.Core.Models;

public sealed record AdbCommand(IReadOnlyList<string> Arguments, string DisplayName)
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    /// <summary>How long adb may run before it is stopped and reported as timed out.</summary>
    public TimeSpan Timeout { get; init; } = DefaultTimeout;

    public string ArgumentString => string.Join(' ', Arguments.Select(QuoteIfNeeded));

    private static string QuoteIfNeeded(string value) =>
        value.Contains(' ', StringComparison.Ordinal) ? $"\"{value}\"" : value;
}
