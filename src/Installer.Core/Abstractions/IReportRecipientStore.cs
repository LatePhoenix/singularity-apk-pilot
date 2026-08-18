namespace Installer.Core.Abstractions;

public interface IReportRecipientStore
{
    string? Load();
    void Save(string email);
}
