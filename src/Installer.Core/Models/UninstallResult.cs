namespace Installer.Core.Models;

public sealed record UninstallResult(
    bool Success,
    string PackageId,
    string Message,
    InstallError? Error = null,
    string? RawOutput = null)
{
    public static UninstallResult Ok(string packageId) =>
        new(true, packageId, "Removed.");

    public static UninstallResult Failed(string packageId, string message, string? raw = null, InstallError error = InstallError.UninstallFailed) =>
        new(false, packageId, message, error, raw);
}
