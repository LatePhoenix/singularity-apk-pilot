using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class InstallProblemPageViewModel : WizardPageViewModel
{
    public ObservableCollection<RecoveryAction> Actions { get; } = [];

    [ObservableProperty]
    private string errorDetail = "";

    [ObservableProperty]
    private bool showReplace;

    [ObservableProperty]
    private bool showRemove;

    public event Action<InstallPolicy>? PolicyRetryRequested;

    protected override void OnApplied(WizardState state)
    {
        Actions.Clear();
        foreach (var action in state.SuggestedActions)
        {
            Actions.Add(action);
        }

        ErrorDetail = state.LastInstallResult?.RawOutput ?? "";
        var error = state.LastInstallResult?.Error;
        ShowReplace = error is InstallError.PackageAlreadyExists or InstallError.VersionDowngrade or InstallError.SignatureMismatch;
        ShowRemove = error is InstallError.PackageAlreadyExists or InstallError.VersionDowngrade or InstallError.SignatureMismatch;
    }

    [RelayCommand]
    private void ReplaceThisApp()
    {
        if (ShowReplace)
        {
            PolicyRetryRequested?.Invoke(InstallPolicy.ReinstallAllowDowngrade);
        }
    }

    [RelayCommand]
    private void RemoveThisApp()
    {
        if (ShowRemove)
        {
            PolicyRetryRequested?.Invoke(InstallPolicy.UninstallThenInstall);
        }
    }
}
