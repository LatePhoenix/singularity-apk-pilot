using System.Security.Cryptography;
using Installer.Infrastructure.Storage;

namespace Installer.Infrastructure.Logging;

public static class HmacKeyStore
{
    public static byte[] GetOrCreate()
    {
        Directory.CreateDirectory(AppDataPaths.Root);
        var path = Path.Combine(AppDataPaths.Root, "diagnostic-hmac.key");
        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length >= 16)
            {
                return existing;
            }
        }

        var key = RandomNumberGenerator.GetBytes(32);
        File.WriteAllBytes(path, key);
        return key;
    }
}
