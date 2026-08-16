using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IDeviceService
{
    Task<IReadOnlyList<DeviceInfo>> DetectAsync(CancellationToken cancellationToken = default);
    DeviceInfo? SelectPrimary(IReadOnlyList<DeviceInfo> devices);
}
