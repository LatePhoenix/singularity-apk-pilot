using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Tests;

public sealed class RecentsStoreTests
{
    [Fact]
    public void Load_drops_missing_files()
    {
        var root = Path.Combine(Path.GetTempPath(), "sai-recents-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var existing = Path.Combine(root, "keep.apk");
        File.WriteAllText(existing, "apk");
        var missing = Path.Combine(root, "gone.apk");
        var folder = Path.Combine(root, "folder");
        Directory.CreateDirectory(folder);
        var paths = new FakePaths(root);
        var store = new RecentsStore(paths, new NoopLog());
        store.Save(new RecentsState(folder, [existing, missing]));

        var loaded = store.Load();
        Assert.Equal([existing], loaded.LastFiles);
        Assert.Equal(folder, loaded.LastFolder);
    }

    [Fact]
    public void Load_missing_store_is_empty()
    {
        var root = Path.Combine(Path.GetTempPath(), "sai-recents-" + Guid.NewGuid().ToString("N"));
        var loaded = new RecentsStore(new FakePaths(root), new NoopLog()).Load();
        Assert.Empty(loaded.LastFiles);
        Assert.Null(loaded.LastFolder);
    }

    private sealed class FakePaths(string root) : IUserDataPaths
    {
        public string DiagnosticsDirectory => Path.Combine(root, "diag");
        public string WirelessEndpointPath => Path.Combine(root, "wifi.json");
        public string RecentsPath => Path.Combine(root, "recents.json");
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? exception = null) { }
    }
}
