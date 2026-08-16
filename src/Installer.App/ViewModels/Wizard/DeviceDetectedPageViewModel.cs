using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class DeviceDetectedPageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    private string deviceSummary = "";

    protected override void OnApplied(WizardState state)
    {
        DeviceSummary = state.Device is null
            ? ""
            : $"{state.Device.DisplayName} · {state.Device.Kind}";
    }
}
