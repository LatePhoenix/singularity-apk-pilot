using Installer.Core.Models;
using Installer.Core.Services.Content;
using Installer.Core.Services.Flow;
using Installer.Core.Services.Recovery;
using Installer.Core.Services.Support;
using Installer.Core.Abstractions;

namespace Installer.Core.Tests.Flow;

public sealed class WizardFlowServiceTests
{
    private readonly WizardFlowService _flow = Create();

    [Fact]
    public void Session_welcome_does_not_name_a_bundled_app()
    {
        var state = _flow.CreateInitialState(InstallManifest.Session);
        Assert.Equal("Install apps on your device", state.Copy.Headline);
        Assert.DoesNotContain("Halo", state.Copy.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("choose", state.Copy.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Quest_unauthorized_goes_to_authorization()
    {
        var state = Detected(Quest(DeviceConnectionState.Unauthorized));
        state = _flow.Advance(state, WizardTrigger.Continue, state.Device);
        Assert.Equal(WizardStep.Authorization, state.CurrentStep);
        Assert.Contains("headset", state.Copy.Headline, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Quest_authorized_but_not_ready_goes_to_developer_mode()
    {
        var state = Detected(Quest(DeviceConnectionState.Offline));
        state = _flow.Advance(state, WizardTrigger.Continue, state.Device);
        Assert.Equal(WizardStep.DeveloperMode, state.CurrentStep);
    }

    [Fact]
    public void Quest_ready_goes_to_ready_to_install()
    {
        var state = Detected(Quest(DeviceConnectionState.ConnectedReady));
        state = _flow.Advance(state, WizardTrigger.Continue, state.Device);
        Assert.Equal(WizardStep.ReadyToInstall, state.CurrentStep);
        Assert.Contains("Choose apps", state.Copy.Headline, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Android_unauthorized_goes_to_authorization()
    {
        var state = Detected(Phone(DeviceConnectionState.Unauthorized));
        state = _flow.Advance(state, WizardTrigger.Continue, state.Device);
        Assert.Equal(WizardStep.Authorization, state.CurrentStep);
        Assert.Contains("phone", state.Copy.Headline, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Android_ready_goes_to_ready_to_install()
    {
        var state = Detected(Phone(DeviceConnectionState.ConnectedReady));
        state = _flow.Advance(state, WizardTrigger.Continue, state.Device);
        Assert.Equal(WizardStep.ReadyToInstall, state.CurrentStep);
        Assert.Contains("Choose apps", state.Copy.Headline, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Install_failed_goes_to_problem()
    {
        var state = _flow.CreateInitialState(InstallManifest.Placeholder);
        state = _flow.Advance(state, WizardTrigger.Install, Quest(DeviceConnectionState.ConnectedReady));
        var failed = InstallResult.Failed(InstallError.VersionDowngrade, "Failure [INSTALL_FAILED_VERSION_DOWNGRADE]", []);
        state = _flow.Advance(state, WizardTrigger.InstallFinished, state.Device, failed);
        Assert.Equal(WizardStep.InstallProblem, state.CurrentStep);
        Assert.NotEmpty(state.SuggestedActions);
    }

    [Fact]
    public void Install_success_goes_to_complete()
    {
        var device = Quest(DeviceConnectionState.ConnectedReady);
        var state = _flow.CreateInitialState(InstallManifest.Placeholder);
        state = _flow.Advance(state, WizardTrigger.Install, device);
        var plan = new InstallPlan("com.singularity.exampleapp", "app.apk", ["-r"], false, true, false, InstallPolicy.ReinstallKeepData);
        state = _flow.Advance(state, WizardTrigger.InstallFinished, device, InstallResult.Succeeded("0.9.3", "Success", plan));
        Assert.Equal(WizardStep.Complete, state.CurrentStep);
    }

    [Fact]
    public void Installing_wifi_copy_does_not_require_cable()
    {
        var device = new DeviceInfo(
            "192.168.1.42:5555",
            "Oculus",
            "Quest 3",
            "14",
            DeviceKind.MetaQuest,
            DeviceConnectionState.ConnectedReady,
            true,
            true,
            new Dictionary<string, string>());
        var state = _flow.CreateInitialState(InstallManifest.Placeholder);
        state = _flow.Advance(state, WizardTrigger.Install, device);
        Assert.Equal(WizardStep.Installing, state.CurrentStep);
        Assert.Contains("Wi-Fi", state.Copy.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cable", state.Copy.Body, StringComparison.OrdinalIgnoreCase);
    }

    private WizardState Detected(DeviceInfo device)
    {
        var state = _flow.CreateInitialState(InstallManifest.Placeholder);
        state = _flow.Advance(state, WizardTrigger.Start);
        return _flow.Advance(state, WizardTrigger.DeviceRefresh, device);
    }

    private static DeviceInfo Quest(DeviceConnectionState state) =>
        new("quest-serial", "Oculus", "Quest 3", "14", DeviceKind.MetaQuest, state, state == DeviceConnectionState.ConnectedReady, true, new Dictionary<string, string>());

    private static DeviceInfo Phone(DeviceConnectionState state) =>
        new("phone-serial", "Google", "Pixel 9", "15", DeviceKind.AndroidPhone, state, state == DeviceConnectionState.ConnectedReady, false, new Dictionary<string, string>());

    private static WizardFlowService Create()
    {
        var copy = new CopyDeckService(new FriendlyMessageService());
        var recovery = new RecoveryService(new AutoFixExecutor(new NoopAdb(), new NoopInstall(), new RetryPolicyFactory(), new NoopLog()));
        return new WizardFlowService(new FlowDecisionEngine(new QuestFlowStrategy(), new AndroidPhoneFlowStrategy()), copy, recovery);
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
