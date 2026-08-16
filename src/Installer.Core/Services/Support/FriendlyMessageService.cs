using Installer.Core.Models;

namespace Installer.Core.Services.Support;

public sealed class FriendlyMessageService
{
    public string TitleFor(InstallError error) => error switch
    {
        InstallError.UnauthorizedDevice or InstallError.DebuggingNotApproved => "Your device has not approved this computer yet",
        InstallError.OfflineDevice => "The device disconnected",
        InstallError.NoDevicesFound => "No device was found",
        InstallError.VersionDowngrade => "An older incompatible version is already installed",
        InstallError.PackageAlreadyExists => "This app is already installed",
        InstallError.SignatureMismatch => "A different copy of this app is already installed",
        InstallError.InsufficientStorage => "The device does not have enough free space",
        InstallError.DeveloperModeLikelyDisabled => "Developer mode may still be off",
        InstallError.CableOrUsbModeIssue => "The cable or USB mode looks wrong",
        _ => "The install did not complete"
    };

    public string CauseFor(InstallError error) => error switch
    {
        InstallError.UnauthorizedDevice or InstallError.DebuggingNotApproved => "The permission prompt on the device was not accepted.",
        InstallError.OfflineDevice => "The cable, sleep state, or USB mode interrupted the connection.",
        InstallError.NoDevicesFound => "The computer does not see a headset or phone yet.",
        InstallError.VersionDowngrade => "A newer build is already on the device.",
        InstallError.PackageAlreadyExists => "The app is already present and needs a replace install.",
        InstallError.SignatureMismatch => "The existing app was signed differently from this test build.",
        InstallError.InsufficientStorage => "The headset or phone storage is full.",
        InstallError.DeveloperModeLikelyDisabled => "Quest developer mode is required before this computer can install apps.",
        InstallError.CableOrUsbModeIssue => "A charge-only cable or the wrong USB mode is likely.",
        _ => "See advanced details or export diagnostics."
    };
}
