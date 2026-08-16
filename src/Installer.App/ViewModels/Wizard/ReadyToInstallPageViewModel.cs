using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class ReadyToInstallPageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    private string summary = "";

    protected override void OnApplied(WizardState state)
    {
        Summary = $"{state.Manifest.DisplayName} {state.Manifest.BuildVersion} → {state.Device?.DisplayName}";
    }
}
