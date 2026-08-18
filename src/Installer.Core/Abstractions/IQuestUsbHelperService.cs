namespace Installer.Core.Abstractions;

public interface IQuestUsbHelperService
{
    bool HasBundledInf { get; }

    string QuestDriverUrl { get; }

    string PhoneDriverUrl(string? manufacturer);

    Task<bool> TryInstallBundledInfAsync(CancellationToken cancellationToken = default);

    void OpenUrl(string url);
}
