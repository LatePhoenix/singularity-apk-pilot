using Installer.Core.Abstractions;

namespace Installer.Infrastructure.Devices;

public sealed class QuestUsbHelperService : IQuestUsbHelperService
{
    public const string DefaultQuestDriverUrl =
        "https://developers.meta.com/horizon/downloads/package/oculus-adb-drivers/";

    public const string DefaultPhoneDriverUrl =
        "https://developer.android.com/studio/run/oem-usb";

    public const string SamsungDriverUrl =
        "https://developer.samsung.com/android/usb-driver.html";

    private readonly IPayloadLocator _payloads;

    public QuestUsbHelperService(IPayloadLocator payloads)
    {
        _payloads = payloads;
    }

    public bool HasBundledInf => BundledInfPath() is not null;

    public string QuestDriverUrl => DefaultQuestDriverUrl;

    public string PhoneDriverUrl(string? manufacturer)
    {
        if (!string.IsNullOrWhiteSpace(manufacturer)
            && manufacturer.Contains("samsung", StringComparison.OrdinalIgnoreCase))
        {
            return SamsungDriverUrl;
        }

        return DefaultPhoneDriverUrl;
    }

    public async Task<bool> TryInstallBundledInfAsync(CancellationToken cancellationToken = default)
    {
        var inf = BundledInfPath();
        if (inf is null)
        {
            return false;
        }

        var start = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "pnputil.exe",
            ArgumentList = { "/add-driver", inf, "/install" },
            UseShellExecute = true,
            Verb = "runas"
        };

        using var process = System.Diagnostics.Process.Start(start);
        if (process is null)
        {
            return false;
        }

        await process.WaitForExitAsync(cancellationToken);
        return process.ExitCode == 0;
    }

    public void OpenUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    private string? BundledInfPath()
    {
        var root = _payloads.PayloadRoot;
        string[] candidates =
        [
            Path.Combine(root, "tools", "oculus-adb-drivers", "android_winusb.inf"),
            Path.Combine(root, "tools", "adb", "android_winusb.inf")
        ];

        return candidates.FirstOrDefault(File.Exists);
    }
}
