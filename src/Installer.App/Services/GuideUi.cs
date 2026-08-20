using System.Windows;
using Installer.App.ViewModels;
using Installer.App.Views;

namespace Installer.App.Services;

public sealed class GuideUi : IGuideUi
{
    private GuideWindow? _window;
    private bool _closing;

    public bool IsOpen => _window is not null;

    public event Action? ClosedByUser;

    public void ShowPopOut(ShellViewModel shell)
    {
        if (_window is not null)
        {
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.WindowState = WindowState.Normal;
            }

            _window.Activate();
            return;
        }

        var window = new GuideWindow(shell);
        _window = window;
        var owner = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w is ShellWindow && w.IsVisible)
            ?? Application.Current?.MainWindow;
        if (owner is not null && !ReferenceEquals(owner, window))
        {
            window.Owner = owner;
        }

        window.Closed += (_, _) =>
        {
            _window = null;
            if (!_closing)
            {
                ClosedByUser?.Invoke();
            }
        };
        window.Show();
    }

    public void ClosePopOut()
    {
        var window = _window;
        if (window is null)
        {
            return;
        }

        _closing = true;
        try
        {
            if (window.Dispatcher.CheckAccess())
            {
                window.Close();
            }
            else
            {
                window.Dispatcher.Invoke(window.Close);
            }
        }
        finally
        {
            _closing = false;
            _window = null;
        }
    }
}
