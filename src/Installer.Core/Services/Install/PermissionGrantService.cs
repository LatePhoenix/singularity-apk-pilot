using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Install;

public sealed class PermissionGrantService
{
    public IReadOnlyList<string> ApplyGrantFlag(IReadOnlyList<string> flags, InstallManifest manifest)
    {
        if (!manifest.GrantPermissions || flags.Contains("-g"))
        {
            return flags;
        }

        var copy = flags.ToList();
        copy.Add("-g");
        return copy;
    }
}
