namespace Installer.Core.Models;

public sealed record InstallSet(
    string PackageId,
    string DisplayName,
    string VersionName,
    IReadOnlyList<string> ApkPaths,
    bool IsSplitSet,
    bool LooksLikeMissingSplits,
    string? LauncherActivity,
    string? SourceBundlePath)
{
    public string PrimaryPath => ApkPaths.Count == 0 ? "" : ApkPaths[0];

    public bool CanVerify =>
        !string.IsNullOrWhiteSpace(PackageId)
        && PackageId.Contains('.', StringComparison.Ordinal)
        && !string.Equals(PackageId, InstallManifest.UserSelectedAppId, StringComparison.OrdinalIgnoreCase);
}
