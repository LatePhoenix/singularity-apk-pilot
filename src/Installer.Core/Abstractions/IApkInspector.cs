using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IApkInspector
{
    ApkIdentity? Inspect(string path);
    IReadOnlyList<ApkIdentity> InspectBundle(string path, string extractDirectory);
}
