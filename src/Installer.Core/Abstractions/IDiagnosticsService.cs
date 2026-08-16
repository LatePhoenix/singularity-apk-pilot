using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IDiagnosticsService
{
    Task<DiagnosticBundleInfo> ExportAsync(
        InstallManifest manifest,
        DeviceInfo? device,
        InstallResult? lastResult,
        string? adbDevicesRaw,
        string destinationDirectory,
        CancellationToken cancellationToken = default);
}
