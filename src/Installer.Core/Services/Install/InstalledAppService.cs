using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Adb;
using Installer.Core.Services.Packages;
using Installer.Core.Utilities;

namespace Installer.Core.Services.Install;

public sealed class InstalledAppService : IInstalledAppService
{
    private readonly IAdbClient _adb;
    private readonly AdbOutputParser _parser;
    private readonly IAppLogger _logger;

    public InstalledAppService(IAdbClient adb, AdbOutputParser parser, IAppLogger logger)
    {
        _adb = adb;
        _parser = parser;
        _logger = logger;
    }

    public bool IsProtected(string packageId) => ProtectedPackageFilter.IsProtected(packageId);

    public async Task<Result<IReadOnlyList<InstalledApp>>> ListAsync(
        string serial,
        IReadOnlySet<string>? recentPackageIds = null,
        CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        try
        {
            var ids = await _adb.ListThirdPartyPackagesAsync(serial, cancellationToken);
            var recents = recentPackageIds ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var apps = ids
                .Where(id => !IsProtected(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase)
                .Select(id => new InstalledApp(
                    id,
                    IsRecent: recents.Contains(id)))
                .ToList();
            return Result<IReadOnlyList<InstalledApp>>.Success(apps);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Error("List installed apps failed.", ex);
            return Result<IReadOnlyList<InstalledApp>>.Failure(
                "Could not read installed apps. Keep the device awake and try again.");
        }
    }

    public async Task<UninstallResult> UninstallAsync(string serial, string packageId, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        Guard.NotBlank(packageId, nameof(packageId));
        if (IsProtected(packageId) || !AdbOutputParser.IsSafePackageId(packageId))
        {
            return UninstallResult.Failed(
                packageId,
                "This app cannot be removed from here. System apps are not listed for a reason.");
        }

        try
        {
            var removed = await _adb.UninstallAsync(serial, packageId, cancellationToken);
            var output = removed.CombinedOutput;
            if (!_parser.IsUninstallSuccess(output) && !removed.Succeeded)
            {
                _logger.Warn($"uninstall failed: {output}");
                return UninstallResult.Failed(
                    packageId,
                    "Could not remove this app. Keep the device awake and try again.",
                    output);
            }

            var stillThere = await _adb.IsPackageInstalledAsync(serial, packageId, cancellationToken);
            if (stillThere)
            {
                return UninstallResult.Failed(
                    packageId,
                    "The app is still on the device. Try again, or remove it from headset settings.",
                    output);
            }

            return UninstallResult.Ok(packageId);
        }
        catch (OperationCanceledException)
        {
            return UninstallResult.Failed(packageId, "Removal was cancelled.");
        }
        catch (Exception ex)
        {
            _logger.Error("Uninstall threw.", ex);
            return UninstallResult.Failed(packageId, "Could not remove this app.", ex.Message);
        }
    }

    public async Task<InstalledApp> EnrichAsync(string serial, InstalledApp app, CancellationToken cancellationToken = default)
    {
        Guard.NotBlank(serial, nameof(serial));
        if (!AdbOutputParser.IsSafePackageId(app.PackageId))
        {
            return app;
        }

        try
        {
            var dump = await _adb.DumpPackageAsync(serial, app.PackageId, cancellationToken);
            var (label, version) = _parser.ParsePackageDump(dump);
            if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(version))
            {
                return app;
            }

            return app with
            {
                Label = label ?? app.Label,
                Version = version ?? app.Version
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.Warn($"Package details skipped for {app.PackageId}: {ex.Message}");
            return app;
        }
    }
}
