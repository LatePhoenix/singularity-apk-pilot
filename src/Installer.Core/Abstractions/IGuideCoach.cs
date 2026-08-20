using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IGuideCoach
{
    GuideScript For(WizardState state, bool hasSelectedFiles = false);
}
