using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Abstractions;

public interface IManifestService
{
    Result<InstallManifest> Load(string? manifestPath = null);
}
