namespace Installer.Core.Models;

public sealed record DeviceHealth(bool AdbSeesDevice, bool WindowsSeesUsb, UsbEvidence Evidence)
{
    public DeviceHealth(bool adbSeesDevice, bool windowsSeesUsb)
        : this(adbSeesDevice, windowsSeesUsb, UsbEvidence.None)
    {
    }

    public string? Hint
    {
        get
        {
            if (AdbSeesDevice)
            {
                return null;
            }

            if (Evidence.AdbDriverMissing)
            {
                return "Windows sees a headset, but USB support is missing. Use Need help connecting to install Meta’s USB helper.";
            }

            if (Evidence.MtpOnly || (Evidence.QuestUsbPresent && !Evidence.AdbInterfacePresent))
            {
                return "Windows sees the headset, but this installer does not. Put the headset on, turn on developer mode, and allow this computer.";
            }

            if (Evidence.CompetingAdbProcess)
            {
                return "Another Android tool may be blocking the connection. Close it, then try again.";
            }

            return WindowsSeesUsb
                ? "Windows sees a headset, but this installer does not. Use a USB cable that can transfer files. If Windows shows an unknown device, install the Oculus ADB driver."
                : "This computer does not see a headset. Try a different cable or USB port, and avoid a hub.";
        }
    }

    public string? StatusChip
    {
        get
        {
            if (AdbSeesDevice)
            {
                return null;
            }

            if (Evidence.AdbDriverMissing)
            {
                return "This computer sees the headset, but USB support is missing.";
            }

            if (Evidence.MtpOnly || (Evidence.QuestUsbPresent && !Evidence.AdbInterfacePresent))
            {
                return "This computer sees the headset, but the headset has not allowed this installer yet.";
            }

            if (WindowsSeesUsb)
            {
                return "This computer sees a device, but this installer does not.";
            }

            return null;
        }
    }

    public string StatusTone => AdbSeesDevice ? "Live" : string.IsNullOrEmpty(StatusChip) ? "Idle" : "Warning";
}
