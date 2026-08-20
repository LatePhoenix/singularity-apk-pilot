using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Windows.Threading;
using Installer.App.ViewModels;

namespace Installer.App.Views;

public partial class ShellWindow : Window
{
    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(ShellViewModel.CurrentPage)
                or nameof(ShellViewModel.ShowPrimary)
                or nameof(ShellViewModel.ShowCancel))
            {
                Dispatcher.BeginInvoke(FocusPrimaryAction, DispatcherPriority.Input);
            }
        };
        Loaded += (_, _) => FocusPrimaryAction();
        Closed += (_, _) => viewModel.Shutdown();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F1 && DataContext is ShellViewModel viewModel)
        {
            if (!viewModel.IsGuideVisible)
            {
                viewModel.ShowGuideCommand.Execute(null);
            }

            viewModel.IsGuideHelpExpanded = !viewModel.IsGuideHelpExpanded;
            e.Handled = true;
        }

        base.OnPreviewKeyDown(e);
    }

    private void FocusPrimaryAction()
    {
        if (PrimaryButton.IsVisible && PrimaryButton.IsEnabled)
        {
            PrimaryButton.Focus();
            return;
        }

        if (CancelButton.IsVisible)
        {
            CancelButton.Focus();
        }
    }

    private void OnLegalNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
