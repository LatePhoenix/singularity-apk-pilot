using System.IO.Compression;
using System.Text.Json;
using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Packages;

public sealed class ApkInspector : IApkInspector
{
    // Inner APKs larger than this are staged on disk instead of in memory; Quest builds can be gigabytes.
    private const long InMemoryLimit = 64L * 1024 * 1024;

    // Real binary manifests are kilobytes; refuse anything that would inflate to a huge buffer.
    private const long MaxManifestBytes = 16L * 1024 * 1024;

    public ApkIdentity? Inspect(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return null;
            }

            var ext = Path.GetExtension(path);
            if (ext.Equals(".apks", StringComparison.OrdinalIgnoreCase) || ext.Equals(".xapk", StringComparison.OrdinalIgnoreCase))
            {
                using var zip = ZipFile.OpenRead(path);
                var apkEntries = zip.Entries
                    .Where(e => e.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(e => e.Name.Contains("base", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                    .ThenBy(e => e.Length);
                foreach (var entry in apkEntries)
                {
                    var identity = InspectEntry(entry, path);
                    if (identity?.HasPackageId == true && !identity.IsSplit)
                    {
                        return identity with { SourcePath = path };
                    }
                }

                return zip.Entries
                    .Select(e => e.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase) ? InspectEntry(e, path) : null)
                    .FirstOrDefault(i => i?.HasPackageId == true);
            }

            using var apk = ZipFile.OpenRead(path);
            var manifest = apk.GetEntry("AndroidManifest.xml");
            if (manifest is null || manifest.Length > MaxManifestBytes)
            {
                return null;
            }

            using var stream = manifest.Open();
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return ManifestIdentityReader.TryRead(memory.ToArray(), path);
        }
        catch
        {
            return null;
        }
    }

    public IReadOnlyList<ApkIdentity> InspectBundle(string path, string extractDirectory)
    {
        var identities = new List<ApkIdentity>();
        try
        {
            Directory.CreateDirectory(extractDirectory);
            using var zip = ZipFile.OpenRead(path);
            foreach (var entry in zip.Entries.Where(e => e.Name.EndsWith(".apk", StringComparison.OrdinalIgnoreCase)))
            {
                var dest = Path.Combine(extractDirectory, Path.GetFileName(entry.FullName));
                entry.ExtractToFile(dest, overwrite: true);
                var identity = Inspect(dest);
                if (identity is not null)
                {
                    identities.Add(identity);
                }
            }

            if (identities.Count == 0)
            {
                TryXapkManifest(zip, path, identities);
            }
        }
        catch
        {
            return identities;
        }

        return identities;
    }

    private static ApkIdentity? InspectEntry(ZipArchiveEntry entry, string sourcePath)
    {
        try
        {
            using var apkStream = entry.Open();
            using Stream apkCopy = entry.Length > InMemoryLimit
                ? new FileStream(
                    Path.Combine(Path.GetTempPath(), "sai-inspect-" + Guid.NewGuid().ToString("N") + ".apk"),
                    FileMode.CreateNew,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    81920,
                    FileOptions.DeleteOnClose)
                : new MemoryStream();
            apkStream.CopyTo(apkCopy);
            apkCopy.Position = 0;
            using var inner = new ZipArchive(apkCopy, ZipArchiveMode.Read, leaveOpen: true);
            var manifest = inner.GetEntry("AndroidManifest.xml");
            if (manifest is null || manifest.Length > MaxManifestBytes)
            {
                return null;
            }

            using var manifestStream = manifest.Open();
            using var memory = new MemoryStream();
            manifestStream.CopyTo(memory);
            return ManifestIdentityReader.TryRead(memory.ToArray(), sourcePath);
        }
        catch
        {
            return null;
        }
    }

    private static void TryXapkManifest(ZipArchive zip, string sourcePath, List<ApkIdentity> identities)
    {
        var manifestEntry = zip.GetEntry("manifest.json");
        if (manifestEntry is null)
        {
            return;
        }

        using var stream = manifestEntry.Open();
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var packageId = root.TryGetProperty("package_name", out var pkg) ? pkg.GetString() ?? "" : "";
        var versionName = root.TryGetProperty("version_name", out var ver) ? ver.GetString() ?? "" : "";
        var label = root.TryGetProperty("name", out var name) ? name.GetString() : null;
        if (!string.IsNullOrWhiteSpace(packageId))
        {
            identities.Add(new ApkIdentity(packageId, versionName, 0, null, label, null, sourcePath));
        }
    }
}
