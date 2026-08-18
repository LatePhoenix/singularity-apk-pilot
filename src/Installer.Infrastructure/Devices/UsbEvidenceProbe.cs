using System.Runtime.Versioning;
using Microsoft.Win32;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Devices;

namespace Installer.Infrastructure.Devices;

[SupportedOSPlatform("windows")]
public sealed class UsbEvidenceProbe : IUsbEvidenceProbe, IUsbPresenceProbe
{
    private readonly IPortableAdbLocator _adb;

    public UsbEvidenceProbe(IPortableAdbLocator adb)
    {
        _adb = adb;
    }

    public bool AndroidUsbPresent() => Collect().WindowsSeesUsb;

    public UsbEvidence Collect()
    {
        var quest = false;
        var android = false;
        var adbInterface = false;
        var driverMissing = false;
        var mtp = false;

        try
        {
            using var usb = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\USB");
            if (usb is not null)
            {
                foreach (var vendor in usb.GetSubKeyNames())
                {
                    using var vendorKey = usb.OpenSubKey(vendor);
                    if (vendorKey is null)
                    {
                        continue;
                    }

                    var vendorQuest = UsbDeviceClassifier.IsQuestVendor(vendor);
                    var vendorAndroid = UsbDeviceClassifier.IsAndroidVendor(vendor);
                    foreach (var instance in vendorKey.GetSubKeyNames())
                    {
                        using var instanceKey = vendorKey.OpenSubKey(instance);
                        if (instanceKey is null)
                        {
                            continue;
                        }

                        var blob = ReadBlob(vendor, instance, instanceKey);
                        if (UsbDeviceClassifier.IsQuestVendor(blob) || vendorQuest)
                        {
                            quest = true;
                        }

                        if (UsbDeviceClassifier.IsAndroidVendor(blob) || vendorAndroid)
                        {
                            android = true;
                        }

                        if (UsbDeviceClassifier.IsAdbInterface(blob))
                        {
                            adbInterface = true;
                            var desc = instanceKey.GetValue("DeviceDesc") as string ?? "";
                            var service = instanceKey.GetValue("Service") as string;
                            var classGuid = instanceKey.GetValue("ClassGUID") as string;
                            if (UsbDeviceClassifier.LooksLikeMissingDriver(desc, service, classGuid)
                                || HasFailedInstall(instanceKey))
                            {
                                driverMissing = true;
                            }
                        }

                        if (UsbDeviceClassifier.IsMtpInterface(blob))
                        {
                            mtp = true;
                        }
                    }
                }
            }
        }
        catch
        {
            // Registry access is best-effort; empty evidence is safe.
        }

        if (quest && !adbInterface && mtp)
        {
            // MTP-only: Windows AutoPlay without an ADB interface.
        }

        return new UsbEvidence(
            quest,
            android && !quest,
            adbInterface,
            driverMissing,
            quest && mtp && !adbInterface,
            HasCompetingAdb());
    }

    private bool HasCompetingAdb()
    {
        System.Diagnostics.Process[] processes;
        try
        {
            processes = System.Diagnostics.Process.GetProcessesByName("adb");
        }
        catch
        {
            return false;
        }

        try
        {
            if (processes.Length <= 1)
            {
                return false;
            }

            var bundled = _adb.FindAdbExecutable();
            if (string.IsNullOrWhiteSpace(bundled))
            {
                return processes.Length > 0;
            }

            var bundledFull = Path.GetFullPath(bundled);
            foreach (var process in processes)
            {
                try
                {
                    var path = process.MainModule?.FileName;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        continue;
                    }

                    if (!string.Equals(Path.GetFullPath(path), bundledFull, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                catch
                {
                    if (processes.Length > 1)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        finally
        {
            foreach (var process in processes)
            {
                process.Dispose();
            }
        }
    }

    private static string ReadBlob(string vendor, string instance, RegistryKey instanceKey)
    {
        var desc = instanceKey.GetValue("DeviceDesc") as string ?? "";
        var friendly = instanceKey.GetValue("FriendlyName") as string ?? "";
        var mfg = instanceKey.GetValue("Mfg") as string ?? "";
        var hardware = JoinMulti(instanceKey.GetValue("HardwareID"));
        var compatible = JoinMulti(instanceKey.GetValue("CompatibleIDs"));
        return $"{vendor} {instance} {desc} {friendly} {mfg} {hardware} {compatible}";
    }

    private static string JoinMulti(object? value)
    {
        if (value is string[] items)
        {
            return string.Join(" ", items);
        }

        return value as string ?? "";
    }

    private static bool HasFailedInstall(RegistryKey instanceKey)
    {
        if (instanceKey.GetValue("ConfigFlags") is int flags && (flags & 0x40) != 0)
        {
            return true;
        }

        if (instanceKey.GetValue("Problem") is int problem && problem is 28 or 10)
        {
            return true;
        }

        return false;
    }
}
