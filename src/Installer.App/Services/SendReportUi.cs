using System.Windows;
using Installer.App.ViewModels;
using Installer.App.Views;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.App.Services;

public sealed class SendReportUi : ISendReportUi
{
    private readonly IDiagnosticsService _diagnostics;
    private readonly IUserDataPaths _paths;
    private readonly IMailComposeService _mail;
    private readonly IReportRecipientStore _recipients;
    private readonly IAppLogger _logger;

    public SendReportUi(
        IDiagnosticsService diagnostics,
        IUserDataPaths paths,
        IMailComposeService mail,
        IReportRecipientStore recipients,
        IAppLogger logger)
    {
        _diagnostics = diagnostics;
        _paths = paths;
        _mail = mail;
        _recipients = recipients;
        _logger = logger;
    }

    public SendReportUiResult Show(InstallManifest manifest, DeviceInfo? device, InstallResult? lastResult)
    {
        var initial = EmailAddress.Default(
            _recipients.Load(),
            manifest.Support?.ContactEmail,
            PublisherLegal.PublisherEmail);
        var viewModel = new SendReportViewModel(
            _diagnostics,
            _paths,
            _mail,
            _recipients,
            _logger,
            manifest,
            device,
            lastResult,
            initial);
        var window = new SendReportWindow(viewModel);
        var owner = Application.Current?.Windows.OfType<Window>()
                         .LastOrDefault(w => w.IsVisible && w is TroubleshootWindow)
                     ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w is ShellWindow && w.IsVisible)
                     ?? Application.Current?.MainWindow;
        if (owner is not null && !ReferenceEquals(owner, window))
        {
            window.Owner = owner;
        }
        else
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        window.ShowDialog();
        return new SendReportUiResult(viewModel.Status);
    }
}
