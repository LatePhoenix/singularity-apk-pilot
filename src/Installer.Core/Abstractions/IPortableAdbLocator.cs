namespace Installer.Core.Abstractions;

public interface IPortableAdbLocator
{
    string? FindAdbExecutable();
}
