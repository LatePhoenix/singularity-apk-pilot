namespace Installer.Core.Models;

public static class InstallProblems
{
    public static bool NeedsConnectionHelp(InstallError? error) =>
        error is InstallError.UnauthorizedDevice
            or InstallError.DebuggingNotApproved
            or InstallError.OfflineDevice
            or InstallError.NoDevicesFound
            or InstallError.CableOrUsbModeIssue
            or InstallError.WirelessConnectFailed
            or InstallError.DeveloperModeLikelyDisabled;

    public static InstallPolicy? PolicyFor(RecoveryActionKind kind) => kind switch
    {
        RecoveryActionKind.RetryWithDowngrade => InstallPolicy.ReinstallAllowDowngrade,
        RecoveryActionKind.UninstallThenInstall => InstallPolicy.UninstallThenInstall,
        RecoveryActionKind.ReplaceExistingApp => InstallPolicy.ReinstallKeepData,
        _ => null
    };
}
