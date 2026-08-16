using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public sealed class QuestFlowStrategy : IWizardFlowStrategy
{
    public DeviceKind Kind => DeviceKind.MetaQuest;

    public WizardStep NextAfterDetection(DeviceInfo device, int connectAttempts)
    {
        return device.State switch
        {
            DeviceConnectionState.ConnectedReady => WizardStep.ReadyToInstall,
            DeviceConnectionState.Unauthorized => WizardStep.Authorization,
            DeviceConnectionState.Offline => WizardStep.DeveloperMode,
            DeviceConnectionState.NotConnected => connectAttempts >= 2 ? WizardStep.DeveloperMode : WizardStep.ConnectDevice,
            _ => WizardStep.ConnectDevice
        };
    }
}
