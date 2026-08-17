using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class DeviceHealthServiceTests
{
    [Fact]
    public void Usb_present_and_adb_empty_points_at_driver()
    {
        var health = new DeviceHealthService(new FakeUsb(true)).Snapshot([]);
        Assert.False(health.AdbSeesDevice);
        Assert.True(health.WindowsSeesUsb);
        Assert.Contains("Oculus ADB driver", health.Hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Usb_absent_and_adb_empty_points_at_cable()
    {
        var health = new DeviceHealthService(new FakeUsb(false)).Snapshot([]);
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
        var health = new DeviceHealthService(new FakeUsb(true)).Snapshot([device]);
        Assert.True(health.AdbSeesDevice);
        Assert.Null(health.Hint);
    }

    private sealed class FakeUsb(bool present) : IUsbPresenceProbe
    {
        public bool AndroidUsbPresent() => present;
    }
}
