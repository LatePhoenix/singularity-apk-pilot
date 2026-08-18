using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class DeviceHealthServiceTests
{
    [Fact]
    public void Usb_present_without_adb_interface_points_at_headset_setup()
    {
        var evidence = new UsbEvidence(true, false, false, false, true, false);
        var health = new DeviceHealthService(new FakeUsb(evidence)).Snapshot([]);
        Assert.False(health.AdbSeesDevice);
        Assert.True(health.WindowsSeesUsb);
        Assert.Contains("developer mode", health.Hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("sees the headset", health.StatusChip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Missing_driver_points_at_usb_helper()
    {
        var evidence = new UsbEvidence(true, false, true, true, false, false);
        var health = new DeviceHealthService(new FakeUsb(evidence)).Snapshot([]);
        Assert.Contains("USB helper", health.Hint, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("USB support is missing", health.StatusChip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Usb_absent_and_adb_empty_points_at_cable()
    {
        var health = new DeviceHealthService(new FakeUsb(UsbEvidence.None)).Snapshot([]);
        Assert.False(health.AdbSeesDevice);
        Assert.False(health.WindowsSeesUsb);
        Assert.Contains("cable", health.Hint, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("driver", health.Hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Adb_sees_device_has_no_hint()
    {
        var device = new DeviceInfo(
            "1WMHH",
            "Oculus",
            "Quest 3",
            "14",
            DeviceKind.MetaQuest,
            DeviceConnectionState.Unauthorized,
            false,
            true,
            new Dictionary<string, string>());
        var health = new DeviceHealthService(new FakeUsb(new UsbEvidence(true, false, true, false, false, false))).Snapshot([device]);
        Assert.True(health.AdbSeesDevice);
        Assert.Null(health.Hint);
    }

    private sealed class FakeUsb(UsbEvidence evidence) : IUsbEvidenceProbe
    {
        public UsbEvidence Collect() => evidence;
    }
}
