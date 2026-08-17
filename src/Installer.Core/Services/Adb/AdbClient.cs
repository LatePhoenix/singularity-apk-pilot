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

    public Task<AdbProcessResult> InstallMultipleAsync(string serial, IReadOnlyList<string> apkPaths, IReadOnlyList<string> flags, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        return RunAsync(_commands.InstallMultiple(serial, apkPaths, flags), cancellationToken);
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

    public Task<AdbProcessResult> TcpIpAsync(string serial, int port = 5555, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        return RunAsync(_commands.TcpIp(serial, port), cancellationToken);
    }

    public Task<AdbProcessResult> ConnectAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(endpoint, nameof(endpoint));
        return RunAsync(_commands.Connect(endpoint), cancellationToken);
    }

    public Task<AdbProcessResult> DisconnectAsync(string? endpoint = null, CancellationToken cancellationToken = default) =>
        RunAsync(_commands.Disconnect(endpoint), cancellationToken);

    public Task<AdbProcessResult> PairAsync(string endpoint, string pairingCode, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(endpoint, nameof(endpoint));
        Guard.NotBlank(pairingCode, nameof(pairingCode));
        return RunAsync(_commands.Pair(endpoint, pairingCode), cancellationToken);
    }

    public async Task<string?> GetWifiAddressAsync(string serial, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        var result = await RunAsync(_commands.WifiAddresses(serial), cancellationToken);
        return _parser.ParseWifiAddress(result.CombinedOutput);
    }

    public async Task<string?> ResolveLauncherAsync(string serial, string packageId, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(packageId, nameof(packageId));
        var result = await RunAsync(_commands.ResolveLauncher(serial, packageId), cancellationToken);
        return _parser.ParseLauncher(result.CombinedOutput, packageId);
    }

    public Task<AdbProcessResult> LaunchAsync(string serial, string packageId, string? activity, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(packageId, nameof(packageId));
        var component = AdbOutputParser.ToComponent(packageId, activity);
        if (string.IsNullOrWhiteSpace(component))
        {
            return Task.FromResult(new AdbProcessResult(1, "", "No app screen to open.", TimeSpan.Zero, []));
        }

        return RunAsync(_commands.Launch(serial, component), cancellationToken);
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
