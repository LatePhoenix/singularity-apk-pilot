using System.IO.Compression;
using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Storage;

public sealed class ZipBundleWriter : IZipBundleWriter
{
    public async Task WriteAsync(string zipPath, IReadOnlyDictionary<string, string> textFiles, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(zipPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }

        await using var stream = File.Create(zipPath);
        using var zip = new ZipArchive(stream, ZipArchiveMode.Create);
        foreach (var pair in textFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var entry = zip.CreateEntry(pair.Key, CompressionLevel.Fastest);
            await using var writer = new StreamWriter(entry.Open());
            await writer.WriteAsync(pair.Value.AsMemory(), cancellationToken);
        }
    }
}
