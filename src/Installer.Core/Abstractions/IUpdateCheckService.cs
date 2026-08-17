namespace Installer.Core.Abstractions;

public interface IUpdateCheckService
{
    string LatestSetupUrl { get; }

    Task<string?> GetNewerInstallerMessageAsync(Version currentVersion, CancellationToken cancellationToken = default);
}
