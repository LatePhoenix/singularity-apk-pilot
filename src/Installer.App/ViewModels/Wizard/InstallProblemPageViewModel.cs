using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class InstallProblemPageViewModel : WizardPageViewModel
{
    public ObservableCollection<RecoveryAction> Actions { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasMoreActions))]
    private RecoveryAction? primaryRecovery;

    [ObservableProperty]
    private string errorDetail = "";

    public override string PrimaryAction => PrimaryRecovery?.Title ?? Copy.PrimaryAction;

    public bool HasMoreActions => Actions.Count > 0;

    public event Action<RecoveryAction>? ActionRequested;

    protected override void OnApplied(WizardState state)
    {
        Actions.Clear();
        RecoveryAction? primary = null;
        foreach (var action in state.SuggestedActions)
        {
            if (action.Kind == RecoveryActionKind.ExportDiagnostics)
            {
                continue;
            }

            if (primary is null)
            {
                primary = action;
                continue;
            }

            Actions.Add(action);
        }

        PrimaryRecovery = primary;
        ErrorDetail = state.LastInstallResult?.RawOutput ?? "";
        OnPropertyChanged(nameof(PrimaryAction));
        OnPropertyChanged(nameof(HasMoreActions));
    }

    [RelayCommand]
    private void RunAction(RecoveryAction? action)
    {
        if (action is not null)
        {
            ActionRequested?.Invoke(action);
        }
    }
}
