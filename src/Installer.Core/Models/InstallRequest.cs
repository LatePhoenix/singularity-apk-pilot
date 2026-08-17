namespace Installer.Core.Models;

public sealed record InstallRequest(
    InstallManifest Manifest,
    DeviceInfo Device,
    InstallPolicy? PolicyOverride = null,
    InstallSet? Set = null);
