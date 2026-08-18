using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface ITroubleshootingService
{
    TroubleshootSession Start(
        WizardStep returnStep,
        UsbEvidence evidence,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> devices);

    TroubleshootSession SelectFamily(
        TroubleshootSession session,
        TroubleshootFamily family,
        IReadOnlyList<DeviceInfo> devices);

    TroubleshootSession Confirm(TroubleshootSession session, IReadOnlyList<DeviceInfo> devices);

    TroubleshootSession Back(TroubleshootSession session, IReadOnlyList<DeviceInfo> devices);

    TroubleshootSession ApplyEvidence(
        TroubleshootSession session,
        UsbEvidence evidence,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> devices);
}
