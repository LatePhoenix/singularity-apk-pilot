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
    Task<AdbProcessResult> InstallMultipleAsync(string serial, IReadOnlyList<string> apkPaths, IReadOnlyList<string> flags, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ListThirdPartyPackagesAsync(string serial, CancellationToken cancellationToken = default);
    Task<string> DumpPackageAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<string> GetLogcatAsync(string serial, string? packageId, CancellationToken cancellationToken = default);
    Task<string?> ResolveLauncherAsync(string serial, string packageId, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> LaunchAsync(string serial, string packageId, string? activity, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> TcpIpAsync(string serial, int port = 5555, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> ConnectAsync(string endpoint, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> DisconnectAsync(string? endpoint = null, CancellationToken cancellationToken = default);
    Task<AdbProcessResult> PairAsync(string endpoint, string pairingCode, CancellationToken cancellationToken = default);
    Task<string?> GetWifiAddressAsync(string serial, CancellationToken cancellationToken = default);
}
