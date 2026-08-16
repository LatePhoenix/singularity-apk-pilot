namespace Installer.Contracts.Dtos;

public sealed class DeviceSnapshotDto
{
    public string SerialHash { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string AndroidVersion { get; set; } = "";
    public string Kind { get; set; } = "";
    public string ConnectionState { get; set; } = "";
    public bool IsAuthorized { get; set; }
    public bool IsQuest { get; set; }
}
