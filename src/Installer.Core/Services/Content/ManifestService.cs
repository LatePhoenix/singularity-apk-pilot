using System.Text.Json;
using Installer.Contracts.Dtos;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Services.Content;

public sealed class ManifestService : IManifestService
{
    private readonly IPayloadLocator _payloads;

    public ManifestService(IPayloadLocator payloads)
    {
        _payloads = payloads;
    }

    public Result<InstallManifest> Load(string? manifestPath = null)
    {
        var path = manifestPath ?? _payloads.FindManifestPath();
        if (path is null || !File.Exists(path))
        {
            return Result<InstallManifest>.Failure("The app payload is missing. Reinstall this program or place app-manifest.json in payloads/current.");
        }

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<InstallManifestDto>(json, JsonDefaults.Manifest);
            if (dto is null)
            {
                return Result<InstallManifest>.Failure("The app payload file is empty.");
            }

            if (!Enum.TryParse<InstallPolicy>(dto.InstallPolicy, ignoreCase: true, out var policy))
            {
                return Result<InstallManifest>.Failure($"Unknown install policy '{dto.InstallPolicy}'.");
            }

            if (string.IsNullOrWhiteSpace(dto.AppId) || string.IsNullOrWhiteSpace(dto.DisplayName) || string.IsNullOrWhiteSpace(dto.ApkPath))
            {
                return Result<InstallManifest>.Failure("The app payload is missing required fields.");
            }

            var notes = dto.PostInstallNotes.ToDictionary(
                pair => pair.Key,
                pair => (IReadOnlyList<string>)pair.Value,
                StringComparer.OrdinalIgnoreCase);

            var manifest = new InstallManifest(
                dto.AppId.Trim(),
                dto.DisplayName.Trim(),
                string.IsNullOrWhiteSpace(dto.BuildVersion) ? "unknown" : dto.BuildVersion.Trim(),
                _payloads.ResolveApkPath(dto.ApkPath),
                dto.TargetPlatforms,
                policy,
                dto.GrantPermissions,
                dto.AllowTestApk,
                dto.LaunchAfterInstall,
                dto.PreferredDeviceFamilies,
                notes,
                dto.Support is null ? null : new SupportContact(dto.Support.ContactLabel, dto.Support.ContactEmail));

            return Result<InstallManifest>.Success(manifest);
        }
        catch (Exception ex)
        {
            return Result<InstallManifest>.Failure($"The app payload could not be read: {ex.Message}");
        }
    }
}
