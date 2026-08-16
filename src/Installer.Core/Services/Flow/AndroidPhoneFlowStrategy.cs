using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public sealed class AndroidPhoneFlowStrategy : IWizardFlowStrategy
{
    public DeviceKind Kind => DeviceKind.AndroidPhone;

    public WizardStep NextAfterDetection(DeviceInfo device, int connectAttempts)
    {
        return device.State switch
        {
            DeviceConnectionState.ConnectedReady => WizardStep.ReadyToInstall,
            DeviceConnectionState.Unauthorized => WizardStep.Authorization,
            _ => WizardStep.ConnectDevice
        };
    }
}
