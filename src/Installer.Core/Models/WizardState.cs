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
    bool DeveloperModeLikelyRequired = false,
    IReadOnlyList<DeviceInfo>? ReadyDevices = null,
    DeviceHealth? Health = null,
    WizardStep? ReturnStep = null)
{
    public IReadOnlyList<DeviceInfo> Ready => ReadyDevices ?? [];

    public bool NeedsDevicePicker => Ready.Count(d => d.State == DeviceConnectionState.ConnectedReady) >= 2;

    public WizardState WithStep(WizardStep step, WizardCopy copy) =>
        this with { CurrentStep = step, Copy = copy, IsBusy = false };
}
