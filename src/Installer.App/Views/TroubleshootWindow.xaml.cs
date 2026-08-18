using System.Windows;
using Installer.App.ViewModels;

namespace Installer.App.Views;

public partial class TroubleshootWindow : Window
{
    public TroubleshootWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) =>
        {
            if (PrimaryButton.IsVisible && PrimaryButton.IsEnabled)
            {
                PrimaryButton.Focus();
            }
        };
    }
}
