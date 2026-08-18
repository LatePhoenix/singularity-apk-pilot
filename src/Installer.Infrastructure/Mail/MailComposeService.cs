using System.Diagnostics;
using System.Runtime.InteropServices;
using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Infrastructure.Mail;

public sealed class MailComposeService : IMailComposeService
{
    private const int MapiDialog = 0x00000008;
    private const int MapiLogonUi = 0x00000001;
    private const int Success = 0;
    private const int UserAbort = 1;

    public MailComposeResult Compose(MailDraft draft, nint ownerHwnd = 0)
    {
        if (TryMapi(draft, ownerHwnd, out var mapi))
        {
            return mapi;
        }

        if (TryOutlook(draft))
        {
            return MailComposeResult.DraftOpened;
        }

        return TryMailto(draft) ? MailComposeResult.DraftOpenedNeedsAttach : MailComposeResult.Failed;
    }

    private static bool TryMapi(MailDraft draft, nint ownerHwnd, out MailComposeResult result)
    {
        result = MailComposeResult.Failed;
        var recipHandle = nint.Zero;
        var fileHandle = nint.Zero;
        try
        {
            var recip = new MapiRecipDesc
            {
                RecipClass = 1,
                Name = draft.To,
                Address = "SMTP:" + draft.To
            };
            recipHandle = Marshal.AllocHGlobal(Marshal.SizeOf<MapiRecipDesc>());
            Marshal.StructureToPtr(recip, recipHandle, false);

            var file = new MapiFileDesc
            {
                Position = -1,
                PathName = draft.AttachmentPath,
                FileName = Path.GetFileName(draft.AttachmentPath)
            };
            fileHandle = Marshal.AllocHGlobal(Marshal.SizeOf<MapiFileDesc>());
            Marshal.StructureToPtr(file, fileHandle, false);

            var message = new MapiMessage
            {
                Subject = draft.Subject,
                NoteText = draft.Body,
                RecipCount = 1,
                Recips = recipHandle,
                FileCount = 1,
                Files = fileHandle
            };

            var code = MAPISendMailW(nint.Zero, ownerHwnd, message, MapiDialog | MapiLogonUi, 0);
            result = code switch
            {
                Success => MailComposeResult.DraftOpened,
                UserAbort => MailComposeResult.Cancelled,
                _ => MailComposeResult.Failed
            };
            return result is MailComposeResult.DraftOpened or MailComposeResult.Cancelled;
        }
        catch
        {
            return false;
        }
        finally
        {
            if (recipHandle != nint.Zero)
            {
                Marshal.DestroyStructure<MapiRecipDesc>(recipHandle);
                Marshal.FreeHGlobal(recipHandle);
            }

            if (fileHandle != nint.Zero)
            {
                Marshal.DestroyStructure<MapiFileDesc>(fileHandle);
                Marshal.FreeHGlobal(fileHandle);
            }
        }
    }

    private static bool TryOutlook(MailDraft draft)
    {
        try
        {
            var mail = $"{draft.To}?subject={Uri.EscapeDataString(draft.Subject)}&body={Uri.EscapeDataString(draft.Body)}";
            var start = new ProcessStartInfo
            {
                FileName = "outlook.exe",
                Arguments = $"/a \"{draft.AttachmentPath}\" /m \"{mail}\"",
                UseShellExecute = true
            };
            using var process = System.Diagnostics.Process.Start(start);
            return process is not null;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryMailto(MailDraft draft)
    {
        try
        {
            var uri =
                $"mailto:{draft.To}?subject={Uri.EscapeDataString(draft.Subject)}&body={Uri.EscapeDataString(draft.Body)}";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });
            return true;
        }
        catch
        {
            return false;
        }
    }

    [DllImport("mapi32.dll", CharSet = CharSet.Unicode)]
    private static extern int MAPISendMailW(nint session, nint uiParam, MapiMessage message, int flags, int reserved);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private sealed class MapiMessage
    {
        public int Reserved;
        public string? Subject;
        public string? NoteText;
        public string? MessageType;
        public string? DateReceived;
        public string? ConversationID;
        public int Flags;
        public nint Originator;
        public int RecipCount;
        public nint Recips;
        public int FileCount;
        public nint Files;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MapiRecipDesc
    {
        public int Reserved;
        public int RecipClass;
        public string? Name;
        public string? Address;
        public int EIDSize;
        public nint EntryID;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct MapiFileDesc
    {
        public int Reserved;
        public int Flags;
        public int Position;
        public string? PathName;
        public string? FileName;
        public nint FileType;
    }
}
