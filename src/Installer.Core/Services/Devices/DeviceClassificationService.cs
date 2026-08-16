using Installer.Core.Models;

namespace Installer.Core.Services.Devices;

public sealed class DeviceClassificationService
{
    public DeviceKind Classify(string? manufacturer, string? model, IReadOnlyDictionary<string, string>? deviceListProperties = null)
    {
        var product = Get(deviceListProperties, "product");
        var listedModel = Get(deviceListProperties, "model");
        var blob = $"{manufacturer} {model} {product} {listedModel}".ToLowerInvariant();

        if (blob.Contains("quest") || blob.Contains("oculus") || IsQuestProduct(product) || blob.Contains("hollywood") || blob.Contains("eureka") || blob.Contains("seacliff") || blob.Contains("panther"))
        {
            return DeviceKind.MetaQuest;
        }

        if (!string.IsNullOrWhiteSpace(manufacturer) || !string.IsNullOrWhiteSpace(model) || !string.IsNullOrWhiteSpace(listedModel))
        {
            return DeviceKind.AndroidPhone;
        }

        return DeviceKind.Unknown;
    }

    public string FriendlyModel(DeviceKind kind, string? model, IReadOnlyDictionary<string, string>? deviceListProperties = null)
    {
        var raw = string.IsNullOrWhiteSpace(model) ? Get(deviceListProperties, "model") : model;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return kind == DeviceKind.MetaQuest ? "Meta Quest" : "Android device";
        }

        return raw.Replace('_', ' ');
    }

    private static bool IsQuestProduct(string product) =>
        product.Equals("hollywood", StringComparison.OrdinalIgnoreCase)
        || product.Equals("eureka", StringComparison.OrdinalIgnoreCase)
        || product.Equals("seacliff", StringComparison.OrdinalIgnoreCase)
        || product.Equals("panther", StringComparison.OrdinalIgnoreCase)
        || product.Contains("quest", StringComparison.OrdinalIgnoreCase);

    private static string Get(IReadOnlyDictionary<string, string>? properties, string key) =>
        properties is not null && properties.TryGetValue(key, out var value) ? value : "";
}
