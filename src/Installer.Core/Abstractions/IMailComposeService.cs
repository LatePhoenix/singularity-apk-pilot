using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IMailComposeService
{
    MailComposeResult Compose(MailDraft draft, nint ownerHwnd = 0);
}
