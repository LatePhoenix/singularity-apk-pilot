namespace Installer.Core.Models;

public enum InstallError
{
    UnauthorizedDevice = 0,
    OfflineDevice = 1,
    NoDevicesFound = 2,
    VersionDowngrade = 3,
    PackageAlreadyExists = 4,
    SignatureMismatch = 5,
    InsufficientStorage = 6,
    DebuggingNotApproved = 7,
    DeveloperModeLikelyDisabled = 8,
    CableOrUsbModeIssue = 9,
    UnknownInstallFailure = 10,
    MissingPayload = 11,
    WirelessConnectFailed = 12,
    MissingSplit = 13,
    UninstallFailed = 14
}
