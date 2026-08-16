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
            return Result<InstallManifest>.Success(InstallManifest.Session);
        }

        try
        {
            var json = File.ReadAllText(path);
            var dto = JsonSerializer.Deserialize<InstallManifestDto>(json, JsonDefaults.Manifest);
            if (dto is null)
            {
                return Result<InstallManifest>.Failure("The app payload file is empty.");
            }

            var policy = InstallManifest.Session.InstallPolicy;
            if (!string.IsNullOrWhiteSpace(dto.InstallPolicy))
            {
                if (!Enum.TryParse<InstallPolicy>(dto.InstallPolicy, ignoreCase: true, out policy))
                {
                    return Result<InstallManifest>.Failure($"Unknown install policy '{dto.InstallPolicy}'.");
                }
            }

            var notes = dto.PostInstallNotes.Count == 0
                ? InstallManifest.Session.PostInstallNotes
                : dto.PostInstallNotes.ToDictionary(
                    pair => pair.Key,
                    pair => (IReadOnlyList<string>)pair.Value,
                    StringComparer.OrdinalIgnoreCase);

            var apkPath = string.IsNullOrWhiteSpace(dto.ApkPath)
                ? ""
                : _payloads.ResolveApkPath(dto.ApkPath);

            var platforms = dto.TargetPlatforms.Count == 0
                ? InstallManifest.Session.TargetPlatforms
                : dto.TargetPlatforms;

            var families = dto.PreferredDeviceFamilies.Count == 0
                ? InstallManifest.Session.PreferredDeviceFamilies
                : dto.PreferredDeviceFamilies;

            var manifest = new InstallManifest(
                string.IsNullOrWhiteSpace(dto.AppId) ? InstallManifest.UserSelectedAppId : dto.AppId.Trim(),
                string.IsNullOrWhiteSpace(dto.DisplayName) ? InstallManifest.Session.DisplayName : dto.DisplayName.Trim(),
                string.IsNullOrWhiteSpace(dto.BuildVersion) ? "" : dto.BuildVersion.Trim(),
                apkPath,
                platforms,
                policy,
                dto.GrantPermissions,
                dto.AllowTestApk,
                dto.LaunchAfterInstall,
                families,
                notes,
                dto.Support is null ? InstallManifest.Session.Support : new SupportContact(dto.Support.ContactLabel, dto.Support.ContactEmail));

            return Result<InstallManifest>.Success(manifest);
        }
        catch (Exception ex)
        {
            return Result<InstallManifest>.Failure($"The app payload could not be read: {ex.Message}");
        }
    }
}
