namespace Installer.Core.Models;

public sealed record DiagnosticBundleInfo(
    string ZipPath,
    DateTimeOffset CreatedUtc,
    string AppId,
    string InstallerVersion);
