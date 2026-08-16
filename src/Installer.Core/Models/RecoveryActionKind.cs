namespace Installer.Core.Models;

public enum RecoveryActionKind
{
    RestartAdbServer = 0,
    RetryDetection = 1,
    RetryInstall = 2,
    RetryWithDowngrade = 3,
    UninstallThenInstall = 4,
    ShowAuthorization = 5,
    ShowDeveloperMode = 6,
    ShowCableHelp = 7,
    ExportDiagnostics = 8
}
