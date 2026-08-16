using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IInstallService
{
    InstallPlan CreatePlan(InstallRequest request);
    Task<InstallResult> InstallAsync(InstallRequest request, CancellationToken cancellationToken = default);
}
