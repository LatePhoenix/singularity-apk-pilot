using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Diagnostics;

public sealed class LogcatCollector
{
    private readonly IAdbClient _adb;

    public LogcatCollector(IAdbClient adb)
    {
        _adb = adb;
    }

    public async Task<string> CollectAsync(DeviceInfo? device, string packageId, CancellationToken cancellationToken = default)
    {
        if (device is null || !device.IsAuthorized || string.IsNullOrWhiteSpace(device.Serial))
        {
            return "";
        }

        try
        {
            return await _adb.GetLogcatAsync(device.Serial, packageId, cancellationToken);
        }
        catch
        {
            return "";
        }
    }
}
