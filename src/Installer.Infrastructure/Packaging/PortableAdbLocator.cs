using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Packaging;

public sealed class PortableAdbLocator : IPortableAdbLocator
{
    public string? FindAdbExecutable()
    {
        foreach (var candidate in Candidates())
        {
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static IEnumerable<string> Candidates()
    {
        var baseDir = AppContext.BaseDirectory;
        yield return Path.Combine(baseDir, "payloads", "tools", "adb", "adb.exe");
        yield return Path.Combine(baseDir, "tools", "adb", "adb.exe");
        yield return Path.Combine(baseDir, "adb", "adb.exe");

        var current = new DirectoryInfo(baseDir);
        for (var i = 0; i < 6 && current is not null; i++)
        {
            yield return Path.Combine(current.FullName, "payloads", "tools", "adb", "adb.exe");
            current = current.Parent;
        }

        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME")
                          ?? Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
        if (!string.IsNullOrWhiteSpace(androidHome))
        {
            yield return Path.Combine(androidHome, "platform-tools", "adb.exe");
        }

        yield return "adb.exe";
    }
}
