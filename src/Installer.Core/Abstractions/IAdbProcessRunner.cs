using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IAdbProcessRunner
{
    Task<AdbProcessResult> RunAsync(AdbCommand command, CancellationToken cancellationToken = default);
}
