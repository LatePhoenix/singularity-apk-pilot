using System.Net;
using System.Net.Sockets;

namespace Installer.Core.Models;

public sealed record WirelessEndpoint(string Host, int Port)
{
    public const int DefaultPort = 5555;

    public string Address => $"{Host}:{Port}";

    public static bool TryParse(string? value, out WirelessEndpoint endpoint)
    {
        endpoint = null!;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        var colon = trimmed.LastIndexOf(':');
        string host;
        var port = DefaultPort;
        if (colon > 0
            && colon < trimmed.Length - 1
            && int.TryParse(trimmed[(colon + 1)..], out var parsed)
            && parsed is > 0 and <= 65535)
        {
            host = trimmed[..colon];
            port = parsed;
        }
        else
        {
            host = trimmed;
        }

        if (!IPAddress.TryParse(host, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork)
        {
            return false;
        }

        endpoint = new WirelessEndpoint(host, port);
        return true;
    }

    public static bool IsWifiSerial(string? serial) => TryParse(serial, out _);
}
