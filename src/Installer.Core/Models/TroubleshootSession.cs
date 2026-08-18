namespace Installer.Core.Models;

public sealed record TroubleshootSession(
    TroubleshootFamily Family,
    TroubleshootNode CurrentNode,
    UsbEvidence Evidence,
    WizardStep ReturnStep,
    DeviceInfo? Device,
    IReadOnlyList<TroubleshootNode> History,
    TroubleshootActionKind RecommendedAction,
    string StatusChip,
    string StatusTone,
    IReadOnlyList<string> GuideSteps,
    string InPageActionLabel,
    bool LooksLikeQuest)
{
    public bool ShowFamilyPicker => CurrentNode == TroubleshootNode.PickDevice || Family == TroubleshootFamily.Unknown;

    public bool CanGoBack => History.Count > 0 || CurrentNode != TroubleshootNode.PickDevice;
}
