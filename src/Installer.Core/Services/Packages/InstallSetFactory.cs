using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Packages;

public sealed class InstallSetFactory : IInstallSetFactory
{
    private readonly IApkInspector _inspector;
    private readonly ITempFileService _temp;
    private readonly object _gate = new();
    private readonly Dictionary<string, ExtractedBundle> _bundles = new(StringComparer.OrdinalIgnoreCase);

    public InstallSetFactory(IApkInspector inspector, ITempFileService temp)
    {
        _inspector = inspector;
        _temp = temp;
    }

    public IReadOnlyList<InstallSet> Group(IReadOnlyList<string> paths)
    {
        var sets = new List<InstallSet>();
        var apkByPackage = new Dictionary<string, List<ApkIdentity>>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in paths.Where(p => !string.IsNullOrWhiteSpace(p)))
        {
            var ext = Path.GetExtension(path);
            if (ext.Equals(".apks", StringComparison.OrdinalIgnoreCase) || ext.Equals(".xapk", StringComparison.OrdinalIgnoreCase))
            {
                sets.Add(FromIdentities(ExtractBundle(path), path));
                continue;
            }

            var identity = _inspector.Inspect(path) ?? new ApkIdentity(InstallManifest.UserSelectedAppId, "", 0, null, null, null, path);
            var key = identity.HasPackageId ? identity.PackageId : path;
            if (!apkByPackage.TryGetValue(key, out var list))
            {
                list = [];
                apkByPackage[key] = list;
            }

            list.Add(identity);
        }

        foreach (var group in apkByPackage.Values)
        {
            sets.Add(FromIdentities(group, null));
        }

        return sets;
    }

    private IReadOnlyList<ApkIdentity> ExtractBundle(string path)
    {
        var info = new FileInfo(path);
        var length = info.Exists ? info.Length : -1;
        var written = info.Exists ? info.LastWriteTimeUtc : DateTime.MinValue;
        lock (_gate)
        {
            // Bundles can be gigabytes; unpack each one once per session instead of on every list change.
            if (_bundles.TryGetValue(path, out var cached)
                && cached.Length == length
                && cached.LastWriteUtc == written
                && cached.Identities.All(identity => File.Exists(identity.SourcePath)))
            {
                return cached.Identities;
            }

            var identities = _inspector.InspectBundle(path, _temp.CreateTempDirectory("sai-bundle-"));
            _bundles[path] = new ExtractedBundle(length, written, identities);
            return identities;
        }
    }

    private static InstallSet FromIdentities(IReadOnlyList<ApkIdentity> identities, string? bundlePath)
    {
        if (identities.Count == 0)
        {
            return new InstallSet(InstallManifest.UserSelectedAppId, bundlePath is null ? "app" : Path.GetFileName(bundlePath), "", [], false, false, null, bundlePath);
        }

        var primary = identities.FirstOrDefault(i => !i.IsSplit) ?? identities[0];
        var packageId = primary.HasPackageId ? primary.PackageId : InstallManifest.UserSelectedAppId;
        var hasBase = identities.Any(i => !i.IsSplit);
        var hasSplits = identities.Any(i => i.IsSplit);
        var looksMissing = (hasSplits && !hasBase) || LooksLikeOrphanSplit(identities);
        var paths = identities.Select(i => i.SourcePath).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (!string.IsNullOrWhiteSpace(bundlePath) && identities.All(i => string.Equals(i.SourcePath, bundlePath, StringComparison.OrdinalIgnoreCase)))
        {
            paths = identities.Select(i => i.SourcePath).ToList();
        }

        return new InstallSet(
            packageId,
            primary.DisplayLabel,
            primary.VersionName,
            paths,
            hasSplits || paths.Count > 1,
            looksMissing,
            primary.LauncherActivity,
            bundlePath);
    }

    private static bool LooksLikeOrphanSplit(IReadOnlyList<ApkIdentity> identities)
    {
        if (identities.Count != 1)
        {
            return false;
        }

        var only = identities[0];
        var name = Path.GetFileName(only.SourcePath);
        return only.IsSplit
               || name.Contains("config.", StringComparison.OrdinalIgnoreCase)
               || name.Contains(".split.", StringComparison.OrdinalIgnoreCase);
    }

    private sealed record ExtractedBundle(long Length, DateTime LastWriteUtc, IReadOnlyList<ApkIdentity> Identities);
}
