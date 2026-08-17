using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Services.Adb;

public sealed class WirelessAdbService : IWirelessAdbService
{
    private static readonly TimeSpan AfterTcpIpDelay = TimeSpan.FromMilliseconds(750);
    private const int ConnectAttempts = 3;

    private readonly IAdbClient _adb;
    private readonly AdbOutputParser _parser;
    private readonly IWirelessEndpointStore _store;
    private readonly IAppLogger _logger;
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public WirelessAdbService(
        IAdbClient adb,
        AdbOutputParser parser,
        IWirelessEndpointStore store,
        IAppLogger logger,
        Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _adb = adb;
        _parser = parser;
        _store = store;
        _logger = logger;
        _delay = delay ?? ((time, token) => Task.Delay(time, token));
        LastEndpoint = _store.Load();
    }

    public WirelessEndpoint? LastEndpoint { get; private set; }

    public async Task<Result<WirelessEndpoint>> EnableFromUsbAsync(string serial, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        var ip = await _adb.GetWifiAddressAsync(serial, cancellationToken);
        var tcpip = await _adb.TcpIpAsync(serial, WirelessEndpoint.DefaultPort, cancellationToken);
        if (!tcpip.Succeeded && string.IsNullOrWhiteSpace(ip))
        {
            _logger.Warn($"tcpip failed: {tcpip.CombinedOutput}");
            return Result<WirelessEndpoint>.Failure(
                "Could not switch this device to Wi-Fi. Keep the cable connected and try again.");
        }

        await _delay(AfterTcpIpDelay, cancellationToken);
        ip ??= await _adb.GetWifiAddressAsync(serial, cancellationToken);
        if (string.IsNullOrWhiteSpace(ip))
        {
            return Result<WirelessEndpoint>.Failure(
                "The device did not report a Wi-Fi address. Put it on the same network as this computer and try again.");
        }

        return await ConnectWithRetryAsync(new WirelessEndpoint(ip, WirelessEndpoint.DefaultPort), cancellationToken);
    }

    public Task<Result<WirelessEndpoint>> ConnectAsync(WirelessEndpoint endpoint, CancellationToken cancellationToken = default) =>
        ConnectWithRetryAsync(endpoint, cancellationToken);

    public async Task<Result<WirelessEndpoint>> PairThenConnectAsync(
        WirelessEndpoint pairing,
        string pairingCode,
        WirelessEndpoint connect,
        CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(pairingCode, nameof(pairingCode));
        var pair = await _adb.PairAsync(pairing.Address, pairingCode.Trim(), cancellationToken);
        if (!_parser.IsPairSuccess(pair.CombinedOutput))
        {
            _logger.Warn($"pair failed: {pair.CombinedOutput}");
            return Result<WirelessEndpoint>.Failure(
                "Could not pair over Wi-Fi. Check the pairing code and pairing port, then try again.");
        }

        await _delay(AfterTcpIpDelay, cancellationToken);
        return await ConnectWithRetryAsync(connect, cancellationToken);
    }

    private async Task<Result<WirelessEndpoint>> ConnectWithRetryAsync(WirelessEndpoint endpoint, CancellationToken cancellationToken)
    {
        AdbProcessResult? last = null;
        for (var attempt = 0; attempt < ConnectAttempts; attempt++)
        {
            if (attempt > 0)
            {
                await _delay(AfterTcpIpDelay, cancellationToken);
            }

            last = await _adb.ConnectAsync(endpoint.Address, cancellationToken);
            if (_parser.IsConnectSuccess(last.CombinedOutput))
            {
                Persist(endpoint);
                return Result<WirelessEndpoint>.Success(endpoint);
            }
        }

        _logger.Warn($"connect failed: {last?.CombinedOutput}");
        return Result<WirelessEndpoint>.Failure(
            "Could not connect over Wi-Fi. Use a USB cable, or confirm the device is on the same network.");
    }

    private void Persist(WirelessEndpoint endpoint)
    {
        LastEndpoint = endpoint;
        _store.Save(endpoint);
    }
}
