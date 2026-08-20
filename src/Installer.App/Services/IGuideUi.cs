using Installer.App.ViewModels;

namespace Installer.App.Services;

public interface IGuideUi
{
    bool IsOpen { get; }

    void ShowPopOut(ShellViewModel shell);

    void ClosePopOut();

    event Action? ClosedByUser;
}
