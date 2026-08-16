using System.Reflection;

namespace Installer.Infrastructure.Packaging;

public sealed class BuildStampReader
{
    public string Version =>
        Assembly.GetEntryAssembly()?.GetName().Version?.ToString()
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
        ?? "0.1.0";
}
