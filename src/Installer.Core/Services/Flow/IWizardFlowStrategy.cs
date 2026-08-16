using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public interface IWizardFlowStrategy
{
    DeviceKind Kind { get; }
    WizardStep NextAfterDetection(DeviceInfo device, int connectAttempts);
}
