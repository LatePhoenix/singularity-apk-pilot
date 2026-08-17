using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class DeviceDetectionServiceTests
{
    private readonly DeviceDetectionService _sut = new(
        new NoopAdb(),
        new AdbOutputParser(),
        new DevicePropertyService(new NoopAdb()),
        new DeviceClassificationService(),
        new NoopLog());

    [Fact]
    public void SelectPrimary_prefers_wifi_quest_when_usb_also_ready()
    {
        var usb = Device("1WMHH000000001", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady);
        var wifi = Device("192.168.1.42:5555", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady);

        var selected = _sut.SelectPrimary([usb, wifi]);

        Assert.NotNull(selected);
        Assert.Equal(wifi.Serial, selected.Serial);
        Assert.True(selected.IsWireless);
    }

    [Fact]
    public void Usb_serial_is_not_wireless()
    {
        var device = Device("1WMHH000000001", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady);
        Assert.Equal(DeviceTransport.Usb, device.Transport);
        Assert.False(device.IsWireless);
    }

    [Theory]
    [InlineData("192.168.1.42:5555", true)]
    [InlineData("10.0.0.8", true)]
    [InlineData("1WMHH000000001", false)]
    [InlineData("emulator-5554", false)]
    public void Parses_wireless_endpoints(string value, bool expected)
    {
        Assert.Equal(expected, WirelessEndpoint.TryParse(value, out _));
        Assert.Equal(expected, WirelessEndpoint.IsWifiSerial(value));
    }

    [Fact]
    public void SelectPrimary_returns_null_when_empty() =>
        Assert.Null(_sut.SelectPrimary([]));

    [Fact]
    public void SelectPrimary_returns_the_only_ready_device()
    {
        var quest = Device("1WMHH000000001", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady);
        Assert.Equal(quest.Serial, _sut.SelectPrimary([quest])!.Serial);
    }

    [Fact]
    public void SelectPrimary_prefers_quest_when_two_ready()
    {
        var phone = Device("PIXEL9", DeviceKind.AndroidPhone, DeviceConnectionState.ConnectedReady);
        var quest = Device("1WMHH000000001", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady);
        Assert.Equal(quest.Serial, _sut.SelectPrimary([phone, quest])!.Serial);
    }

    private static DeviceInfo Device(string serial, DeviceKind kind, DeviceConnectionState state) =>
        new(serial,
            kind == DeviceKind.MetaQuest ? "Oculus" : "Google",
            kind == DeviceKind.MetaQuest ? "Quest 3" : "Pixel 9",
            "14",
            kind,
            state,
            state == DeviceConnectionState.ConnectedReady,
            kind == DeviceKind.MetaQuest,
            new Dictionary<string, string>());

    private sealed class NoopAdb : IAdbClient
    {
        public Task StartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task KillServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task RestartServerAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<AdbDeviceRecord>>([]);
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
