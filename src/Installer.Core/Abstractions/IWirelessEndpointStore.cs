using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IWirelessEndpointStore
{
    WirelessEndpoint? Load();
    void Save(WirelessEndpoint endpoint);
}
