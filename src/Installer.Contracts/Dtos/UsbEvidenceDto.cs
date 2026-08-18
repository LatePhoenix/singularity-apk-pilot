namespace Installer.Contracts.Dtos;

public sealed class UsbEvidenceDto
{
    public bool QuestUsbPresent { get; set; }
    public bool AndroidUsbPresent { get; set; }
    public bool AdbInterfacePresent { get; set; }
    public bool AdbDriverMissing { get; set; }
    public bool MtpOnly { get; set; }
    public bool CompetingAdbProcess { get; set; }
}
