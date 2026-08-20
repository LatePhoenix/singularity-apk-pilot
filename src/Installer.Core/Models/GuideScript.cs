namespace Installer.Core.Models;

public sealed record GuideScript(
    string Greeting,
    string Now,
    string Then,
    string ButtonHint,
    string Progress,
    IReadOnlyList<string> Checks,
    string Mood = "Calm")
{
    public static GuideScript Empty { get; } = new("", "", "", "", "", []);

    public bool HasThen => !string.IsNullOrWhiteSpace(Then);

    public bool HasChecks => Checks.Count > 0;
}
