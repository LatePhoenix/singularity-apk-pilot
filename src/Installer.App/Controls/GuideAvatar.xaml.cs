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
        var key = Mood switch
        {
            "Warn" => "WarningTextBrush",
            "Done" => "AccentGreenBrush",
            _ => "BrandCyanBrush"
        };
        var color = Color.FromRgb(0x2F, 0xA8, 0xC8);
        if (TryFindResource(key) is SolidColorBrush source)
        {
            color = source.Color;
        }

        Resources["AvatarStroke"] = new SolidColorBrush(color);
    }
}
