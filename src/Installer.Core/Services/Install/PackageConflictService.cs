using Installer.Core.Models;
using Installer.Core.Services.Recovery;

namespace Installer.Core.Services.Install;

public sealed class PackageConflictService
{
    private readonly ErrorClassifier _classifier;

    public PackageConflictService(ErrorClassifier classifier)
    {
        _classifier = classifier;
    }

    public InstallError? ClassifyConflict(string output) =>
        _classifier.Classify(output) is var error && IsConflict(error) ? error : null;

    public InstallPolicy? SuggestedAlternatePolicy(InstallError error, InstallPolicy current)
    {
        return error switch
        {
            InstallError.VersionDowngrade when current != InstallPolicy.ReinstallAllowDowngrade && current != InstallPolicy.UninstallThenInstall
                => InstallPolicy.ReinstallAllowDowngrade,
            InstallError.PackageAlreadyExists when current == InstallPolicy.InstallFresh
                => InstallPolicy.ReinstallKeepData,
            InstallError.SignatureMismatch => InstallPolicy.UninstallThenInstall,
            _ => null
        };
    }

    private static bool IsConflict(InstallError error) =>
        error is InstallError.VersionDowngrade or InstallError.PackageAlreadyExists or InstallError.SignatureMismatch;
}
