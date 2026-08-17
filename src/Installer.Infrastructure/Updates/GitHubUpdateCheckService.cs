using System.Net.Http;
using System.Text.Json;
using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Updates;

public sealed class GitHubUpdateCheckService : IUpdateCheckService
{
    private const string LatestUrl = "https://api.github.com/repos/LatePhoenix/singularity-apk-installer/releases/latest";
    private const string DownloadUrl = "https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe";

    public string LatestSetupUrl => DownloadUrl;

    public async Task<string?> GetNewerInstallerMessageAsync(Version currentVersion, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(4) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("SingularityApkInstaller");
            using var response = await client.GetAsync(LatestUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            var tag = doc.RootElement.TryGetProperty("tag_name", out var tagEl) ? tagEl.GetString() ?? "" : "";
            var latest = ParseVersion(tag);
            var current = Normalize(currentVersion);
            if (latest is null || latest <= current)
            {
                return null;
            }

            return $"A newer installer is available ({tag.TrimStart('v', 'V')}).";
        }
        catch
        {
            return null;
        }
    }

    private static Version? ParseVersion(string tag)
    {
        var trimmed = tag.Trim().TrimStart('v', 'V');
        if (Version.TryParse(trimmed, out var version) || Version.TryParse(trimmed + ".0", out version))
        {
            return Normalize(version);
        }

        return null;
    }

    private static Version Normalize(Version version) =>
        new(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
}
