using System.Diagnostics;
using Installer.Infrastructure.Process;

namespace Installer.Infrastructure.Tests;

public sealed class ProcessServiceTests
{
    private static readonly string[] SlowCommand = ["/c", "ping", "-n", "30", "127.0.0.1"];

    [Fact]
    public async Task Hung_process_is_stopped_and_reported_as_timed_out()
    {
        var sw = Stopwatch.StartNew();

        var result = await new ProcessService().RunAsync("cmd.exe", SlowCommand, timeout: TimeSpan.FromMilliseconds(300));

        Assert.Equal(ProcessService.TimedOutExitCode, result.ExitCode);
        Assert.Contains("timed out", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(15), $"took {sw.Elapsed}");
    }

    [Fact]
    public async Task Caller_cancellation_still_throws()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new ProcessService().RunAsync("cmd.exe", SlowCommand, cts.Token, timeout: TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public async Task Fast_process_is_not_affected_by_timeout()
    {
        var result = await new ProcessService().RunAsync("cmd.exe", ["/c", "echo", "hello"], timeout: TimeSpan.FromSeconds(30));

        Assert.Equal(0, result.ExitCode);
        Assert.Contains("hello", result.StandardOutput, StringComparison.Ordinal);
    }
}
