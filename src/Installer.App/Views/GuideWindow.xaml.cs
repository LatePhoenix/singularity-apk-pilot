using System.Windows;
using Installer.App.ViewModels;

namespace Installer.App.Views;

public partial class GuideWindow : Window
{
    public GuideWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    public void ToggleHelp() => Companion.ToggleHelp();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Owner is null)
        {
            return;
        }

        Owner.LocationChanged += OwnerMoved;
        Owner.SizeChanged += OwnerMoved;
        SnapToOwner();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        if (Owner is null)
        {
            return;
        }

        Owner.LocationChanged -= OwnerMoved;
        Owner.SizeChanged -= OwnerMoved;
    }

    private void OwnerMoved(object? sender, EventArgs e) => SnapToOwner();

    private void SnapToOwner()
    {
        if (Owner is null)
        {
            return;
        }

        Height = Math.Max(MinHeight, Owner.ActualHeight);
        var right = Owner.Left + Owner.ActualWidth + 8;
        var wa = SystemParameters.WorkArea;
        Left = right + Width > wa.Right ? Math.Max(wa.Left, Owner.Left - Width - 8) : right;
        Top = Math.Clamp(Owner.Top, wa.Top, Math.Max(wa.Top, wa.Bottom - Height));
    }
}
