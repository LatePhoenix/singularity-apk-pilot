using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Devices;

public sealed class DeviceHealthService : IDeviceHealthService
{
    private readonly IUsbPresenceProbe _usb;

    public DeviceHealthService(IUsbPresenceProbe usb)
    {
        _usb = usb;
    }

    public DeviceHealth Snapshot(IReadOnlyList<DeviceInfo> devices)
    {
        var adbSees = devices.Any(d => d.State != DeviceConnectionState.NotConnected);
        return new DeviceHealth(adbSees, _usb.AndroidUsbPresent());
    }
}
