using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IRecoveryService
{
    IReadOnlyList<RecoveryAction> Suggest(InstallError error, InstallManifest manifest);
    Task<InstallResult?> TryAutoFixAsync(InstallRequest request, InstallResult failure, CancellationToken cancellationToken = default);
}
