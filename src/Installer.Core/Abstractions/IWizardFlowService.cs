using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IWizardFlowService
{
    WizardState CreateInitialState(InstallManifest manifest);
    WizardState Advance(
        WizardState state,
        WizardTrigger trigger,
        DeviceInfo? device = null,
        InstallResult? installResult = null,
        IReadOnlyList<DeviceInfo>? readyDevices = null,
        DeviceHealth? health = null,
        TroubleshootSession? troubleshoot = null);
}
