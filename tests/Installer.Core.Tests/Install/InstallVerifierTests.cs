using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Install;
using Installer.Core.Services.Recovery;

namespace Installer.Core.Tests.Install;

public sealed class InstallVerifierTests
{
    [Fact]
    public async Task Verify_uses_package_presence()
    {
        var adb = new FakeAdb { PackageInstalled = true };
        var verifier = new InstallVerifier(adb);
        Assert.True(await verifier.VerifyAsync("s", "com.app"));
    }

    [Fact]
    public void Failed_install_output_is_classified()
    {
        var classifier = new ErrorClassifier();
        Assert.Equal(InstallError.VersionDowngrade, classifier.Classify("Failure [INSTALL_FAILED_VERSION_DOWNGRADE]"));
    }

    private sealed class FakeAdb : IAdbClient
    {
        public bool PackageInstalled { get; set; }
        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task KillServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RestartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdbDeviceRecord>>([]);
        public Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default) => Task.FromResult(PackageInstalled);
        public Task<string> GetLogcatAsync(string serial, string? packageId, CancellationToken cancellationToken = default) => Task.FromResult("");
    }
}
