using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class InstallProblemPageViewModel : WizardPageViewModel
{
    public ObservableCollection<RecoveryAction> Actions { get; } = [];

    [ObservableProperty]
    private string errorDetail = "";

    protected override void OnApplied(WizardState state)
    {
        Actions.Clear();
        foreach (var action in state.SuggestedActions)
        {
            Actions.Add(action);
        }

        ErrorDetail = state.LastInstallResult?.RawOutput ?? "";
    }
}
