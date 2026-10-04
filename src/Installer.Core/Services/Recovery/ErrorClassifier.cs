using System.Text.RegularExpressions;
using Installer.Core.Models;

namespace Installer.Core.Services.Recovery;

public sealed class ErrorClassifier
{
    // A Windows path in adb output ends at the next colon ("failed to install C:\x\y.apk: Failure [...]").
    // Folder and file names must not steer the result, e.g. a "usb-builds" or "signature" folder.
    private static readonly Regex LocalPath = new(
        @"(?:[A-Za-z]:|\\\\[^\\\r\n]+|%[A-Za-z_]+%)\\[^\r\n:""*?<>|]*",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public InstallError Classify(string? output)
    {
        var text = LocalPath.Replace(output ?? "", "<path>");
        var lower = text.ToLowerInvariant();

        if (ContainsAny(lower, "failed to stat", "no such file", "cannot find the path", "the system cannot find"))
        {
            return InstallError.MissingPayload;
        }

        // Android install codes are exact, so check them before looser connection words.
        if (ContainsAny(lower, "install_failed_version_downgrade", "version downgrade"))
        {
            return InstallError.VersionDowngrade;
        }

        if (ContainsAny(lower, "install_failed_already_exists"))
        {
            return InstallError.PackageAlreadyExists;
        }

        if (ContainsAny(lower, "install_failed_update_incompatible", "install_failed_inconsistent_certificates", "signatures do not match"))
        {
            return InstallError.SignatureMismatch;
        }

        if (ContainsAny(lower, "install_failed_insufficient_storage", "not enough storage", "no space"))
        {
            return InstallError.InsufficientStorage;
        }

        if (ContainsAny(lower, "install_failed_missing_split", "missing split", "missing_split"))
        {
            return InstallError.MissingSplit;
        }

        if (ContainsAny(lower, "install_failed_no_matching_abis", "no_matching_abis"))
        {
            return InstallError.IncompatibleAbi;
        }

        if (ContainsAny(lower, "install_failed_deprecated_sdk_version"))
        {
            return InstallError.AppTargetsOldAndroid;
        }

        if (ContainsAny(lower, "install_failed_older_sdk"))
        {
            return InstallError.DeviceAndroidTooOld;
        }

        if (ContainsAny(lower, "install_parse_failed", "install_failed_invalid_apk", "install_failed_bad_signature"))
        {
            return InstallError.InvalidApk;
        }

        if (ContainsAny(lower, "install_failed_user_restricted", "install_failed_verification_failure", "install_failed_aborted"))
        {
            return InstallError.InstallBlockedOnDevice;
        }

        if (ContainsAny(lower, "delete_failed", "unknown package", "failure calling service package"))
        {
            return InstallError.UninstallFailed;
        }

        // The local adb helper itself broke (killed, version clash with another Android tool, or hung).
        if (ContainsAny(lower,
                "cannot connect to daemon",
                "failed to start daemon",
                "could not read ok from adb server",
                "adb server version",
                "adb server didn't ack",
                "protocol fault",
                "adb timed out"))
        {
            return InstallError.ConnectionHelperFailed;
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

        if (ContainsAny(lower, "developer mode", "mtp"))
        {
            return InstallError.DeveloperModeLikelyDisabled;
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
