namespace Installer.Core.Models;

public sealed record DeviceInfo(
    string Serial,
    string Manufacturer,
    string Model,
    string AndroidVersion,
    DeviceKind Kind,
    DeviceConnectionState State,
    bool IsAuthorized,
    bool IsQuest,
    IReadOnlyDictionary<string, string> Properties)
{
    public DeviceTransport Transport =>
        WirelessEndpoint.IsWifiSerial(Serial) ? DeviceTransport.Wifi : DeviceTransport.Usb;

    public bool IsWireless => Transport == DeviceTransport.Wifi;

    public string ConnectionLabel => IsWireless ? "Wi-Fi" : "USB";

    public string PickerLabel => $"{DisplayName} · {ConnectionLabel}";

    public static DeviceInfo None { get; } = new(
        Serial: "",
        Manufacturer: "",
        Model: "",
        AndroidVersion: "",
        Kind: DeviceKind.Unknown,
        State: DeviceConnectionState.NotConnected,
        IsAuthorized: false,
        IsQuest: false,
        Properties: new Dictionary<string, string>());

    public string DisplayName =>
        string.IsNullOrWhiteSpace(Model) ? (Kind == DeviceKind.MetaQuest ? "Meta Quest" : "Android device") : Model.Replace('_', ' ');
}
