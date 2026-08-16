using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class InstallingPageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    private string statusLabel = "Starting";

    protected override void OnApplied(WizardState state)
    {
        StatusLabel = string.IsNullOrWhiteSpace(state.StatusMessage) ? "Installing" : state.StatusMessage;
    }
}
