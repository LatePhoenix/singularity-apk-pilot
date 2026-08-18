using Installer.Contracts.Dtos;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Diagnostics;

namespace Installer.Core.Tests.Diagnostics;

public sealed class DiagnosticsServiceTests
{
    [Fact]
    public async Task Export_includes_session_log_with_hashed_serial()
    {
        var zip = new CapturingZip();
        var clock = new FixedClock(new DateTimeOffset(2026, 8, 18, 12, 0, 0, TimeSpan.Zero));
        var service = new DiagnosticsService(
            new NoopAdb(),
            clock,
            zip,
            new LogcatCollector(new NoopAdb()),
            new EnvironmentSnapshotService(new NoopAdbLocator(), clock),
            new FakeUsb(),
            new FakeSessionLog("device ABC123 connected"));

        var dest = Path.Combine(Path.GetTempPath(), "singularity-diag-test");
        Directory.CreateDirectory(dest);
        var device = new DeviceInfo(
            "ABC123",
            "Oculus",
            "Quest 2",
            "12",
            DeviceKind.MetaQuest,
            DeviceConnectionState.Unauthorized,
            false,
            true,
            new Dictionary<string, string>());

        await service.ExportAsync(InstallManifest.Session, device, null, null, dest);

        Assert.True(zip.Files.ContainsKey("session-log.txt"));
        Assert.DoesNotContain("ABC123", zip.Files["session-log.txt"], StringComparison.Ordinal);
        Assert.Contains("connected", zip.Files["session-log.txt"], StringComparison.Ordinal);
    }

    private sealed class CapturingZip : IZipBundleWriter
    {
        public Dictionary<string, string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

        public Task WriteAsync(string zipPath, IReadOnlyDictionary<string, string> textFiles, CancellationToken cancellationToken = default)
        {
            Files.Clear();
            foreach (var pair in textFiles)
            {
                Files[pair.Key] = pair.Value;
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }

    private sealed class FakeSessionLog(string text) : ISessionLog
    {
        public string ReadAll() => text;
    }

    private sealed class FakeUsb : IUsbEvidenceProbe
    {
        public UsbEvidence Collect() => UsbEvidence.None;
    }

    private sealed class NoopAdbLocator : IPortableAdbLocator
    {
        public string? FindAdbExecutable() => @"C:\adb.exe";
    }

    private sealed class NoopAdb : IAdbClient
    {
        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task KillServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<AdbProcessResult> RestartServerAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, ["start-server"]));
        public Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AdbDeviceRecord>>([]);
        public Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyList<string>> ListThirdPartyPackagesAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
        public Task<string> DumpPackageAsync(string serial, string packageId, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<string> GetLogcatAsync(string serial, string? packageId, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<AdbProcessResult> TcpIpAsync(string serial, int port = 5555, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> ConnectAsync(string endpoint, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> DisconnectAsync(string? endpoint = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> PairAsync(string endpoint, string pairingCode, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, []));
        public Task<string?> GetWifiAddressAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);
        public Task<AdbProcessResult> InstallMultipleAsync(string serial, IReadOnlyList<string> apkPaths, IReadOnlyList<string> flags, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<string?> ResolveLauncherAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult<string?>(null);
        public Task<AdbProcessResult> LaunchAsync(string serial, string packageId, string? activity, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Starting", "", TimeSpan.Zero, []));
    }
}
