using System.Windows;
using System.Windows.Controls;

namespace Installer.App.Controls;

public partial class GuideAvatar : UserControl
{
    public static readonly DependencyProperty MoodProperty = DependencyProperty.Register(
        nameof(Mood),
        typeof(string),
        typeof(GuideAvatar),
        new PropertyMetadata("Calm", OnMoodChanged));

    public GuideAvatar()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyMood();
    }

    public string Mood
    {
        get => (string)GetValue(MoodProperty);
        set => SetValue(MoodProperty, value);
    }

    private static void OnMoodChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((GuideAvatar)d).ApplyMood();
    }

    private void ApplyMood()
    {
        if (CalmCanvas is null)
        {
            return;
        }

        CalmCanvas.Visibility = VisibleIf("Calm");
        WaitCanvas.Visibility = VisibleIf("Wait");
        WorkCanvas.Visibility = VisibleIf("Work");
        WarnCanvas.Visibility = VisibleIf("Warn");
        DoneCanvas.Visibility = VisibleIf("Done");
        if (CalmCanvas.Visibility == Visibility.Collapsed
            && WaitCanvas.Visibility == Visibility.Collapsed
            && WorkCanvas.Visibility == Visibility.Collapsed
            && WarnCanvas.Visibility == Visibility.Collapsed
            && DoneCanvas.Visibility == Visibility.Collapsed)
        {
            CalmCanvas.Visibility = Visibility.Visible;
        }
    }

    private Visibility VisibleIf(string mood) =>
        string.Equals(Mood, mood, StringComparison.OrdinalIgnoreCase)
            ? Visibility.Visible
            : Visibility.Collapsed;
}
