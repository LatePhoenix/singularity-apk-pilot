using Installer.Core.Models;

namespace Installer.Core.Services.Adb;

public sealed class AdbCommandFactory
{
    public AdbCommand StartServer() => new(["start-server"], "Start connection helper");

    public AdbCommand KillServer() => new(["kill-server"], "Restart connection helper");

    public AdbCommand Devices() => new(["devices", "-l"], "List devices");

    public AdbCommand GetProperty(string serial, string key) =>
        new(["-s", serial, "shell", "getprop", key], $"Read device property {key}");

    public AdbCommand Install(string serial, string apkPath, IReadOnlyList<string> flags)
    {
        var args = new List<string> { "-s", serial, "install" };
        args.AddRange(flags.Where(flag => !string.IsNullOrWhiteSpace(flag)));
        args.Add(apkPath);
        return new AdbCommand(args, "Install app");
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
}
