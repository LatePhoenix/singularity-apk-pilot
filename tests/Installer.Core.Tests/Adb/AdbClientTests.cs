using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;

namespace Installer.Core.Tests.Adb;

public sealed class AdbClientTests
{
    [Fact]
    public async Task RestartServer_fails_when_kill_fails()
    {
        var runner = new ScriptedRunner();
        runner.Results["kill-server"] = new AdbProcessResult(1, "", "cannot kill", TimeSpan.Zero, ["kill-server"]);
        var client = Create(runner);

        var result = await client.RestartServerAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(["kill-server"], runner.Calls);
    }

    [Fact]
    public async Task RestartServer_fails_when_start_fails()
    {
        var runner = new ScriptedRunner();
        runner.Results["start-server"] = new AdbProcessResult(-1, "", "timed out", TimeSpan.FromSeconds(15), ["start-server"]);
        var client = Create(runner);

        var result = await client.RestartServerAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(["kill-server", "start-server"], runner.Calls);
    }

    [Fact]
    public async Task RestartServer_succeeds_when_kill_and_start_succeed()
    {
        var runner = new ScriptedRunner();
        var client = Create(runner);

        var result = await client.RestartServerAsync();

        Assert.True(result.Succeeded);
        Assert.Equal(["kill-server", "start-server"], runner.Calls);
    }

    [Fact]
    public async Task ListDevices_throws_when_adb_cannot_answer()
    {
        var runner = new ScriptedRunner();
        runner.Results["devices"] = new AdbProcessResult(1, "", "adb.exe: cannot connect to daemon", TimeSpan.Zero, ["devices", "-l"]);
        var client = Create(runner);

        await Assert.ThrowsAsync<AdbCommandException>(() => client.ListDevicesAsync());
    }

    [Fact]
    public async Task ListDevices_returns_empty_when_adb_answers_with_no_devices()
    {
        var runner = new ScriptedRunner();
        runner.Results["devices"] = new AdbProcessResult(0, "List of devices attached\n\n", "", TimeSpan.Zero, ["devices", "-l"]);
        var client = Create(runner);

        Assert.Empty(await client.ListDevicesAsync());
    }

    [Fact]
    public void Install_commands_get_a_long_timeout_and_queries_a_short_one()
    {
        var commands = new AdbCommandFactory();

        Assert.True(commands.Install("serial", "app.apk", []).Timeout >= TimeSpan.FromMinutes(10));
        Assert.True(commands.InstallMultiple("serial", ["a.apk", "b.apk"], []).Timeout >= TimeSpan.FromMinutes(10));
        Assert.True(commands.GetProperty("serial", "ro.product.model").Timeout <= TimeSpan.FromSeconds(30));
        Assert.Equal(AdbCommand.DefaultTimeout, commands.Devices().Timeout);
    }

    private static AdbClient Create(IAdbProcessRunner runner) =>
        new(runner, new AdbCommandFactory(), new AdbOutputParser(), new NoopLog());

    private sealed class ScriptedRunner : IAdbProcessRunner
    {
        public List<string> Calls { get; } = [];

        public Dictionary<string, AdbProcessResult> Results { get; } = new(StringComparer.Ordinal);

        public Task<AdbProcessResult> RunAsync(AdbCommand command, CancellationToken cancellationToken = default)
        {
            var key = command.Arguments[0];
            Calls.Add(key);
            if (Results.TryGetValue(key, out var result))
            {
                return Task.FromResult(result);
            }

            return Task.FromResult(new AdbProcessResult(0, "", "", TimeSpan.Zero, command.Arguments));
        }
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }

        public void Warn(string message) { }

        public void Error(string message, Exception? exception = null) { }
    }
}
