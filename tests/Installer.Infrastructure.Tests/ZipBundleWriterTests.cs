using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Tests;

public sealed class ZipBundleWriterTests
{
    [Fact]
    public async Task Writes_text_entries()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sai-zip-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var zipPath = Path.Combine(dir, "bundle.zip");
        var writer = new ZipBundleWriter();
        await writer.WriteAsync(zipPath, new Dictionary<string, string> { ["hello.txt"] = "world" });
        Assert.True(File.Exists(zipPath));
        Assert.True(new FileInfo(zipPath).Length > 0);
    }
}
