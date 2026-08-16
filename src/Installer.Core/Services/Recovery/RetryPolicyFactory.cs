using Installer.Core.Models;

namespace Installer.Core.Services.Recovery;

public sealed class RetryPolicyFactory
{
    public InstallPolicy? NextPolicy(InstallError error, InstallPolicy current) =>
        error switch
        {
            InstallError.VersionDowngrade when current != InstallPolicy.UninstallThenInstall
                => InstallPolicy.ReinstallAllowDowngrade,
            InstallError.PackageAlreadyExists when current == InstallPolicy.InstallFresh
                => InstallPolicy.ReinstallKeepData,
            InstallError.SignatureMismatch => InstallPolicy.UninstallThenInstall,
            _ => null
        };
}
