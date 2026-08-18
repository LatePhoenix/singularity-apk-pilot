namespace Installer.Core.Models;

public sealed record UsbEvidence(
    bool QuestUsbPresent,
    bool AndroidUsbPresent,
    bool AdbInterfacePresent,
    bool AdbDriverMissing,
    bool MtpOnly,
    bool CompetingAdbProcess)
{
    public static UsbEvidence None { get; } = new(false, false, false, false, false, false);

    public bool WindowsSeesUsb => QuestUsbPresent || AndroidUsbPresent;
}
