using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Recovery;

public sealed class RecoveryService : IRecoveryService
{
    private readonly AutoFixExecutor _autoFix;

    public RecoveryService(AutoFixExecutor autoFix)
    {
        _autoFix = autoFix;
    }

    public IReadOnlyList<RecoveryAction> Suggest(InstallError error, InstallManifest manifest)
    {
        var actions = new List<RecoveryAction>();
        switch (error)
        {
            case InstallError.UnauthorizedDevice:
            case InstallError.DebuggingNotApproved:
                actions.Add(Action("auth", "Approve this computer", "Put on the headset or unlock the phone and allow the prompt.", RecoveryActionKind.ShowAuthorization, false));
                actions.Add(Action("rescan", "Check again", "Look for the device after you allow access.", RecoveryActionKind.RetryDetection, true));
                break;
            case InstallError.NoDevicesFound:
            case InstallError.CableOrUsbModeIssue:
                actions.Add(Action("cable", "Check the cable", "Use a USB cable that can transfer files, not a charge-only cable.", RecoveryActionKind.ShowCableHelp, false));
                actions.Add(Action("restart", "Restart connection helper", "Restart the built-in connection helper and scan again.", RecoveryActionKind.RestartAdbServer, true));
                break;
            case InstallError.DeveloperModeLikelyDisabled:
                actions.Add(Action("devmode", "Turn on developer mode", "Use the Meta Horizon phone app to turn on developer mode.", RecoveryActionKind.ShowDeveloperMode, false));
                actions.Add(Action("rescan", "Check again", "Scan again after developer mode is on.", RecoveryActionKind.RetryDetection, true));
                break;
            case InstallError.OfflineDevice:
                actions.Add(Action("cable", "Reconnect the device", "Keep the device awake and try a different USB port.", RecoveryActionKind.ShowCableHelp, false));
                actions.Add(Action("retry", "Try again", "Retry after the device is connected.", RecoveryActionKind.RetryInstall, true));
                break;
            case InstallError.VersionDowngrade:
                actions.Add(Action("downgrade", "Replace the older app", "Install this test build over the older version.", RecoveryActionKind.RetryWithDowngrade, true));
                actions.Add(Action("uninstall", "Remove the old app first", "Uninstall the existing app, then install this build.", RecoveryActionKind.UninstallThenInstall, true));
                break;
            case InstallError.PackageAlreadyExists:
                actions.Add(Action("reinstall", "Replace the installed app", "Keep app data when possible and install again.", RecoveryActionKind.RetryInstall, true));
                break;
            case InstallError.SignatureMismatch:
                actions.Add(Action("uninstall", "Remove the old app first", "The copy already on the device cannot be updated in place.", RecoveryActionKind.UninstallThenInstall, true));
                break;
            case InstallError.InsufficientStorage:
                actions.Add(Action("retry", "Try again", "Free some space on the device, then retry.", RecoveryActionKind.RetryInstall, false));
                break;
            default:
                actions.Add(Action("restart", "Restart connection helper", "Restart the helper and try the install again.", RecoveryActionKind.RestartAdbServer, true));
                actions.Add(Action("retry", "Try again", "Run the install one more time.", RecoveryActionKind.RetryInstall, true));
                break;
        }

        actions.Add(Action("diag", "Export diagnostics", "Save a diagnostics file to send to support.", RecoveryActionKind.ExportDiagnostics, false));
        return actions.Take(3).ToList();
    }

    public Task<InstallResult?> TryAutoFixAsync(InstallRequest request, InstallResult failure, CancellationToken cancellationToken = default) =>
        _autoFix.ExecuteAsync(request, failure, cancellationToken);

    private static RecoveryAction Action(string id, string title, string description, RecoveryActionKind kind, bool automatic) =>
        new(id, title, description, kind, automatic);
}
