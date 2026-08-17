using Installer.Core.Models;

namespace Installer.Core.Services.Install;

public sealed class InstallPlanner
{
    public InstallPlan Create(InstallRequest request)
    {
        var manifest = request.Manifest;
        var policy = request.PolicyOverride ?? manifest.InstallPolicy;
        var flags = new List<string>();
        var uninstallFirst = policy == InstallPolicy.UninstallThenInstall;

        switch (policy)
        {
            case InstallPolicy.ReinstallKeepData:
                flags.Add("-r");
                break;
            case InstallPolicy.ReinstallAllowDowngrade:
                flags.Add("-r");
                flags.Add("-d");
                break;
            case InstallPolicy.InstallTestBuild:
                flags.Add("-r");
                flags.Add("-t");
                break;
            case InstallPolicy.InstallFresh:
            case InstallPolicy.UninstallThenInstall:
                break;
        }

        if (manifest.AllowTestApk && !flags.Contains("-t"))
        {
            flags.Add("-t");
        }

        if (manifest.GrantPermissions && !flags.Contains("-g"))
        {
            flags.Add("-g");
        }

        var set = request.Set;
        var packageId = set?.CanVerify == true ? set.PackageId : manifest.AppId;
        var apkPath = set?.PrimaryPath is { Length: > 0 } primary ? primary : manifest.ApkPath;
        var files = set?.ApkPaths is { Count: > 0 } paths ? paths : string.IsNullOrWhiteSpace(apkPath) ? [] : [apkPath];
        var verify = set?.CanVerify == true || manifest.CanVerifyPackage;

        return new InstallPlan(
            packageId,
            apkPath,
            flags,
            uninstallFirst,
            verify,
            set?.CanVerify == true || manifest.LaunchAfterInstall,
            policy,
            files,
            set?.LauncherActivity);
    }
}
