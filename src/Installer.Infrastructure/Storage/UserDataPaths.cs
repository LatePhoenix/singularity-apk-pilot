using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Storage;

public sealed class UserDataPaths : IUserDataPaths
{
    public string DiagnosticsDirectory => AppDataPaths.Diagnostics;

    public string WirelessEndpointPath => Path.Combine(AppDataPaths.Root, "wireless-endpoint.json");
}
