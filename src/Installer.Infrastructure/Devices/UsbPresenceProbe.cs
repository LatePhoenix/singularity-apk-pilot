using System.Runtime.Versioning;
using Microsoft.Win32;
using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Devices;

[SupportedOSPlatform("windows")]
public sealed class UsbPresenceProbe : IUsbPresenceProbe
{
    private static readonly string[] Needles = ["VID_2833", "ADB", "Android", "Oculus", "Quest", "Meta"];

    public bool AndroidUsbPresent()
    {
        try
        {
            using var usb = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");
            if (usb is null)
            {
                return false;
            }

            foreach (var vendor in usb.GetSubKeyNames())
            {
                if (ContainsNeedle(vendor))
                {
                    return true;
                }

                using var vendorKey = usb.OpenSubKey(vendor);
                if (vendorKey is null)
                {
                    continue;
                }

                foreach (var instance in vendorKey.GetSubKeyNames())
                {
                    using var instanceKey = vendorKey.OpenSubKey(instance);
                    var desc = instanceKey?.GetValue("DeviceDesc") as string
                               ?? instanceKey?.GetValue("FriendlyName") as string
                               ?? instanceKey?.GetValue("Mfg") as string
                               ?? "";
                    if (ContainsNeedle(vendor) || ContainsNeedle(desc) || ContainsNeedle(instance))
                    {
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool ContainsNeedle(string value) =>
        Needles.Any(needle => value.Contains(needle, StringComparison.OrdinalIgnoreCase));
}
