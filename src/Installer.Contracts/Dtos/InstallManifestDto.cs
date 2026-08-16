namespace Installer.Contracts.Dtos;

public sealed class InstallManifestDto
{
    public int SchemaVersion { get; set; } = 1;
    public string AppId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string BuildVersion { get; set; } = "";
    public string ApkPath { get; set; } = "";
    public List<string> TargetPlatforms { get; set; } = [];
    public string InstallPolicy { get; set; } = "";
    public bool GrantPermissions { get; set; }
    public bool AllowTestApk { get; set; }
    public bool LaunchAfterInstall { get; set; }
    public List<string> PreferredDeviceFamilies { get; set; } = [];
    public Dictionary<string, List<string>> PostInstallNotes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public SupportContactDto? Support { get; set; }
}
