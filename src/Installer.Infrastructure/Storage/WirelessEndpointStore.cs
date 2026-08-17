using System.Text.Json;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Infrastructure.Storage;

public sealed class WirelessEndpointStore : IWirelessEndpointStore
{
    private readonly IUserDataPaths _paths;
    private readonly IAppLogger _logger;

    public WirelessEndpointStore(IUserDataPaths paths, IAppLogger logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public WirelessEndpoint? Load()
    {
        try
        {
            var path = _paths.WirelessEndpointPath;
            if (!File.Exists(path))
            {
                return null;
            }

            var dto = JsonSerializer.Deserialize<StoredEndpoint>(File.ReadAllText(path), JsonDefaults.Manifest);
            if (dto is null || string.IsNullOrWhiteSpace(dto.Host))
            {
                return null;
            }

            var port = dto.Port is > 0 and <= 65535 ? dto.Port : WirelessEndpoint.DefaultPort;
            return new WirelessEndpoint(dto.Host, port);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not load saved Wi-Fi address: {ex.Message}");
            return null;
        }
    }

    public void Save(WirelessEndpoint endpoint)
    {
        try
        {
            var path = _paths.WirelessEndpointPath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(new StoredEndpoint(endpoint.Host, endpoint.Port), JsonDefaults.Manifest);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not save Wi-Fi address: {ex.Message}");
        }
    }

    private sealed record StoredEndpoint(string Host, int Port);
}
