using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class TroubleshootPageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryAction))]
    [NotifyPropertyChangedFor(nameof(PrimaryRunsAction))]
    [NotifyPropertyChangedFor(nameof(ShowConfirmInstalled))]
    private bool showFamilyPicker;

    [ObservableProperty]
    private bool looksLikeQuest;

    [ObservableProperty]
    private bool canGoBack;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PrimaryAction))]
    [NotifyPropertyChangedFor(nameof(PrimaryRunsAction))]
    [NotifyPropertyChangedFor(nameof(ShowConfirmInstalled))]
    private string inPageActionLabel = "";

    [ObservableProperty]
    private string actionStatus = "";

    [ObservableProperty]
    private bool actionBusy;

    public IReadOnlyList<GuideStep> Steps { get; private set; } = [];

    public TroubleshootActionKind ActionKind { get; private set; }

    public TroubleshootFamily Family { get; private set; }

    public bool PrimaryRunsAction =>
        ActionKind is TroubleshootActionKind.RestartAdbServer
            or TroubleshootActionKind.InstallUsbHelper
            or TroubleshootActionKind.OpenDriverDownload
            or TroubleshootActionKind.OpenPhoneUsbSupport
        && !string.IsNullOrWhiteSpace(InPageActionLabel);

    public bool ShowConfirmInstalled =>
        ActionKind is TroubleshootActionKind.InstallUsbHelper
            or TroubleshootActionKind.OpenDriverDownload
            or TroubleshootActionKind.OpenPhoneUsbSupport;

    public override string PrimaryAction =>
        PrimaryRunsAction ? InPageActionLabel : Copy.PrimaryAction;

    public event Action<TroubleshootFamily>? FamilySelected;

    public event Action? BackRequested;

    public event Action<TroubleshootActionKind>? ActionRequested;

    public event Action<TroubleshootFamily>? SwitchToQuestRequested;

    public event Action? ConfirmInstalledRequested;

    protected override void OnApplied(WizardState state)
    {
        var session = state.Troubleshoot;
        ShowFamilyPicker = session?.ShowFamilyPicker != false;
        LooksLikeQuest = session?.LooksLikeQuest == true;
        CanGoBack = session?.CanGoBack == true;
        Family = session?.Family ?? TroubleshootFamily.Unknown;
        ActionKind = session?.RecommendedAction ?? TroubleshootActionKind.None;
        InPageActionLabel = session?.InPageActionLabel ?? "";
        Steps = (session?.GuideSteps ?? [])
            .Select((text, index) => new GuideStep((index + 1).ToString(), text))
            .ToList();
        OnPropertyChanged(nameof(Steps));
        OnPropertyChanged(nameof(PrimaryRunsAction));
        OnPropertyChanged(nameof(ShowConfirmInstalled));
        OnPropertyChanged(nameof(PrimaryAction));
        if (!ActionBusy)
        {
            ActionStatus = "";
        }
    }

    public void SetActionLabel(string label)
    {
        InPageActionLabel = label;
        OnPropertyChanged(nameof(PrimaryRunsAction));
        OnPropertyChanged(nameof(ShowConfirmInstalled));
        OnPropertyChanged(nameof(PrimaryAction));
    }

    [RelayCommand]
    private void ChooseQuest() => FamilySelected?.Invoke(TroubleshootFamily.MetaQuest);

    [RelayCommand]
    private void ChoosePhone() => FamilySelected?.Invoke(TroubleshootFamily.AndroidPhone);

    [RelayCommand]
    private void SwitchToQuest() => SwitchToQuestRequested?.Invoke(TroubleshootFamily.MetaQuest);

    [RelayCommand]
    private void Back()
    {
        if (CanGoBack)
        {
            BackRequested?.Invoke();
        }
    }

    [RelayCommand]
    private void RunAction()
    {
        if (PrimaryRunsAction && !ActionBusy)
        {
            ActionRequested?.Invoke(ActionKind);
        }
    }

    [RelayCommand]
    private void ConfirmInstalled() => ConfirmInstalledRequested?.Invoke();
}
