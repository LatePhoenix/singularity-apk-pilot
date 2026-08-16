namespace Installer.Core.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
