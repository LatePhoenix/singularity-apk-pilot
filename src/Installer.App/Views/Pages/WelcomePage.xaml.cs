using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Installer.App.Views.Pages;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private void OnUpdateNavigate(object sender, RequestNavigateEventArgs e)
    {
        if (e.Uri is not null)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        }

        e.Handled = true;
    }
}
