using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Storage;

public sealed class TempFileService : ITempFileService
{
    private const string Prefix = "sai-";

    private readonly object _gate = new();
    private readonly List<string> _created = [];
    private readonly string _root;

    public TempFileService()
        : this(Path.GetTempPath())
    {
    }

    public TempFileService(string root)
    {
        _root = root;
    }

    public string CreateTempDirectory(string prefix = Prefix)
    {
        if (!prefix.StartsWith(Prefix, StringComparison.Ordinal))
        {
            prefix = Prefix + prefix;
        }

        var path = Path.Combine(_root, prefix + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        lock (_gate)
        {
            _created.Add(path);
        }

        return path;
    }

    public void DeleteAll()
    {
        string[] paths;
        lock (_gate)
        {
            paths = [.. _created];
            _created.Clear();
        }

        foreach (var path in paths)
        {
            TryDelete(path);
        }
    }

    public void DeleteStale(TimeSpan age)
    {
        try
        {
            var cutoff = DateTime.UtcNow - age;
            foreach (var path in Directory.EnumerateDirectories(_root, Prefix + "*"))
            {
                if (Directory.GetLastWriteTimeUtc(path) < cutoff)
                {
                    TryDelete(path);
                }
            }
        }
        catch (Exception)
        {
            // Best effort; a locked or missing temp folder is not worth failing over.
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception)
        {
            // Files may still be open (adb mid-install); the stale sweep retries next launch.
        }
    }
}
