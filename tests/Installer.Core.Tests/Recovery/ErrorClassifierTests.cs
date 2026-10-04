using Installer.Core.Models;
using Installer.Core.Services.Recovery;

namespace Installer.Core.Tests.Recovery;

public sealed class ErrorClassifierTests
{
    private readonly ErrorClassifier _sut = new();

    [Theory]
    [InlineData("error: device unauthorized", InstallError.UnauthorizedDevice)]
    [InlineData("error: device offline", InstallError.OfflineDevice)]
    [InlineData("error: no devices/emulators found", InstallError.NoDevicesFound)]
    [InlineData("Failure [INSTALL_FAILED_VERSION_DOWNGRADE]", InstallError.VersionDowngrade)]
    [InlineData("Failure [INSTALL_FAILED_ALREADY_EXISTS]", InstallError.PackageAlreadyExists)]
    [InlineData("Failure [INSTALL_FAILED_UPDATE_INCOMPATIBLE: signatures do not match]", InstallError.SignatureMismatch)]
    [InlineData("Failure [INSTALL_FAILED_INSUFFICIENT_STORAGE]", InstallError.InsufficientStorage)]
    [InlineData("developer mode is required", InstallError.DeveloperModeLikelyDisabled)]
    [InlineData("device disconnected / usb closed", InstallError.CableOrUsbModeIssue)]
    [InlineData("Failure [INSTALL_PARSE_FAILED_NO_CERTIFICATES]", InstallError.InvalidApk)]
    [InlineData("adb.exe: failed to stat C:\\payloads\\current\\example-app.apk: No such file or directory", InstallError.MissingPayload)]
    [InlineData("failed to connect to 192.168.1.42:5555", InstallError.WirelessConnectFailed)]
    [InlineData("Failed: Wrong password", InstallError.WirelessConnectFailed)]
    [InlineData("Failure [INSTALL_FAILED_MISSING_SPLIT: Missing split for com.demo]", InstallError.MissingSplit)]
    [InlineData("Failure [DELETE_FAILED_INTERNAL_ERROR]", InstallError.UninstallFailed)]
    public void Classifies_sample_output(string output, InstallError expected)
    {
        Assert.Equal(expected, _sut.Classify(output));
    }

    [Theory]
    [InlineData(
        "Performing Streamed Install\nadb.exe: failed to install C:\\Users\\tester\\Downloads\\VRCQ.apk: Failure [INSTALL_FAILED_NO_MATCHING_ABIS: INSTALL_FAILED_NO_MATCHING_ABIS: Failed to extract native libraries, res=-113]",
        InstallError.IncompatibleAbi)]
    [InlineData("Failure [INSTALL_FAILED_DEPRECATED_SDK_VERSION: App package must target at least SDK version 23]", InstallError.AppTargetsOldAndroid)]
    [InlineData("Failure [INSTALL_FAILED_OLDER_SDK: Requires newer sdk version #34 (current version is #29)]", InstallError.DeviceAndroidTooOld)]
    [InlineData("Failure [INSTALL_FAILED_INVALID_APK: Split lib_slice was defined multiple times]", InstallError.InvalidApk)]
    [InlineData("Failure [INSTALL_FAILED_USER_RESTRICTED: Install canceled by user]", InstallError.InstallBlockedOnDevice)]
    [InlineData("Failure [INSTALL_FAILED_INCONSISTENT_CERTIFICATES]", InstallError.SignatureMismatch)]
    [InlineData(
        "* daemon not running; starting now at tcp:5037\ncould not read ok from ADB Server\n* failed to start daemon\nadb.exe: cannot connect to daemon",
        InstallError.ConnectionHelperFailed)]
    [InlineData("error: protocol fault (couldn't read status): connection reset", InstallError.ConnectionHelperFailed)]
    [InlineData("Performing Streamed Install\nadb timed out after 1800 seconds.", InstallError.ConnectionHelperFailed)]
    public void Classifies_device_and_helper_failures(string output, InstallError expected)
    {
        Assert.Equal(expected, _sut.Classify(output));
    }

    [Theory]
    [InlineData(@"adb.exe: failed to install C:\Users\tester\usb-builds\app.apk: Failure [INSTALL_FAILED_VERSION_DOWNGRADE]", InstallError.VersionDowngrade)]
    [InlineData(@"adb.exe: failed to install C:\signature\offline\app.apk: Failure [INSTALL_FAILED_INSUFFICIENT_STORAGE]", InstallError.InsufficientStorage)]
    [InlineData(@"adb.exe: failed to install D:\mtp closed\app.apk: Failure [weird]", InstallError.UnknownInstallFailure)]
    [InlineData(@"adb.exe: failed to install %USERPROFILE%\Downloads\usb.apk: Failure [weird]", InstallError.UnknownInstallFailure)]
    [InlineData(@"adb.exe: failed to install \\nas\share\usb\app.apk: Failure [weird]", InstallError.UnknownInstallFailure)]
    public void Ignores_words_inside_local_file_paths(string output, InstallError expected)
    {
        Assert.Equal(expected, _sut.Classify(output));
    }
}
