using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Storage;

public sealed class UserDataPaths : IUserDataPaths
{
    public string DiagnosticsDirectory => AppDataPaths.Diagnostics;
}
