namespace Installer.Core.Models;

public sealed record RecentsState(string? LastFolder, IReadOnlyList<string> LastFiles)
{
    public static RecentsState Empty { get; } = new(null, []);
}
