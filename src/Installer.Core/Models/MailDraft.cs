namespace Installer.Core.Models;

public sealed record MailDraft(string To, string Subject, string Body, string AttachmentPath);

public enum MailComposeResult
{
    DraftOpened = 0,
    DraftOpenedNeedsAttach = 1,
    Cancelled = 2,
    Failed = 3
}
