using System.Text.Json;
using Installer.Core.Abstractions;
using Installer.Core.Utilities;

namespace Installer.Infrastructure.Storage;

public sealed class ReportRecipientStore : IReportRecipientStore
{
    private readonly IUserDataPaths _paths;
    private readonly IAppLogger _logger;

    public ReportRecipientStore(IUserDataPaths paths, IAppLogger logger)
    {
        _paths = paths;
        _logger = logger;
    }

    public string? Load()
    {
        try
        {
            var path = _paths.ReportRecipientPath;
            if (!File.Exists(path))
            {
                return null;
            }

            var dto = JsonSerializer.Deserialize<StoredRecipient>(File.ReadAllText(path), JsonDefaults.Manifest);
            return EmailAddress.TryNormalize(dto?.Email, out var email) ? email : null;
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not load saved report email: {ex.Message}");
            return null;
        }
    }

    public void Save(string email)
    {
        if (!EmailAddress.TryNormalize(email, out var normalized))
        {
            return;
        }

        try
        {
            var path = _paths.ReportRecipientPath;
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonSerializer.Serialize(new StoredRecipient(normalized), JsonDefaults.Manifest));
        }
        catch (Exception ex)
        {
            _logger.Warn($"Could not save report email: {ex.Message}");
        }
    }

    private sealed record StoredRecipient(string Email);
}
