using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Installer.Contracts.Dtos;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Core.Services.Diagnostics;

public sealed class DiagnosticsService : IDiagnosticsService
{
    private readonly IAdbClient _adb;
    private readonly IClock _clock;
    private readonly IZipBundleWriter _zip;
    private readonly LogcatCollector _logcat;
    private readonly EnvironmentSnapshotService _environment;
    private readonly IUsbEvidenceProbe _usb;
    private readonly ISessionLog _sessionLog;

    public DiagnosticsService(
        IAdbClient adb,
        IClock clock,
        IZipBundleWriter zip,
        LogcatCollector logcat,
        EnvironmentSnapshotService environment,
        IUsbEvidenceProbe usb,
        ISessionLog sessionLog)
    {
        _adb = adb;
        _clock = clock;
        _zip = zip;
        _logcat = logcat;
        _environment = environment;
        _usb = usb;
        _sessionLog = sessionLog;
    }

    public async Task<DiagnosticBundleInfo> ExportAsync(
        InstallManifest manifest,
        DeviceInfo? device,
        InstallResult? lastResult,
        string? adbDevicesRaw,
        string destinationDirectory,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(destinationDirectory);
        var stamp = _clock.UtcNow;
        var zipPath = Path.Combine(destinationDirectory, $"singularity-diagnostics-{stamp:yyyyMMdd-HHmmss}.zip");
        var version = typeof(DiagnosticsService).Assembly.GetName().Version?.ToString() ?? "0.1.0";

        if (adbDevicesRaw is null)
        {
            try
            {
                var list = await _adb.ListDevicesAsync(cancellationToken);
                adbDevicesRaw = string.Join(Environment.NewLine, list.Select(d => $"{HashSerial(d.Serial)} {d.State}"));
            }
            catch (Exception ex)
            {
                adbDevicesRaw = ex.Message;
            }
        }

        var logcat = await _logcat.CollectAsync(device, manifest.AppId, cancellationToken);
        var metadata = new DiagnosticBundleDto
        {
            CreatedUtc = stamp,
            InstallerVersion = version,
            AppId = manifest.AppId,
            BuildVersion = manifest.BuildVersion,
            DeviceKind = device?.Kind.ToString(),
            InstallError = lastResult?.Error?.ToString(),
            Device = device is null ? null : ToSnapshot(device),
            LastAttempt = lastResult is null ? null : ToAttempt(manifest, lastResult, stamp)
        };

        var evidence = _usb.Collect();
        var files = new Dictionary<string, string>
        {
            ["metadata.json"] = JsonSerializer.Serialize(metadata, JsonDefaults.Manifest),
            ["environment.json"] = JsonSerializer.Serialize(_environment.Capture(manifest), JsonDefaults.Manifest),
            ["device.json"] = JsonSerializer.Serialize(metadata.Device, JsonDefaults.Manifest),
            ["usb-evidence.json"] = JsonSerializer.Serialize(ToEvidence(evidence), JsonDefaults.Manifest),
            ["adb-devices.txt"] = Sanitize(adbDevicesRaw, device?.Serial),
            ["install-attempt.json"] = JsonSerializer.Serialize(metadata.LastAttempt, JsonDefaults.Manifest),
            ["adb-output.txt"] = Sanitize(lastResult?.RawOutput ?? "", device?.Serial),
            ["logcat-filtered.txt"] = Sanitize(logcat, device?.Serial),
            ["session-log.txt"] = Sanitize(_sessionLog.ReadAll(), device?.Serial)
        };

        await _zip.WriteAsync(zipPath, files, cancellationToken);
        return new DiagnosticBundleInfo(zipPath, stamp, manifest.AppId, version);
    }

    private static UsbEvidenceDto ToEvidence(UsbEvidence evidence) => new()
    {
        QuestUsbPresent = evidence.QuestUsbPresent,
        AndroidUsbPresent = evidence.AndroidUsbPresent,
        AdbInterfacePresent = evidence.AdbInterfacePresent,
        AdbDriverMissing = evidence.AdbDriverMissing,
        MtpOnly = evidence.MtpOnly,
        CompetingAdbProcess = evidence.CompetingAdbProcess
    };

    private static DeviceSnapshotDto ToSnapshot(DeviceInfo device) => new()
    {
        SerialHash = HashSerial(device.Serial),
        Manufacturer = device.Manufacturer,
        Model = device.Model,
        AndroidVersion = device.AndroidVersion,
        Kind = device.Kind.ToString(),
        ConnectionState = device.State.ToString(),
        IsAuthorized = device.IsAuthorized,
        IsQuest = device.IsQuest
    };

    private static InstallAttemptDto ToAttempt(InstallManifest manifest, InstallResult result, DateTimeOffset stamp) => new()
    {
        PackageId = manifest.AppId,
        ApkPath = result.Plan?.ApkPath ?? manifest.ApkPath,
        Policy = (result.Plan?.Policy ?? manifest.InstallPolicy).ToString(),
        AdbFlags = result.Plan?.AdbFlags.ToList() ?? [],
        RequiresUninstallFirst = result.Plan?.RequiresUninstallFirst ?? false,
        Success = result.Success,
        Error = result.Error?.ToString(),
        ExitCode = result.ExitCode,
        StartedUtc = stamp,
        EndedUtc = stamp
    };

    private static string HashSerial(string serial)
    {
        if (string.IsNullOrWhiteSpace(serial))
        {
            return "";
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(serial));
        return Convert.ToHexString(hash)[..12];
    }

    private static string Sanitize(string text, string? serial)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(serial))
        {
            return text;
        }

        return text.Replace(serial, HashSerial(serial), StringComparison.OrdinalIgnoreCase);
    }
}
