using System.Runtime.InteropServices;
using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Diagnostics;

public sealed class EnvironmentSnapshotService
{
    private readonly IPortableAdbLocator _adbLocator;
    private readonly IClock _clock;

    public EnvironmentSnapshotService(IPortableAdbLocator adbLocator, IClock clock)
    {
        _adbLocator = adbLocator;
        _clock = clock;
    }

    public IReadOnlyDictionary<string, string> Capture(InstallManifest manifest)
    {
        return new Dictionary<string, string>
        {
            ["capturedUtc"] = _clock.UtcNow.ToString("O"),
            ["os"] = RuntimeInformation.OSDescription,
            ["osArchitecture"] = RuntimeInformation.OSArchitecture.ToString(),
            ["framework"] = RuntimeInformation.FrameworkDescription,
            ["adbPath"] = _adbLocator.FindAdbExecutable() ?? "",
            ["appId"] = manifest.AppId,
            ["buildVersion"] = manifest.BuildVersion
        };
    }
}
