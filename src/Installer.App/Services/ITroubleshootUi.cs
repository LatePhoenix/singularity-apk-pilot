using Installer.App.ViewModels;

namespace Installer.App.Services;

public interface ITroubleshootUi
{
    void ShowDialog(ShellViewModel shell);

    void Close();
}
