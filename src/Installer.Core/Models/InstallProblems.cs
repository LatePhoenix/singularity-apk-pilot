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

    /// <summary>Retrying the same file cannot help; the tester needs a different APK.</summary>
    public static bool NeedsDifferentFile(InstallError? error) =>
        error is InstallError.MissingPayload
            or InstallError.MissingSplit
            or InstallError.IncompatibleAbi
            or InstallError.AppTargetsOldAndroid
            or InstallError.DeviceAndroidTooOld
            or InstallError.InvalidApk;

    public static InstallPolicy? PolicyFor(RecoveryActionKind kind) => kind switch
    {
        RecoveryActionKind.RetryWithDowngrade => InstallPolicy.ReinstallAllowDowngrade,
        RecoveryActionKind.UninstallThenInstall => InstallPolicy.UninstallThenInstall,
        RecoveryActionKind.ReplaceExistingApp => InstallPolicy.ReinstallKeepData,
        _ => null
    };
}
