using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Logging;

public sealed class FileLogger : IAppLogger
{
    private readonly SessionLogWriter _writer;

    public FileLogger(SessionLogWriter writer)
    {
        _writer = writer;
    }

    public void Info(string message) => _writer.Write("INFO", message);

    public void Warn(string message) => _writer.Write("WARN", message);

    public void Error(string message, Exception? exception = null) =>
        _writer.Write("ERROR", exception is null ? message : $"{message} {exception}");
}
