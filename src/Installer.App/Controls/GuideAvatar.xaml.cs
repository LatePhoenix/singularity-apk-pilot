using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
        if (Resources["AvatarStroke"] is not SolidColorBrush stroke)
        {
            return;
        }

        var key = Mood switch
        {
            "Warn" => "WarningTextBrush",
            "Done" => "AccentGreenBrush",
            _ => "BrandCyanBrush"
        };
        if (TryFindResource(key) is SolidColorBrush source)
        {
            stroke.Color = source.Color;
        }
    }
}
