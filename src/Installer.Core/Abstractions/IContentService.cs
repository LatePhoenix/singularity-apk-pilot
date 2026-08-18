using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IContentService
{
    WizardCopy GetCopy(
        WizardStep step,
        InstallManifest manifest,
        DeviceInfo? device,
        InstallError? error = null,
        DeviceHealth? health = null,
        TroubleshootSession? troubleshoot = null);
}
