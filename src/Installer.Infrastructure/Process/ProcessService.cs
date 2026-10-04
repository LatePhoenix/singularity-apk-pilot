using System.Diagnostics;
using System.Text;
using Installer.Core.Models;

namespace Installer.Infrastructure.Process;

public sealed class ProcessService
{
    public const int TimedOutExitCode = -1;

    public async Task<AdbProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default,
        string? workingDirectory = null,
        TimeSpan? timeout = null)
    {
        var start = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(fileName) ?? Environment.CurrentDirectory
        };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        var stdout = new StringBuilder();
        var stderr = new StringBuilder();
        using var process = new System.Diagnostics.Process { StartInfo = start, EnableRaisingEvents = true };
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                lock (stdout)
                {
                    stdout.AppendLine(e.Data);
                }
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
            {
                lock (stderr)
                {
                    stderr.AppendLine(e.Data);
                }
            }
        };

        var sw = Stopwatch.StartNew();
        if (!process.Start())
        {
            throw new InvalidOperationException($"Could not start {fileName}.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (timeout is { } limit && limit > TimeSpan.Zero && limit != Timeout.InfiniteTimeSpan)
        {
            timeoutCts.CancelAfter(limit);
        }

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            sw.Stop();
            string error;
            lock (stderr)
            {
                stderr.AppendLine($"adb timed out after {sw.Elapsed.TotalSeconds:0} seconds.");
                error = stderr.ToString();
            }

            string output;
            lock (stdout)
            {
                output = stdout.ToString();
            }

            return new AdbProcessResult(TimedOutExitCode, output, error, sw.Elapsed, arguments);
        }
        catch (OperationCanceledException)
        {
            TryKill(process);
            throw;
        }

        sw.Stop();
        return new AdbProcessResult(process.ExitCode, stdout.ToString(), stderr.ToString(), sw.Elapsed, arguments);
    }

    private static void TryKill(System.Diagnostics.Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // ignored
        }
    }
}
