namespace Installer.Core.Abstractions;

public interface IZipBundleWriter
{
    Task WriteAsync(string zipPath, IReadOnlyDictionary<string, string> textFiles, CancellationToken cancellationToken = default);
}
