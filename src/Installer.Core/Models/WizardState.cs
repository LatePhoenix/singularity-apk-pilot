namespace Installer.Core.Models;

public sealed record WizardState(
    WizardStep CurrentStep,
    InstallManifest Manifest,
    DeviceInfo? Device,
    InstallResult? LastInstallResult,
    IReadOnlyList<RecoveryAction> SuggestedActions,
    WizardCopy Copy,
    bool IsBusy = false,
    string? StatusMessage = null,
    int ConnectAttempts = 0,
    bool DeveloperModeLikelyRequired = false)
{
    public WizardState WithStep(WizardStep step, WizardCopy copy) =>
        this with { CurrentStep = step, Copy = copy, IsBusy = false };
}
