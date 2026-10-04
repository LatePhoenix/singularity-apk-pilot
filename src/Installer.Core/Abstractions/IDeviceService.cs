using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IDeviceService
{
    /// <summary>Lists devices; an adb failure reads as no devices.</summary>
    Task<IReadOnlyList<DeviceInfo>> DetectAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists devices, or returns null when adb could not answer.</summary>
    Task<IReadOnlyList<DeviceInfo>?> TryDetectAsync(CancellationToken cancellationToken = default);

    DeviceInfo? SelectPrimary(IReadOnlyList<DeviceInfo> devices);
}
