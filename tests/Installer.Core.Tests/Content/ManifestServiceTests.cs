using Installer.Core.Abstractions;
using Installer.Core.Services.Content;

namespace Installer.Core.Tests.Content;

public sealed class ManifestServiceTests
{
    [Fact]
    public void Missing_file_uses_session_defaults()
    {
        var service = new ManifestService(new MissingLocator());
        var loaded = service.Load();
        Assert.True(loaded.IsSuccess);
        Assert.Equal(Installer.Core.Models.InstallManifest.UserSelectedAppId, loaded.Value!.AppId);
        Assert.Equal("", loaded.Value.ApkPath);
    }

    private sealed class MissingLocator : IPayloadLocator
    {
        public string PayloadRoot => "payloads";
        public string? FindManifestPath() => null;
        public string ResolveApkPath(string apkPath) => apkPath;
    }
}
