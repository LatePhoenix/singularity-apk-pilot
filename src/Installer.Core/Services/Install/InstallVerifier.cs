using Installer.Core.Abstractions;

namespace Installer.Core.Services.Install;

public sealed class InstallVerifier
{
    private readonly IAdbClient _adb;

    public InstallVerifier(IAdbClient adb)
    {
        _adb = adb;
    }

    public Task<bool> VerifyAsync(string serial, string packageId, CancellationToken cancellationToken = default) =>
        _adb.IsPackageInstalledAsync(serial, packageId, cancellationToken);
}
