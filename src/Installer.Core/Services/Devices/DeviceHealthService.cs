using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Devices;

public sealed class DeviceHealthService : IDeviceHealthService
{
    private readonly IUsbEvidenceProbe _usb;

    public DeviceHealthService(IUsbEvidenceProbe usb)
    {
        _usb = usb;
    }

    public DeviceHealth Snapshot(IReadOnlyList<DeviceInfo> devices)
    {
        var evidence = _usb.Collect();
        var adbSees = devices.Any(d => d.State != DeviceConnectionState.NotConnected);
        return new DeviceHealth(adbSees, evidence.WindowsSeesUsb, evidence);
    }
}
