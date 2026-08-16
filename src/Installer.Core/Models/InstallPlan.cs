namespace Installer.Core.Models;

public sealed record InstallPlan(
    string PackageId,
    string ApkPath,
    IReadOnlyList<string> AdbFlags,
    bool RequiresUninstallFirst,
    bool VerifyAfterInstall,
    bool OfferLaunchAfterInstall,
    InstallPolicy Policy);
