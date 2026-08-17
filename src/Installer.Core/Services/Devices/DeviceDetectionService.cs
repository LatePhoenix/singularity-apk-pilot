using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;

namespace Installer.Core.Services.Devices;

public sealed class DeviceDetectionService : IDeviceService
{
    private readonly IAdbClient _adb;
    private readonly AdbOutputParser _parser;
    private readonly DevicePropertyService _properties;
    private readonly DeviceClassificationService _classification;
    private readonly IAppLogger _logger;

    public DeviceDetectionService(
        IAdbClient adb,
        AdbOutputParser parser,
        DevicePropertyService properties,
        DeviceClassificationService classification,
        IAppLogger logger)
    {
        _adb = adb;
        _parser = parser;
        _properties = properties;
        _classification = classification;
        _logger = logger;
    }

    public async Task<IReadOnlyList<DeviceInfo>> DetectAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<AdbDeviceRecord> records;
        try
        {
            await _adb.StartServerAsync(cancellationToken);
            records = await _adb.ListDevicesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.Error("Device list failed.", ex);
            return [];
        }

        var devices = new List<DeviceInfo>(records.Count);
        foreach (var record in records)
        {
            devices.Add(await ToDeviceInfoAsync(record, cancellationToken));
        }

        return devices;
    }

    public DeviceInfo? SelectPrimary(IReadOnlyList<DeviceInfo> devices)
    {
        if (devices.Count == 0)
        {
            return null;
        }

        return devices.FirstOrDefault(d => d.Kind == DeviceKind.MetaQuest && d.State == DeviceConnectionState.ConnectedReady && d.IsWireless)
               ?? devices.FirstOrDefault(d => d.Kind == DeviceKind.MetaQuest && d.State == DeviceConnectionState.ConnectedReady)
               ?? devices.FirstOrDefault(d => d.State == DeviceConnectionState.ConnectedReady && d.IsWireless)
               ?? devices.FirstOrDefault(d => d.State == DeviceConnectionState.ConnectedReady)
               ?? devices.FirstOrDefault(d => d.State == DeviceConnectionState.Unauthorized)
               ?? devices[0];
    }

    private async Task<DeviceInfo> ToDeviceInfoAsync(AdbDeviceRecord record, CancellationToken cancellationToken)
    {
        var state = _parser.ParseConnectionState(record.State);
        var isAuthorized = state == DeviceConnectionState.ConnectedReady;
        IReadOnlyDictionary<string, string> props = new Dictionary<string, string>(record.Properties, StringComparer.OrdinalIgnoreCase);

        if (isAuthorized)
        {
            var live = await _properties.ReadAsync(record.Serial, cancellationToken);
            var merged = new Dictionary<string, string>(props, StringComparer.OrdinalIgnoreCase);
            foreach (var pair in live)
            {
                merged[pair.Key] = pair.Value;
            }

            props = merged;
        }

        props.TryGetValue(DevicePropertyService.ManufacturerKey, out var manufacturer);
        props.TryGetValue(DevicePropertyService.ModelKey, out var model);
        props.TryGetValue(DevicePropertyService.ReleaseKey, out var release);
        manufacturer ??= "";
        model ??= record.Properties.TryGetValue("model", out var listedModel) ? listedModel : "";
        release ??= "";

        var kind = _classification.Classify(manufacturer, model, record.Properties);
        var friendly = _classification.FriendlyModel(kind, model, record.Properties);

        return new DeviceInfo(
            record.Serial,
            manufacturer,
            friendly,
            release,
            kind,
            state,
            isAuthorized,
            kind == DeviceKind.MetaQuest,
            props);
    }
}
