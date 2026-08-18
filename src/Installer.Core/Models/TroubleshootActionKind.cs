namespace Installer.Core.Models;

public enum TroubleshootActionKind
{
    None = 0,
    RestartAdbServer = 1,
    InstallUsbHelper = 2,
    OpenDriverDownload = 3,
    OpenPhoneUsbSupport = 4,
    ExportDiagnostics = 5
}
