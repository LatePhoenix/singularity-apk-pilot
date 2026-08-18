using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.App.ViewModels;

public sealed partial class SendReportViewModel : ObservableObject
{
    private readonly IDiagnosticsService _diagnostics;
    private readonly IUserDataPaths _paths;
    private readonly IMailComposeService _mail;
    private readonly IReportRecipientStore _recipients;
    private readonly IAppLogger _logger;
    private readonly InstallManifest _manifest;
    private readonly DeviceInfo? _device;
    private readonly InstallResult? _lastResult;

    public SendReportViewModel(
        IDiagnosticsService diagnostics,
        IUserDataPaths paths,
        IMailComposeService mail,
        IReportRecipientStore recipients,
        IAppLogger logger,
        InstallManifest manifest,
        DeviceInfo? device,
        InstallResult? lastResult,
        string initialEmail)
    {
        _diagnostics = diagnostics;
        _paths = paths;
        _mail = mail;
        _recipients = recipients;
        _logger = logger;
        _manifest = manifest;
        _device = device;
        _lastResult = lastResult;
        Email = initialEmail;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string email = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool isBusy;

    [ObservableProperty]
    private string error = "";

    [ObservableProperty]
    private bool showResult;

    [ObservableProperty]
    private string resultHeadline = "";

    [ObservableProperty]
    private string resultBody = "";

    [ObservableProperty]
    private string attachmentPath = "";

    [ObservableProperty]
    private bool showAttachHint;

    [ObservableProperty]
    private string copyHint = "";

    public nint OwnerHandle { get; set; }

    public string Status { get; private set; } = "";

    public bool CanSend => !IsBusy && EmailAddress.TryNormalize(Email, out _);

    public event Action? CloseRequested;

    [RelayCommand(CanExecute = nameof(CanSend))]
    private async Task SendAsync()
    {
        if (!EmailAddress.TryNormalize(Email, out var to))
        {
            Error = "Enter an email address, like name@email.com.";
            return;
        }

        IsBusy = true;
        Error = "";
        CopyHint = "";
        try
        {
            Directory.CreateDirectory(_paths.DiagnosticsDirectory);
            var info = await _diagnostics.ExportAsync(
                _manifest,
                _device,
                _lastResult,
                null,
                _paths.DiagnosticsDirectory);
            AttachmentPath = info.ZipPath;
            _recipients.Save(to);

            var draft = new MailDraft(
                to,
                "Singularity APK Installer report",
                "I'm sending a report from Singularity APK Installer. The ZIP file should be attached.",
                info.ZipPath);
            var result = _mail.Compose(draft, OwnerHandle);
            switch (result)
            {
                case MailComposeResult.DraftOpened:
                    Status = "Check your email app. Send that message if it is still open.";
                    ShowOutcome(
                        "Check your email app",
                        $"A message to {to} should be ready, with the report attached. If that window is still open, press Send. If you already sent it, you are done.");
                    break;
                case MailComposeResult.DraftOpenedNeedsAttach:
                    Status = "Your email app opened. Attach the report if it is missing, then send.";
                    ShowOutcome(
                        "Attach the report in your email app",
                        $"A message to {to} should be open, but the file may not be attached yet. In your email app, choose Attach, pick the file below, then press Send.",
                        showAttachHint: true);
                    break;
                case MailComposeResult.Cancelled:
                    Error = "The email was cancelled. You can try again.";
                    ShowAttachHint = true;
                    break;
                default:
                    Status = "Could not open your email app.";
                    Error = "Could not open your email app. Check that this computer has Mail or Outlook, then try again. You can also copy the file path below and attach it yourself.";
                    ShowAttachHint = true;
                    break;
            }

            _logger.Info($"Report compose result: {result}.");
        }
        catch (Exception ex)
        {
            _logger.Error("Report send failed.", ex);
            Error = "Could not create the report. Try again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void CopyPath()
    {
        if (string.IsNullOrWhiteSpace(AttachmentPath))
        {
            return;
        }

        try
        {
            System.Windows.Clipboard.SetText(AttachmentPath);
            CopyHint = "Copied. In your email app, choose Attach and paste this if asked for a file location.";
        }
        catch
        {
            CopyHint = "Could not copy. Select the path above and copy it yourself.";
        }
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke();

    private void ShowOutcome(string headline, string body, bool showAttachHint = false)
    {
        ShowResult = true;
        ResultHeadline = headline;
        ResultBody = body;
        ShowAttachHint = showAttachHint;
    }
}
