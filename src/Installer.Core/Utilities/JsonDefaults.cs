using System.Text.Json;

namespace Installer.Core.Utilities;

public static class JsonDefaults
{
    public static JsonSerializerOptions Manifest { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };
}
