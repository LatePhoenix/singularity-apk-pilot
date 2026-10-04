namespace Installer.Core.Abstractions;

public interface ITempFileService
{
    string CreateTempDirectory(string prefix = "sai-");

    /// <summary>Deletes every directory this session created.</summary>
    void DeleteAll();

    /// <summary>Deletes leftover directories from earlier sessions that are older than <paramref name="age"/>.</summary>
    void DeleteStale(TimeSpan age);
}
