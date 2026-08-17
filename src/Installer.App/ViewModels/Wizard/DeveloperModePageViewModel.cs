using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class DeveloperModePageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    private string healthHint = "";

    protected override void OnApplied(WizardState state)
    {
        HealthHint = state.Health?.Hint ?? "";
    }
}
