using CommunityToolkit.Mvvm.ComponentModel;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class CompletePageViewModel : WizardPageViewModel
{
    public IReadOnlyList<string> Notes { get; private set; } = [];

    protected override void OnApplied(WizardState state)
    {
        var key = state.Device?.IsQuest == true ? "quest" : "android";
        Notes = state.Manifest.PostInstallNotes.TryGetValue(key, out var notes) ? notes : [];
        OnPropertyChanged(nameof(Notes));
    }
}
