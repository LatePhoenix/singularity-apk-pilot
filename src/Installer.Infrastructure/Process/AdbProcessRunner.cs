using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Infrastructure.Process;

public sealed class AdbProcessRunner : IAdbProcessRunner
{
    private readonly ProcessService _process;
    private readonly IPortableAdbLocator _locator;
    private readonly IAppLogger _logger;

    public AdbProcessRunner(ProcessService process, IPortableAdbLocator locator, IAppLogger logger)
    {
        _process = process;
        _locator = locator;
        _logger = logger;
    }

    public Task<AdbProcessResult> RunAsync(AdbCommand command, CancellationToken cancellationToken = default)
    {
        var adb = _locator.FindAdbExecutable();
        if (string.IsNullOrWhiteSpace(adb) || !File.Exists(adb))
        {
            _logger.Error("Bundled device helper (adb) was not found.");
            return Task.FromResult(new AdbProcessResult(
                ExitCode: 127,
                StandardOutput: "",
                StandardError: "The device helper is missing. Reinstall this program.",
                Duration: TimeSpan.Zero,
                Arguments: command.Arguments));
        }

        return _process.RunAsync(adb, command.Arguments, cancellationToken, Path.GetDirectoryName(adb));
    }
}
