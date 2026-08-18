namespace Installer.Core.Models;

public sealed record InstalledApp(
    string PackageId,
    string? Label = null,
    string? Version = null,
    bool IsRecent = false)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Label) ? PackageId : Label;

    public string Summary
    {
        get
        {
            var version = string.IsNullOrWhiteSpace(Version) ? "" : $" · {Version}";
            return string.IsNullOrWhiteSpace(Label) ? PackageId : $"{PackageId}{version}";
        }
    }
}
