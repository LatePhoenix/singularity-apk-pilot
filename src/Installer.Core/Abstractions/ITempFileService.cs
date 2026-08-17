namespace Installer.Core.Abstractions;

public interface ITempFileService
{
    string CreateTempDirectory(string prefix = "sai-");
}
