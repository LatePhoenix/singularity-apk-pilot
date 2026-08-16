using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Services.Adb;

public sealed class AdbClient : IAdbClient
{
    private readonly IAdbProcessRunner _runner;
    private readonly AdbCommandFactory _commands;
    private readonly AdbOutputParser _parser;
    private readonly IAppLogger _logger;

    public AdbClient(IAdbProcessRunner runner, AdbCommandFactory commands, AdbOutputParser parser, IAppLogger logger)
    {
        _runner = runner;
        _commands = commands;
        _parser = parser;
        _logger = logger;
    }

    public Task StartServerAsync(CancellationToken cancellationToken = default) =>
        RunAsync(_commands.StartServer(), cancellationToken);

    public Task KillServerAsync(CancellationToken cancellationToken = default) =>
        RunAsync(_commands.KillServer(), cancellationToken);

    public async Task RestartServerAsync(CancellationToken cancellationToken = default)
    {
        await KillServerAsync(cancellationToken);
        await StartServerAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AdbDeviceRecord>> ListDevicesAsync(CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(_commands.Devices(), cancellationToken);
        return _parser.ParseDevices(result.CombinedOutput);
    }

    public async Task<string> GetPropertyAsync(string serial, string key, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(key, nameof(key));
        var result = await RunAsync(_commands.GetProperty(serial, key), cancellationToken);
        return result.StandardOutput.Trim();
    }

    public Task<AdbProcessResult> InstallAsync(string serial, string apkPath, IReadOnlyList<string> flags, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(apkPath, nameof(apkPath));
        return RunAsync(_commands.Install(serial, apkPath, flags), cancellationToken);
    }

    public Task<AdbProcessResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(packageId, nameof(packageId));
        return RunAsync(_commands.Uninstall(serial, packageId), cancellationToken);
    }

    public async Task<bool> IsPackageInstalledAsync(string serial, string packageId, CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(_commands.ListPackages(serial, packageId), cancellationToken);
        return _parser.IsPackageListed(result.StandardOutput, packageId);
    }

    public async Task<string> GetLogcatAsync(string serial, string? packageId, CancellationToken cancellationToken = default)
    {
        var result = await RunAsync(_commands.Logcat(serial, packageId), cancellationToken);
        var output = result.CombinedOutput;
        if (string.IsNullOrWhiteSpace(packageId))
        {
            return output;
        }

        return string.Join(Environment.NewLine, output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Contains(packageId, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<AdbProcessResult> RunAsync(AdbCommand command, CancellationToken cancellationToken)
    {
        _logger.Info($"adb {command.ArgumentString}");
        var result = await _runner.RunAsync(command, cancellationToken);
        if (!result.Succeeded)
        {
            _logger.Warn($"adb exited {result.ExitCode}: {TrimForLog(result.CombinedOutput)}");
        }

        return result;
    }

    private static string TrimForLog(string value) =>
        value.Length <= 500 ? value : value[..500] + "…";
}
