namespace Installer.Contracts.Dtos;

public sealed class InstallAttemptDto
{
    public string PackageId { get; set; } = "";
    public string ApkPath { get; set; } = "";
    public string Policy { get; set; } = "";
    public List<string> AdbFlags { get; set; } = [];
    public bool RequiresUninstallFirst { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public int ExitCode { get; set; }
    public DateTimeOffset StartedUtc { get; set; }
    public DateTimeOffset EndedUtc { get; set; }
}
