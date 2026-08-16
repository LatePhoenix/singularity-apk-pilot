namespace Installer.Core.Models;

public enum WizardStep
{
    Welcome = 0,
    ConnectDevice = 1,
    DeviceDetected = 2,
    Authorization = 3,
    DeveloperMode = 4,
    ReadyToInstall = 5,
    Installing = 6,
    InstallProblem = 7,
    Complete = 8
}
