using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Devices;

public sealed class DevicePropertyService
{
    public const string ManufacturerKey = "ro.product.manufacturer";
    public const string ModelKey = "ro.product.model";
    public const string ReleaseKey = "ro.build.version.release";

    private readonly IAdbClient _adb;

    public DevicePropertyService(IAdbClient adb)
    {
        _adb = adb;
    }

    public async Task<IReadOnlyDictionary<string, string>> ReadAsync(string serial, CancellationToken cancellationToken = default)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in new[] { ManufacturerKey, ModelKey, ReleaseKey })
        {
            try
            {
                values[key] = await _adb.GetPropertyAsync(serial, key, cancellationToken);
            }
            catch (Exception)
            {
                values[key] = "";
            }
        }

        return values;
    }
}
