using System.Globalization;
using Installer.Core.Models;

namespace Installer.Core.Services.Adb;

public sealed class AdbCommandFactory
{
    public AdbCommand StartServer() => new(["start-server"], "Start connection helper");

    public AdbCommand KillServer() => new(["kill-server"], "Restart connection helper");

    public AdbCommand Devices() => new(["devices", "-l"], "List devices");

    public AdbCommand TcpIp(string serial, int port) =>
        new(["-s", serial, "tcpip", port.ToString(CultureInfo.InvariantCulture)], "Enable Wi-Fi connection");

    public AdbCommand Connect(string endpoint) =>
        new(["connect", endpoint], "Connect over Wi-Fi");

    public AdbCommand Disconnect(string? endpoint = null)
    {
        var args = new List<string> { "disconnect" };
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            args.Add(endpoint);
        }

        return new AdbCommand(args, "Disconnect Wi-Fi");
    }

    public AdbCommand Pair(string endpoint, string pairingCode) =>
        new(["pair", endpoint, pairingCode], "Pair over Wi-Fi");

    public AdbCommand WifiAddresses(string serial) =>
        new(["-s", serial, "shell", "ip", "-o", "-4", "addr", "show", "scope", "global"], "Read Wi-Fi address");

    public AdbCommand GetProperty(string serial, string key) =>
        new(["-s", serial, "shell", "getprop", key], $"Read device property {key}");

    public AdbCommand Install(string serial, string apkPath, IReadOnlyList<string> flags)
    {
        var args = new List<string> { "-s", serial, "install" };
        args.AddRange(flags.Where(flag => !string.IsNullOrWhiteSpace(flag)));
        args.Add(apkPath);
        return new AdbCommand(args, "Install app");
    }

    public AdbCommand InstallMultiple(string serial, IReadOnlyList<string> apkPaths, IReadOnlyList<string> flags)
    {
        var args = new List<string> { "-s", serial, "install-multiple" };
        args.AddRange(flags.Where(flag => !string.IsNullOrWhiteSpace(flag)));
        args.AddRange(apkPaths.Where(path => !string.IsNullOrWhiteSpace(path)));
        return new AdbCommand(args, "Install app files");
    }

    public AdbCommand Uninstall(string serial, string packageId) =>
        new(["-s", serial, "uninstall", packageId], "Remove previous app");

    public AdbCommand ListPackages(string serial, string? packageId = null)
    {
        var args = new List<string> { "-s", serial, "shell", "pm", "list", "packages" };
        if (!string.IsNullOrWhiteSpace(packageId))
        {
            args.Add(packageId);
        }

        return new AdbCommand(args, "Verify app");
    }

    public AdbCommand Logcat(string serial, string? packageId = null)
    {
        var args = new List<string> { "-s", serial, "logcat", "-d", "-t", "200" };
        if (!string.IsNullOrWhiteSpace(packageId))
        {
            args.Add("-s");
            args.Add($"ActivityManager:I PackageManager:I *:S");
        }

        return new AdbCommand(args, "Collect device log");
    }

    public AdbCommand ResolveLauncher(string serial, string packageId) =>
        new(["-s", serial, "shell", "cmd", "package", "resolve-activity", "--brief", packageId], "Find app to open");

    public AdbCommand Launch(string serial, string component) =>
        new(["-s", serial, "shell", "am", "start", "-n", component], "Open app");
}
