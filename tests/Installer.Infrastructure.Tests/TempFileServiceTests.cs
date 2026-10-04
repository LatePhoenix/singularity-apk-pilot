using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Tests;

public sealed class TempFileServiceTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "temp-service-test-" + Guid.NewGuid().ToString("N"));

    public TempFileServiceTests()
    {
        Directory.CreateDirectory(_root);
    }

    [Fact]
    public void DeleteAll_removes_directories_created_this_session()
    {
        var service = new TempFileService(_root);
        var bundle = service.CreateTempDirectory("sai-bundle-");
        File.WriteAllText(Path.Combine(bundle, "base.apk"), "x");

        service.DeleteAll();

        Assert.False(Directory.Exists(bundle));
    }

    [Fact]
    public void DeleteStale_removes_only_old_sai_directories()
    {
        var old = Directory.CreateDirectory(Path.Combine(_root, "sai-bundle-old")).FullName;
        Directory.SetLastWriteTimeUtc(old, DateTime.UtcNow.AddDays(-3));
        var fresh = Directory.CreateDirectory(Path.Combine(_root, "sai-bundle-fresh")).FullName;
        var other = Directory.CreateDirectory(Path.Combine(_root, "someone-else")).FullName;
        Directory.SetLastWriteTimeUtc(other, DateTime.UtcNow.AddDays(-3));

        new TempFileService(_root).DeleteStale(TimeSpan.FromDays(1));

        Assert.False(Directory.Exists(old));
        Assert.True(Directory.Exists(fresh));
        Assert.True(Directory.Exists(other));
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
