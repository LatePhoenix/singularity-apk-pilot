namespace Installer.Core.Models;

public sealed record ApkIdentity(
    string PackageId,
    string VersionName,
    int VersionCode,
    string? SplitName,
    string? Label,
    string? LauncherActivity,
    string SourcePath)
{
    public bool IsSplit => !string.IsNullOrWhiteSpace(SplitName);

    public bool HasPackageId =>
        !string.IsNullOrWhiteSpace(PackageId) && PackageId.Contains('.', StringComparison.Ordinal);

    public string DisplayLabel =>
        !string.IsNullOrWhiteSpace(Label) ? Label
        : HasPackageId ? PackageId
        : Path.GetFileName(SourcePath);

    public string Summary
    {
        get
        {
            var version = string.IsNullOrWhiteSpace(VersionName) ? "" : $" {VersionName}";
            var split = IsSplit ? $" · split {SplitName}" : "";
            return HasPackageId ? $"{DisplayLabel}{version}{split}" : Path.GetFileName(SourcePath);
        }
    }
}
