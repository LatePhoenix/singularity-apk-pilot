namespace Installer.Infrastructure.Storage;

public sealed class TempFileService
{
    public string CreateTempDirectory(string prefix = "sai-")
    {
        var path = Path.Combine(Path.GetTempPath(), prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
