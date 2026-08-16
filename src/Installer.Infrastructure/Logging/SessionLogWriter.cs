using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Logging;

public sealed class SessionLogWriter
{
    private readonly object _gate = new();
    private readonly string _path;

    public SessionLogWriter()
    {
        Directory.CreateDirectory(AppDataPaths.Logs);
        _path = Path.Combine(AppDataPaths.Logs, $"session-{DateTime.Now:yyyyMMdd-HHmmss}.log");
    }

    public string LogPath => _path;

    public void Write(string level, string message)
    {
        var line = $"{DateTimeOffset.Now:O} [{level}] {message}{Environment.NewLine}";
        lock (_gate)
        {
            File.AppendAllText(_path, line);
        }
    }
}
