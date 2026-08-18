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

    public IReadOnlyList<string> ParsePackageList(string output)
    {
        var ids = new List<string>();
        if (string.IsNullOrWhiteSpace(output))
        {
            return ids;
        }

        foreach (var raw in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.StartsWith("package:", StringComparison.OrdinalIgnoreCase))
            {
                var id = line["package:".Length..].Trim();
                if (IsSafePackageId(id))
                {
                    ids.Add(id);
                }
            }
        }

        return ids;
    }

    public bool IsUninstallSuccess(string output)
    {
        var text = output ?? "";
        if (text.Contains("Failure", StringComparison.OrdinalIgnoreCase)
            || text.Contains("DELETE_FAILED", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Unknown package", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return text.Contains("Success", StringComparison.OrdinalIgnoreCase);
    }

    public (string? Label, string? Version) ParsePackageDump(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return (null, null);
        }

        string? label = null;
        string? version = null;
        foreach (var raw in output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            label ??= ReadTaggedValue(line, "applicationLabel=");
            label ??= ReadTaggedValue(line, "nonLocalizedLabel=");
            label ??= ReadTaggedValue(line, "application-label:");
            version ??= ReadTaggedValue(line, "versionName=");
            if (label is not null && version is not null)
            {
                break;
            }
        }

        return (CleanDumpValue(label), CleanDumpValue(version));
    }

    public static bool IsSafePackageId(string? packageId)
    {
        if (string.IsNullOrWhiteSpace(packageId) || packageId.Length > 256)
        {
            return false;
        }

        foreach (var c in packageId)
        {
            if (!char.IsLetterOrDigit(c) && c is not '.' and not '_')
            {
                return false;
            }
        }

        return packageId.Contains('.', StringComparison.Ordinal);
    }

    private static string? ReadTaggedValue(string line, string tag)
    {
        var index = line.IndexOf(tag, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return null;
        }

        var value = line[(index + tag.Length)..].Trim().Trim('\'', '"');
        var space = value.IndexOf(' ');
        if (space > 0 && !tag.Contains("Label", StringComparison.OrdinalIgnoreCase) && !tag.Contains("label", StringComparison.Ordinal))
        {
            value = value[..space];
        }

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string? CleanDumpValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value is "null" or "0")
        {
            return null;
        }

        return value.Trim();
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
