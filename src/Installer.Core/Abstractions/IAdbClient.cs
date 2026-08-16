using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IAdbClient
{
    Task StartServerAsync(CancellationToken cancellationToken = default);
    Task KillServerAsync(CancellationToken cancellationToken = default);
    Task RestartServerAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default);
    Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<string> GetLogcatAsync(string serial, string? packageId, CancellationToken cancellationToken = default);
}
