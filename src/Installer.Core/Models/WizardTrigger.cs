namespace Installer.Core.Models;

public enum WizardTrigger
{
    Start = 0,
    DeviceRefresh = 1,
    Continue = 2,
    ConfirmAuthorization = 3,
    ConfirmDeveloperMode = 4,
    Install = 5,
    InstallFinished = 6,
    Cancel = 7,
    AutoFix = 8,
    Retry = 9,
    Done = 10,
    OpenInstalledApps = 11,
    CloseInstalledApps = 12,
    OpenTroubleshoot = 13,
    CloseTroubleshoot = 14
}
