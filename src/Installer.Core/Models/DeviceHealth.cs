namespace Installer.Core.Models;

public sealed record DeviceHealth(bool AdbSeesDevice, bool WindowsSeesUsb)
{
    public string? Hint
    {
        get
        {
            if (AdbSeesDevice)
            {
                return null;
            }

            return WindowsSeesUsb
                ? "Windows sees a headset, but this installer does not. Use a USB cable that can transfer files. If Windows shows an unknown device, install the Oculus ADB driver."
                : "This computer does not see a headset. Try a different cable or USB port, and avoid a hub.";
        }
    }
}
