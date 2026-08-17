using CommunityToolkit.Mvvm.ComponentModel;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class WelcomePageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasUpdate))]
    [NotifyPropertyChangedFor(nameof(UpdateUri))]
    private string updateMessage = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateUri))]
    private string updateUrl = "https://github.com/LatePhoenix/singularity-apk-installer/releases/latest/download/SingularityApkInstaller-win-x64-setup.exe";

    public bool HasUpdate => !string.IsNullOrWhiteSpace(UpdateMessage);

    public Uri? UpdateUri => Uri.TryCreate(UpdateUrl, UriKind.Absolute, out var uri) ? uri : null;
}
