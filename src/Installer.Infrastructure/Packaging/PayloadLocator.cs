using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Packaging;

public sealed class PayloadLocator : IPayloadLocator
{
    public PayloadLocator()
    {
        PayloadRoot = ResolveRoot();
    }

    public string PayloadRoot { get; }

    public string? FindManifestPath()
    {
        var direct = Path.Combine(PayloadRoot, "current", "app-manifest.json");
        return File.Exists(direct) ? direct : null;
    }

    public string ResolveApkPath(string apkPath)
    {
        if (Path.IsPathRooted(apkPath) && File.Exists(apkPath))
        {
            return apkPath;
        }

        var relativeToPayload = Path.GetFullPath(Path.Combine(PayloadRoot, "..", apkPath));
        if (File.Exists(relativeToPayload))
        {
            return relativeToPayload;
        }

        var underCurrent = Path.Combine(PayloadRoot, "current", Path.GetFileName(apkPath));
        if (File.Exists(underCurrent))
        {
            return underCurrent;
        }

        var fromBase = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, apkPath));
        return fromBase;
    }

    private static string ResolveRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        for (var i = 0; i < 6 && current is not null; i++)
        {
            var payloads = Path.Combine(current.FullName, "payloads");
            if (Directory.Exists(payloads))
            {
                return payloads;
            }

            current = current.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "payloads");
    }
}
