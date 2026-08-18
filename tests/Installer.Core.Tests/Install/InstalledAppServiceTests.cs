using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;
using Installer.Core.Services.Install;
using Installer.Core.Services.Packages;

namespace Installer.Core.Tests.Install;

public sealed class InstalledAppServiceTests
{
    [Fact]
    public void Protects_system_and_horizon_ids()
    {
        Assert.True(ProtectedPackageFilter.IsProtected("com.android.systemui"));
        Assert.True(ProtectedPackageFilter.IsProtected("com.oculus.vrshell"));
        Assert.True(ProtectedPackageFilter.IsProtected("com.oculus.os.vrlockscreen"));
        Assert.True(ProtectedPackageFilter.IsProtected("android"));
        Assert.False(ProtectedPackageFilter.IsProtected("com.singularity.demo"));
        Assert.True(ProtectedPackageFilter.IsProtected("com.bad;rm"));
    }

    [Fact]
    public async Task List_skips_protected_and_marks_recents()
    {
        var adb = new FakeAdb
        {
            Packages = ["com.singularity.demo", "com.oculus.vrshell", "com.other.app"]
        };
        var sut = Create(adb);
        var result = await sut.ListAsync("S1", new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "com.singularity.demo" });
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value!.Count);
        Assert.Contains(result.Value, app => app.PackageId == "com.singularity.demo" && app.IsRecent);
        Assert.Contains(result.Value, app => app.PackageId == "com.other.app" && !app.IsRecent);
        Assert.DoesNotContain(result.Value, app => app.PackageId.Contains("oculus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Uninstall_refuses_protected_ids()
    {
        var adb = new FakeAdb();
        var result = await Create(adb).UninstallAsync("S1", "com.oculus.vrshell");
        Assert.False(result.Success);
        Assert.Equal(0, adb.UninstallCalls);
    }

    [Fact]
    public async Task Uninstall_succeeds_when_gone_after()
    {
        var adb = new FakeAdb { UninstallOutput = "Success", PackageInstalled = false };
        var result = await Create(adb).UninstallAsync("S1", "com.singularity.demo");
        Assert.True(result.Success);
        Assert.Equal(1, adb.UninstallCalls);
    }

    [Fact]
    public async Task Uninstall_fails_when_still_installed()
    {
        var adb = new FakeAdb { UninstallOutput = "Success", PackageInstalled = true };
        var result = await Create(adb).UninstallAsync("S1", "com.singularity.demo");
        Assert.False(result.Success);
        Assert.Equal(InstallError.UninstallFailed, result.Error);
    }

    [Fact]
    public async Task Uninstall_fails_on_adb_failure_text()
    {
        var adb = new FakeAdb { UninstallExit = 1, UninstallOutput = "Failure [DELETE_FAILED_INTERNAL_ERROR]" };
        var result = await Create(adb).UninstallAsync("S1", "com.singularity.demo");
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Enrich_reads_label_and_version()
    {
        var adb = new FakeAdb { Dump = "versionName=9.0\napplicationLabel=Demo\n" };
        var enriched = await Create(adb).EnrichAsync("S1", new InstalledApp("com.singularity.demo"));
        Assert.Equal("Demo", enriched.Label);
        Assert.Equal("9.0", enriched.Version);
    }

    private static InstalledAppService Create(FakeAdb adb) =>
        new(adb, new AdbOutputParser(), new NoopLog());

    private sealed class FakeAdb : IAdbClient
    {
        public IReadOnlyList<string> Packages { get; set; } = [];
        public string UninstallOutput { get; set; } = "Success";
        public int UninstallExit { get; set; }
        public bool PackageInstalled { get; set; }
        public int UninstallCalls { get; private set; }
        public string Dump { get; set; } = "";

        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task KillServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<AdbProcessResult> RestartServerAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, ["start-server"]));
        public Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AdbDeviceRecord>>([]);
        public Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default)
        {
            UninstallCalls++;
            return Task.FromResult(new AdbProcessResult(UninstallExit, UninstallOutput, "", TimeSpan.Zero, []));
        }

        public Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(PackageInstalled);
        public Task<IReadOnlyList<string>> ListThirdPartyPackagesAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult(Packages);
        public Task<string> DumpPackageAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Dump);
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

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? exception = null) { }
    }
}
