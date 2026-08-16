using CommunityToolkit.Mvvm.ComponentModel;
using Installer.App.Controls;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public abstract partial class WizardPageViewModel : ObservableObject
{
    [ObservableProperty]
    private WizardCopy copy = new("", "", "", "", "");

    [ObservableProperty]
    private string advancedDetails = "";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private DeviceIllustrationKind illustration = DeviceIllustrationKind.Cable;

    [ObservableProperty]
    private string illustrationDescription = "";

    [ObservableProperty]
    private string statusText = "";

    [ObservableProperty]
    private string statusTone = "Idle";

    public string Headline => Copy.Headline;
    public string Body => Copy.Body;
    public string PrimaryAction => Copy.PrimaryAction;
    public string Help => Copy.Help;
    public bool ShowStatus => !string.IsNullOrWhiteSpace(StatusText);

    public void Apply(WizardState state)
    {
        Copy = state.Copy;
        AdvancedDetails = state.Copy.Advanced;
        IsBusy = state.IsBusy;
        (Illustration, IllustrationDescription, StatusText, StatusTone) = ResolveChrome(state);
        OnApplied(state);
        OnPropertyChanged(nameof(Headline));
        OnPropertyChanged(nameof(Body));
        OnPropertyChanged(nameof(PrimaryAction));
        OnPropertyChanged(nameof(Help));
        OnPropertyChanged(nameof(ShowStatus));
    }

    protected virtual void OnApplied(WizardState state)
    {
    }

    private static (DeviceIllustrationKind kind, string description, string status, string tone) ResolveChrome(WizardState state)
    {
        var quest = state.Device?.IsQuest == true || state.Device?.Kind == DeviceKind.MetaQuest;
        return state.CurrentStep switch
        {
            WizardStep.Welcome or WizardStep.ConnectDevice =>
                (DeviceIllustrationKind.Cable, "Computer connected to a headset or phone with a USB-C cable.", "", "Idle"),
            WizardStep.DeviceDetected when quest =>
                (DeviceIllustrationKind.Headset, $"{state.Device?.DisplayName} headset detected.", "Headset connected", "Live"),
            WizardStep.DeviceDetected =>
                (DeviceIllustrationKind.Phone, $"{state.Device?.DisplayName} phone detected.", "Phone connected", "Live"),
            WizardStep.Authorization when quest =>
                (DeviceIllustrationKind.HeadsetPrompt, "USB debugging prompt waiting inside the headset.", "Waiting for allow", "Idle"),
            WizardStep.Authorization =>
                (DeviceIllustrationKind.PhonePrompt, "USB debugging prompt waiting on the phone.", "Waiting for allow", "Idle"),
            WizardStep.DeveloperMode =>
                (DeviceIllustrationKind.DeveloperMode, "Turn on developer mode in the Meta Horizon phone app, then reconnect the headset.", "", "Idle"),
            WizardStep.ReadyToInstall =>
                (DeviceIllustrationKind.Package, $"Ready to install {state.Manifest.DisplayName} on {state.Device?.DisplayName}.", "Ready", "Live"),
            WizardStep.Installing =>
                (DeviceIllustrationKind.Installing, $"Installing {state.Manifest.DisplayName}. Keep the cable connected.", state.StatusMessage ?? "Installing", "Live"),
            WizardStep.InstallProblem =>
                (DeviceIllustrationKind.Problem, state.Copy.Headline, "Install did not finish", "Warning"),
            WizardStep.Complete =>
                (DeviceIllustrationKind.Complete, $"{state.Manifest.DisplayName} is installed.", "Installed", "Live"),
            _ => (DeviceIllustrationKind.Cable, "", "", "Idle")
        };
    }
}
