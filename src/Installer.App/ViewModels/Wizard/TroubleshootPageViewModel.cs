using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Installer.Core.Models;

namespace Installer.App.ViewModels.Wizard;

public sealed partial class TroubleshootPageViewModel : WizardPageViewModel
{
    [ObservableProperty]
    private bool showFamilyPicker;

    [ObservableProperty]
    private bool looksLikeQuest;

    [ObservableProperty]
    private bool canGoBack;

    [ObservableProperty]
    private bool showInPageAction;

    [ObservableProperty]
    private string inPageActionLabel = "";

    [ObservableProperty]
    private string actionStatus = "";

    [ObservableProperty]
    private bool actionBusy;

    public IReadOnlyList<GuideStep> Steps { get; private set; } = [];

    public TroubleshootActionKind ActionKind { get; private set; }

    public TroubleshootFamily Family { get; private set; }

    public event Action<TroubleshootFamily>? FamilySelected;

    public event Action? BackRequested;

    public event Action<TroubleshootActionKind>? ActionRequested;

    public event Action<TroubleshootFamily>? SwitchToQuestRequested;

    protected override void OnApplied(WizardState state)
    {
        var session = state.Troubleshoot;
        ShowFamilyPicker = session?.ShowFamilyPicker != false;
        LooksLikeQuest = session?.LooksLikeQuest == true;
        CanGoBack = session?.CanGoBack == true;
        Family = session?.Family ?? TroubleshootFamily.Unknown;
        ActionKind = session?.RecommendedAction ?? TroubleshootActionKind.None;
        InPageActionLabel = session?.InPageActionLabel ?? "";
        ShowInPageAction = ActionKind != TroubleshootActionKind.None && !string.IsNullOrWhiteSpace(InPageActionLabel);
        Steps = (session?.GuideSteps ?? [])
            .Select((text, index) => new GuideStep((index + 1).ToString(), text))
            .ToList();
        OnPropertyChanged(nameof(Steps));
        if (!ActionBusy)
        {
            ActionStatus = "";
        }
    }

    public void SetActionLabel(string label)
    {
        InPageActionLabel = label;
        ShowInPageAction = ActionKind != TroubleshootActionKind.None && !string.IsNullOrWhiteSpace(label);
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
        if (ActionKind != TroubleshootActionKind.None && !ActionBusy)
        {
            ActionRequested?.Invoke(ActionKind);
        }
    }
}
