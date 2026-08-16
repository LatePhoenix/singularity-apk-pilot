using System.Windows;
using System.Windows.Input;
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
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F1)
        {
            HelpExpander.IsExpanded = !HelpExpander.IsExpanded;
            if (HelpExpander.IsExpanded)
            {
                HelpExpander.Focus();
            }

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
}
