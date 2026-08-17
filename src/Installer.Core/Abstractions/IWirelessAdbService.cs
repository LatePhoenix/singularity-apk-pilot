using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Abstractions;

public interface IWirelessAdbService
{
    WirelessEndpoint? LastEndpoint { get; }
    Task<Result<WirelessEndpoint>> EnableFromUsbAsync(string serial, CancellationToken cancellationToken = default);
    Task<Result<WirelessEndpoint>> ConnectAsync(WirelessEndpoint endpoint, CancellationToken cancellationToken = default);
    Task<Result<WirelessEndpoint>> PairThenConnectAsync(
        WirelessEndpoint pairing,
        string pairingCode,
        WirelessEndpoint connect,
        CancellationToken cancellationToken = default);
}
