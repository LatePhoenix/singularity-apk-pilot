using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class CompletePageViewModel : WizardPageViewModel
{
    public IReadOnlyList<string> Notes { get; private set; } = [];

    [ObservableProperty]
    private bool canOpen;

    [ObservableProperty]
    private string openLabel = "Open on device";

    public event Action? OpenRequested;

    protected override void OnApplied(WizardState state)
    {
        var key = state.Device?.IsQuest == true ? "quest" : "android";
        Notes = state.Manifest.PostInstallNotes.TryGetValue(key, out var notes) ? notes : [];
        CanOpen = state.Manifest.CanVerifyPackage && state.Device?.State == DeviceConnectionState.ConnectedReady;
        OpenLabel = state.Manifest.CanVerifyPackage ? $"Open {state.Manifest.DisplayName}" : "Open on device";
        OnPropertyChanged(nameof(Notes));
    }

    [RelayCommand]
    private void OpenOnDevice()
    {
        if (CanOpen)
        {
            OpenRequested?.Invoke();
        }
    }
}
