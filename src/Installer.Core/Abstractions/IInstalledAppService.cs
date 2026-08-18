using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Abstractions;

public interface IInstalledAppService
{
    bool IsProtected(string packageId);

    Task<Result<IReadOnlyList<InstalledApp>>> ListAsync(
        string serial,
        IReadOnlySet<string>? recentPackageIds = null,
        CancellationToken cancellationToken = default);

    Task<UninstallResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default);

    Task<InstalledApp> EnrichAsync(string serial, InstalledApp app, CancellationToken cancellationToken = default);
}
