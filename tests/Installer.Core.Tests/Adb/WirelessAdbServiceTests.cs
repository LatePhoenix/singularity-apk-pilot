using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Adb;

public sealed class WirelessAdbServiceTests
{
    [Fact]
    public async Task EnableFromUsb_reads_ip_then_connects_and_saves()
    {
        var adb = new FakeAdb
        {
            WifiAddress = "192.168.1.42",
            ConnectOutput = "connected to 192.168.1.42:5555"
        };
        var store = new MemoryStore();
        var sut = Create(adb, store);

        var result = await sut.EnableFromUsbAsync("USBSERIAL");

        Assert.True(result.IsSuccess);
        Assert.Equal("192.168.1.42:5555", result.Value!.Address);
        Assert.Equal("USBSERIAL", adb.TcpIpSerial);
        Assert.Equal("192.168.1.42:5555", adb.LastConnect);
        Assert.Equal("192.168.1.42:5555", store.Endpoint!.Address);
        Assert.Equal(sut.LastEndpoint, store.Endpoint);
    }

    [Fact]
    public async Task EnableFromUsb_fails_when_no_wifi_address()
    {
        var sut = Create(new FakeAdb { WifiAddress = null, ConnectOutput = "connected to 0.0.0.0:5555" }, new MemoryStore());

        var result = await sut.EnableFromUsbAsync("USBSERIAL");

        Assert.False(result.IsSuccess);
        Assert.Contains("Wi-Fi address", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Connect_retries_then_succeeds()
    {
        var adb = new FakeAdb
        {
            ConnectOutputs = ["failed to connect to 192.168.1.42:5555", "connected to 192.168.1.42:5555"]
        };
        var sut = Create(adb, new MemoryStore());

        var result = await sut.ConnectAsync(new WirelessEndpoint("192.168.1.42", 5555));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, adb.ConnectCalls);
    }

    [Fact]
    public async Task PairThenConnect_pairs_then_connects()
    {
        var adb = new FakeAdb
        {
            PairOutput = "Successfully paired to 192.168.1.42:37123 [guid=adb-x]",
            ConnectOutput = "connected to 192.168.1.42:5555"
        };
        var sut = Create(adb, new MemoryStore());

        var result = await sut.PairThenConnectAsync(
            new WirelessEndpoint("192.168.1.42", 37123),
            "123456",
            new WirelessEndpoint("192.168.1.42", 5555));

        Assert.True(result.IsSuccess);
        Assert.Equal("192.168.1.42:37123", adb.LastPairEndpoint);
        Assert.Equal("123456", adb.LastPairCode);
        Assert.Equal("192.168.1.42:5555", adb.LastConnect);
    }

    [Fact]
    public async Task PairThenConnect_fails_on_wrong_code()
    {
        var sut = Create(new FakeAdb { PairOutput = "Failed: Wrong password", ConnectOutput = "connected to 192.168.1.42:5555" }, new MemoryStore());

        var result = await sut.PairThenConnectAsync(
            new WirelessEndpoint("192.168.1.42", 37123),
            "000000",
            new WirelessEndpoint("192.168.1.42", 5555));

        Assert.False(result.IsSuccess);
        Assert.Contains("pair", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    private static WirelessAdbService Create(FakeAdb adb, MemoryStore store) =>
        new(adb, new AdbOutputParser(), store, new NoopLog(), (_, _) => Task.CompletedTask);

    private sealed class MemoryStore : IWirelessEndpointStore
    {
        public WirelessEndpoint? Endpoint { get; set; }
        public WirelessEndpoint? Load() => Endpoint;
        public void Save(WirelessEndpoint endpoint) => Endpoint = endpoint;
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? exception = null) { }
    }

    private sealed class FakeAdb : IAdbClient
    {
        public string? WifiAddress { get; set; }
        public string ConnectOutput { get; set; } = "";
        public List<string>? ConnectOutputs { get; set; }
        public string PairOutput { get; set; } = "";
        public string? TcpIpSerial { get; private set; }
        public string? LastConnect { get; private set; }
        public string? LastPairEndpoint { get; private set; }
        public string? LastPairCode { get; private set; }
        public int ConnectCalls { get; private set; }

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
        public Task<AdbProcessResult> TcpIpAsync(string serial, int port = 5555, CancellationToken cancellationToken = default)
        {
            TcpIpSerial = serial;
            return Task.FromResult(new AdbProcessResult(0, $"restarting in TCP mode port: {port}", "", TimeSpan.Zero, []));
        }

        public Task<AdbProcessResult> ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
        {
            LastConnect = endpoint;
            ConnectCalls++;
            var output = ConnectOutputs is { Count: > 0 }
                ? ConnectOutputs[Math.Min(ConnectCalls - 1, ConnectOutputs.Count - 1)]
                : ConnectOutput;
            return Task.FromResult(new AdbProcessResult(0, output, "", TimeSpan.Zero, []));
        }

        public Task<AdbProcessResult> DisconnectAsync(string? endpoint = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, []));

        public Task<AdbProcessResult> PairAsync(string endpoint, string pairingCode, CancellationToken cancellationToken = default)
        {
            LastPairEndpoint = endpoint;
            LastPairCode = pairingCode;
            return Task.FromResult(new AdbProcessResult(1, PairOutput, "", TimeSpan.Zero, []));
        }

        public Task<string?> GetWifiAddressAsync(string serial, CancellationToken cancellationToken = default) =>
            Task.FromResult(WifiAddress);
    }
}
