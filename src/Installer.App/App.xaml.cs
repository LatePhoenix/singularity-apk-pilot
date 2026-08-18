using System.IO;
using System.Windows;
using Installer.App.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace Installer.App;

public partial class App : Application
{
    private IServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += (_, args) =>
        {
            ShowStartupFailure(args.Exception);
            args.Handled = true;
            Shutdown(1);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                ShowStartupFailure(ex);
            }
        };

        base.OnStartup(e);
        try
        {
            _services = ServiceRegistration.Create();
            var window = _services.GetRequiredService<Views.ShellWindow>();
            window.Show();
        }
        catch (Exception ex)
        {
            ShowStartupFailure(ex);
            Shutdown(1);
        }
    }

    private static void ShowStartupFailure(Exception ex)
    {
        var blocked = ex is FileLoadException
                      || ex.Message.Contains("Application Control", StringComparison.OrdinalIgnoreCase)
                      || ex.Message.Contains("0x800711C7", StringComparison.OrdinalIgnoreCase);
        var text = blocked
            ? "Windows blocked a file this app needs. Install the latest APK Pilot build, or allow this app in Smart App Control / Application Control."
            : ex.Message;
        MessageBox.Show(
            $"{text}\n\n{ex.GetType().FullName}",
            "APK Pilot",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
