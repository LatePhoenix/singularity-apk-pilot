using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using Installer.App.ViewModels;

namespace Installer.App.Views;

public partial class SendReportWindow : Window
{
    private readonly SendReportViewModel _viewModel;

    public SendReportWindow(SendReportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.OwnerHandle = new WindowInteropHelper(this).Handle;
        EmailBox.Focus();
        EmailBox.SelectAll();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_viewModel.IsBusy)
        {
            e.Cancel = true;
        }
    }
}
