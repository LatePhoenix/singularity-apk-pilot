using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Installer.App.Controls;

public partial class DeviceIllustration : UserControl
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(DeviceIllustrationKind),
        typeof(DeviceIllustration),
        new PropertyMetadata(DeviceIllustrationKind.Cable, OnKindChanged));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description),
        typeof(string),
        typeof(DeviceIllustration),
        new PropertyMetadata("", OnDescriptionChanged));

    public DeviceIllustration()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyKind();
    }

    public DeviceIllustrationKind Kind
    {
        get => (DeviceIllustrationKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DeviceIllustration)d).ApplyKind();
    }

    private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((DeviceIllustration)d).UpdateAutomation();
    }

    private void ApplyKind()
    {
        if (CableCanvas is null)
        {
            return;
        }

        CableCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Cable);
        HeadsetCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Headset);
        PhoneCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Phone);
        HeadsetPromptCanvas.Visibility = VisibleIf(DeviceIllustrationKind.HeadsetPrompt);
        PhonePromptCanvas.Visibility = VisibleIf(DeviceIllustrationKind.PhonePrompt);
        DeveloperModeCanvas.Visibility = VisibleIf(DeviceIllustrationKind.DeveloperMode);
        PackageCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Package);
        InstallingCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Installing);
        ProblemCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Problem);
        CompleteCanvas.Visibility = VisibleIf(DeviceIllustrationKind.Complete);
        UpdateAutomation();
    }

    private Visibility VisibleIf(DeviceIllustrationKind kind) =>
        Kind == kind ? Visibility.Visible : Visibility.Collapsed;

    private void UpdateAutomation()
    {
        AutomationProperties.SetName(this, string.IsNullOrWhiteSpace(Description) ? Kind.ToString() : Description);
    }
}
