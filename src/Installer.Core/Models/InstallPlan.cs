namespace Installer.Core.Models;

public sealed record InstallPlan(
    string PackageId,
    string ApkPath,
    IReadOnlyList<string> AdbFlags,
    bool RequiresUninstallFirst,
    bool VerifyAfterInstall,
    bool OfferLaunchAfterInstall,
    InstallPolicy Policy,
    IReadOnlyList<string>? ApkPaths = null,
    string? LauncherActivity = null)
{
    public IReadOnlyList<string> Files => ApkPaths is { Count: > 0 } ? ApkPaths : [ApkPath];

    public bool UsesMultiple => Files.Count > 1;
}
