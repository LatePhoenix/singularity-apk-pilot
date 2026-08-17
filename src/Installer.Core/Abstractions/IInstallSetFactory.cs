using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IInstallSetFactory
{
    IReadOnlyList<InstallSet> Group(IReadOnlyList<string> paths);
}
