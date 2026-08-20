using System.Windows.Controls;

namespace Installer.App.Controls;

public partial class GuideCompanion : UserControl
{
    public GuideCompanion()
    {
        InitializeComponent();
    }

    public void ToggleHelp()
    {
        HelpExpander.IsExpanded = !HelpExpander.IsExpanded;
        if (HelpExpander.IsExpanded)
        {
            HelpExpander.Focus();
        }
    }
}
