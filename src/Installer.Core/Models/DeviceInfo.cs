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
