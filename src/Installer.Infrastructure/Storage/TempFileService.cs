using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Storage;

public sealed class TempFileService : ITempFileService
{
    public string CreateTempDirectory(string prefix = "sai-")
    {
        var path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
