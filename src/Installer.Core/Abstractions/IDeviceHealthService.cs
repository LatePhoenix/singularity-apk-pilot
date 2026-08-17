using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IDeviceHealthService
{
    DeviceHealth Snapshot(IReadOnlyList<DeviceInfo> devices);
}
