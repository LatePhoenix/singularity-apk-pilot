namespace Installer.Core.Models;

public enum InstallPolicy
{
    InstallFresh = 0,
    ReinstallKeepData = 1,
    ReinstallAllowDowngrade = 2,
    UninstallThenInstall = 3,
    InstallTestBuild = 4
}
