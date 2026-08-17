using System.Text.Json;
using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Utilities;

namespace Installer.Infrastructure.Storage;

public sealed class RecentsStore : IRecentsStore
{
    private readonly IUserDataPaths _paths;
    private readonly IAppLogger _logger;

    public RecentsStore(IUserDataPaths paths, IAppLogger logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public RecentsState Load()
    {
        try
        {
            var path = _paths.RecentsPath;
            if (!File.Exists(path))
            {
                return RecentsState.Empty;
            }

            var dto = JsonSerializer.Deserialize<Stored>(File.ReadAllText(path), JsonDefaults.Manifest);
            var files = (dto?.Files ?? [])
                .Where(file => !string.IsNullOrWhiteSpace(file) && File.Exists(file))
                .ToList();
            var folder = !string.IsNullOrWhiteSpace(dto?.Folder) && Directory.Exists(dto.Folder) ? dto.Folder : files.Count > 0 ? Path.GetDirectoryName(files[0]) : null;
            return new RecentsState(folder, files);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not load last files: {ex.Message}");
            return RecentsState.Empty;
        }
    }

    public void Save(RecentsState state)
    {
        try
        {
            var path = _paths.RecentsPath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(new Stored(state.LastFolder, state.LastFiles.ToList()), JsonDefaults.Manifest);
            File.WriteAllText(path, json);
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not save last files: {ex.Message}");
        }
    }

    private sealed record Stored(string? Folder, List<string>? Files);
}
