using System.Text.RegularExpressions;
using Installer.Core.Models;

namespace Installer.Core.Services.Adb;

public sealed class AdbOutputParser
{
    private static readonly Regex DeviceLine = new(
        @"^(?<serial>\S+)\s+(?<state>device|unauthorized|offline|no permissions|unknown)(?:\s+(?<rest>.*))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
}
