using System.Text.RegularExpressions;
using Installer.Core.Models;

namespace Installer.Core.Services.Adb;

public sealed class AdbOutputParser
{
    private static readonly Regex DeviceLine = new(
        @"^(?<serial>\S+)\s+(?<state>device|unauthorized|offline|no permissions|unknown)(?:\s+(?<rest>.*))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex WifiAddressLine = new(
        @"^\d+:\s+(?<iface>\S+)\s+inet\s+(?<ip>\d{1,3}(?:\.\d{1,3}){3})/",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Multiline);

    private static readonly string[] SkipInterfaceTokens = ["lo", "usb", "rndis", "tether", "dummy"];

    public IReadOnlyList<AdbDeviceRecord> ParseDevices(string output)
    {
        var devices = new List<AdbDeviceRecord>();
        if (string.IsNullOrWhiteSpace(output))
        {
            return devices;
        }

        foreach (var rawLine in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase) || line.StartsWith("*", StringComparison.Ordinal))
            {
                continue;
            }

            var match = DeviceLine.Match(line);
            if (!match.Success)
            {
                continue;
            }

            devices.Add(new AdbDeviceRecord(
                match.Groups["serial"].Value,
                match.Groups["state"].Value,
                ParseProperties(match.Groups["rest"].Value)));
        }

        return devices;
    }

    public DeviceConnectionState ParseConnectionState(string adbState) =>
        adbState.ToLowerInvariant() switch
        {
            "device" => DeviceConnectionState.ConnectedReady,
            "unauthorized" => DeviceConnectionState.Unauthorized,
            "offline" => DeviceConnectionState.Offline,
            "no permissions" => DeviceConnectionState.Unauthorized,
            _ => DeviceConnectionState.Offline
        };

    public string? ParseWifiAddress(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        foreach (Match match in WifiAddressLine.Matches(output))
        {
            var iface = match.Groups["iface"].Value;
            var ip = match.Groups["ip"].Value;
            if (ip.StartsWith("127.", StringComparison.Ordinal) || ShouldSkipInterface(iface))
            {
                continue;
            }

            return ip;
        }

        return null;
    }

    public bool IsConnectSuccess(string output)
    {
        var lower = (output ?? "").ToLowerInvariant();
        return lower.Contains("connected to", StringComparison.Ordinal)
               || lower.Contains("already connected", StringComparison.Ordinal);
    }

    public bool IsPairSuccess(string output) =>
        (output ?? "").Contains("successfully paired", StringComparison.OrdinalIgnoreCase);

    public string? ParseLauncher(string output, string packageId)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        foreach (var raw in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Contains("No activity", StringComparison.OrdinalIgnoreCase) || !line.Contains('/'))
            {
                continue;
            }

            var token = line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Last();
            if (token.Contains(packageId, StringComparison.OrdinalIgnoreCase) || token.StartsWith('.'))
            {
                return token;
            }
        }

        return null;
    }

    public static string? ToComponent(string packageId, string? activity)
    {
        if (string.IsNullOrWhiteSpace(activity))
        {
            return null;
        }

        if (activity.Contains('/', StringComparison.Ordinal))
        {
            return activity;
        }

        return activity.StartsWith('.') ? packageId + "/" + activity : packageId + "/" + activity;
    }

    public bool IsPackageListed(string output, string packageId)
    {
        if (string.IsNullOrWhiteSpace(output) || string.IsNullOrWhiteSpace(packageId))
        {
            return false;
        }

        var needle = "package:" + packageId;
        return output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Any(line => line.Trim().Equals(needle, StringComparison.OrdinalIgnoreCase)
                         || line.Trim().Equals(packageId, StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyDictionary<string, string> ParseProperties(string rest)
    {
        var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(rest))
        {
            return properties;
        }

        foreach (var token in rest.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = token.IndexOf(':');
            if (separator <= 0 || separator == token.Length - 1)
            {
                continue;
            }

            properties[token[..separator]] = token[(separator + 1)..];
        }

        return properties;
    }

    private static bool ShouldSkipInterface(string iface)
    {
        foreach (var token in SkipInterfaceTokens)
        {
            if (iface.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
