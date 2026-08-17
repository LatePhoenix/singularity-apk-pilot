using Installer.Core.Models;

namespace Installer.Core.Abstractions;

public interface IRecentsStore
{
    RecentsState Load();
    void Save(RecentsState state);
}
