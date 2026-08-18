using Installer.Core.Models;

namespace Installer.Core.Services.Recovery;

public sealed class ErrorClassifier
{
    public InstallError Classify(string? output)
    {
        var text = output ?? "";
        var lower = text.ToLowerInvariant();

        if (ContainsAny(lower, "failed to stat", "no such file", "cannot find the path", "the system cannot find"))
        {
            return InstallError.MissingPayload;
        }

        if (ContainsAny(lower, "unauthorized", "debugging is not allowed"))
        {
            return InstallError.UnauthorizedDevice;
        }

        if (ContainsAny(lower, "device offline", "offline"))
        {
            return InstallError.OfflineDevice;
        }

        if (ContainsAny(lower, "no devices/emulators found", "no device", "waiting for device"))
        {
            return lower.Contains("cable") || lower.Contains("usb")
                ? InstallError.CableOrUsbModeIssue
                : InstallError.NoDevicesFound;
        }

        if (ContainsAny(lower, "install_failed_version_downgrade", "version downgrade"))
        {
            return InstallError.VersionDowngrade;
        }

        if (ContainsAny(lower, "install_failed_already_exists"))
        {
            return InstallError.PackageAlreadyExists;
        }

        if (ContainsAny(lower, "install_failed_update_incompatible", "signatures do not match", "signature"))
        {
            return InstallError.SignatureMismatch;
        }

        if (ContainsAny(lower, "install_failed_insufficient_storage", "not enough storage", "no space"))
        {
            return InstallError.InsufficientStorage;
        }

        if (ContainsAny(lower, "developer mode", "mtp"))
        {
            return InstallError.DeveloperModeLikelyDisabled;
        }

        if (ContainsAny(lower, "install_failed_missing_split", "missing split", "missing_split"))
        {
            return InstallError.MissingSplit;
        }

        if (ContainsAny(lower, "delete_failed", "unknown package", "failure calling service package"))
        {
            return InstallError.UninstallFailed;
        }

        if (ContainsAny(lower, "failed to connect", "cannot connect to", "failed to pair", "wrong password", "connection refused"))
        {
            return InstallError.WirelessConnectFailed;
        }

        if (ContainsAny(lower, "usb", "not found", "device disconnected", "closed"))
        {
            return InstallError.CableOrUsbModeIssue;
        }

        return InstallError.UnknownInstallFailure;
    }

    private static bool ContainsAny(string text, params string[] needles) =>
        needles.Any(needle => text.Contains(needle, StringComparison.Ordinal));
}
