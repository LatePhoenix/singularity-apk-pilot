namespace Installer.Core.Models;

public sealed record AdbDeviceRecord(
    string Serial,
    string State,
    IReadOnlyDictionary<string, string> Properties);
