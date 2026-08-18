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
    [InlineData("Failure [INSTALL_PARSE_FAILED_NO_CERTIFICATES]", InstallError.UnknownInstallFailure)]
    [InlineData("adb.exe: failed to stat C:\\payloads\\current\\example-app.apk: No such file or directory", InstallError.MissingPayload)]
    [InlineData("failed to connect to 192.168.1.42:5555", InstallError.WirelessConnectFailed)]
    [InlineData("Failed: Wrong password", InstallError.WirelessConnectFailed)]
    [InlineData("Failure [INSTALL_FAILED_MISSING_SPLIT: Missing split for com.demo]", InstallError.MissingSplit)]
    [InlineData("Failure [DELETE_FAILED_INTERNAL_ERROR]", InstallError.UninstallFailed)]
    public void Classifies_sample_output(string output, InstallError expected)
    {
        Assert.Equal(expected, _sut.Classify(output));
    }
}
