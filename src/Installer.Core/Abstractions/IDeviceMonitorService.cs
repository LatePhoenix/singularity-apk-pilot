using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IDeviceMonitorService
{
    event EventHandler<IReadOnlyList<DeviceInfo>>? DevicesChanged;
    IReadOnlyList<DeviceInfo> CurrentDevices { get; }
    Task StartAsync(CancellationToken cancellationToken = default);
    void Stop();
}
