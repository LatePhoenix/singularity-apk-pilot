namespace Installer.Core.Models;

public enum DeviceConnectionState
{
    NotConnected = 0,
    Unauthorized = 1,
    Offline = 2,
    ConnectedReady = 3,
    BusyInstalling = 4,
    InstallFailed = 5,
    Installed = 6
}
