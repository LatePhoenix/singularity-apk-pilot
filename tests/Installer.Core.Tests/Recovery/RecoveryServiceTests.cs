using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Recovery;

namespace Installer.Core.Tests.Recovery;

public sealed class RecoveryServiceTests
{
    private readonly RecoveryService _sut = new(new AutoFixExecutor(new NoopAdb(), new NoopInstall(), new RetryPolicyFactory(), new NoopLog()));

    [Theory]
    [InlineData(InstallError.UnauthorizedDevice)]
    [InlineData(InstallError.OfflineDevice)]
    [InlineData(InstallError.NoDevicesFound)]
    [InlineData(InstallError.VersionDowngrade)]
    [InlineData(InstallError.PackageAlreadyExists)]
    [InlineData(InstallError.SignatureMismatch)]
    [InlineData(InstallError.InsufficientStorage)]
    [InlineData(InstallError.DeveloperModeLikelyDisabled)]
    [InlineData(InstallError.CableOrUsbModeIssue)]
    [InlineData(InstallError.UnknownInstallFailure)]
    [InlineData(InstallError.MissingPayload)]
    [InlineData(InstallError.WirelessConnectFailed)]
    [InlineData(InstallError.MissingSplit)]
    [InlineData(InstallError.UninstallFailed)]
    public void Suggests_at_most_three_actions(InstallError error)
    {
        var actions = _sut.Suggest(error, InstallManifest.Placeholder);
        Assert.InRange(actions.Count, 1, 3);
        Assert.Contains(actions, a => a.Kind == RecoveryActionKind.ExportDiagnostics || a.IsAutomatic || !string.IsNullOrWhiteSpace(a.Title));
    }

    [Fact]
    public void Downgrade_offers_automatic_replace()
    {
        var actions = _sut.Suggest(InstallError.VersionDowngrade, InstallManifest.Placeholder);
        Assert.Contains(actions, a => a.Kind == RecoveryActionKind.RetryWithDowngrade && a.IsAutomatic);
    }

    [Fact]
    public void Already_exists_offers_replace_this_app()
    {
        var actions = _sut.Suggest(InstallError.PackageAlreadyExists, InstallManifest.Placeholder);
        Assert.Contains(actions, a => a.Kind == RecoveryActionKind.ReplaceExistingApp && a.Title.Contains("Replace", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class NoopAdb : IAdbClient
    {
        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task KillServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RestartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<AdbDeviceRecord>>([]);
        public Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default) => Task.FromResult("");
        public Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "Success", "", TimeSpan.Zero, []));
        public Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<IReadOnlyList<string>> ListThirdPartyPackagesAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<string>>([]);
        public Task<string> DumpPackageAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
            Task.FromResult("");
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

    private sealed class NoopInstall : IInstallService
    {
        public InstallPlan CreatePlan(InstallRequest request) => new(request.Manifest.AppId, request.Manifest.ApkPath, [], false, true, false, request.Manifest.InstallPolicy);
        public Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default) =>
            Task.FromResult(InstallResult.Succeeded("1", "Success", CreatePlan(request)));
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? exception = null) { }
    }
}
