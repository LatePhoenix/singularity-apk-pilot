using Installer.Core.Models;

namespace Installer.App.Services;

public interface ISendReportUi
{
    SendReportUiResult Show(InstallManifest manifest, DeviceInfo? device, InstallResult? lastResult);
}

public sealed record SendReportUiResult(string Status);
