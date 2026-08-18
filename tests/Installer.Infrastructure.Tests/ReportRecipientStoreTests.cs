using Installer.Core.Abstractions;
using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Tests;

public sealed class ReportRecipientStoreTests
{
    [Fact]
    public void Save_then_load_round_trips_normalized_email()
    {
        var root = Path.Combine(Path.GetTempPath(), "sai-report-email-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var store = new ReportRecipientStore(new FakePaths(root), new NoopLog());
        store.Save("  Matt.Brossard323@gmail.com  ");

        Assert.Equal("Matt.Brossard323@gmail.com", store.Load());
    }

    [Fact]
    public void Load_missing_file_is_null()
    {
        var root = Path.Combine(Path.GetTempPath(), "sai-report-email-" + Guid.NewGuid().ToString("N"));
        Assert.Null(new ReportRecipientStore(new FakePaths(root), new NoopLog()).Load());
    }

    [Fact]
    public void Save_ignores_invalid_email()
    {
        var root = Path.Combine(Path.GetTempPath(), "sai-report-email-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        var store = new ReportRecipientStore(new FakePaths(root), new NoopLog());
        store.Save("not-an-email");
        Assert.Null(store.Load());
    }

    private sealed class FakePaths(string root) : IUserDataPaths
    {
        public string DiagnosticsDirectory => Path.Combine(root, "diag");
        public string WirelessEndpointPath => Path.Combine(root, "wifi.json");
        public string RecentsPath => Path.Combine(root, "recents.json");
        public string ReportRecipientPath => Path.Combine(root, "report-email.json");
    }

    private sealed class NoopLog : IAppLogger
    {
        public void Info(string message) { }
        public void Warn(string message) { }
        public void Error(string message, Exception? exception = null) { }
    }
}
