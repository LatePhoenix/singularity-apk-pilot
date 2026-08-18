using System.Windows;
using Installer.App.ViewModels;
using Installer.App.Views;

namespace Installer.App.Services;

public sealed class TroubleshootUi : ITroubleshootUi
{
    private TroubleshootWindow? _window;

    public void ShowDialog(ShellViewModel shell)
    {
        var window = new TroubleshootWindow(shell);
        _window = window;
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w is ShellWindow && w.IsVisible)
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
        _window = null;
    }

    public void Close()
    {
        var window = _window;
        if (window is null)
        {
            return;
        }

        if (window.Dispatcher.CheckAccess())
        {
            window.Close();
        }
        else
        {
            window.Dispatcher.Invoke(window.Close);
        }
    }
}
