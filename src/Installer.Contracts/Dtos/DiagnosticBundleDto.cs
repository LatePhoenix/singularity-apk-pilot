namespace Installer.Contracts.Dtos;

public sealed class DiagnosticBundleDto
{
    public DateTimeOffset CreatedUtc { get; set; }
    public string InstallerVersion { get; set; } = "";
    public string AppId { get; set; } = "";
    public string? BuildVersion { get; set; }
    public string? DeviceKind { get; set; }
    public string? InstallError { get; set; }
    public DeviceSnapshotDto? Device { get; set; }
    public InstallAttemptDto? LastAttempt { get; set; }
}
