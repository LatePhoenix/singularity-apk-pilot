using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Installer.Core.Models;

namespace Installer.Core.Utilities;

public sealed class LogRedactor
{
    private static readonly Regex PairCommand = new(
        @"\bpair\s+(\S+)\s+\S+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private readonly byte[] _hmacKey;

    public LogRedactor(byte[] hmacKey)
    {
        _hmacKey = hmacKey.Length >= 16 ? hmacKey : SHA256.HashData(hmacKey);
    }

    public static LogRedactor ForTests() => new(Encoding.UTF8.GetBytes("sai-test-hmac-key-32-bytes-long!"));

    public string HashSerial(string? serial)
    {
        if (string.IsNullOrWhiteSpace(serial))
        {
            return "";
        }

        var hash = HMACSHA256.HashData(_hmacKey, Encoding.UTF8.GetBytes(serial));
        return Convert.ToHexString(hash)[..16];
    }

    public string RedactCommand(AdbCommand command) => RedactArguments(command.Arguments);

    public string RedactArguments(IReadOnlyList<string> arguments)
    {
        var copy = new string[arguments.Count];
        for (var i = 0; i < arguments.Count; i++)
        {
            copy[i] = arguments[i];
        }

        for (var i = 0; i < copy.Length; i++)
        {
            if (copy[i].Equals("pair", StringComparison.OrdinalIgnoreCase) && i + 2 < copy.Length)
            {
                copy[i + 2] = "***";
            }

            if (copy[i].Equals("-s", StringComparison.OrdinalIgnoreCase) && i + 1 < copy.Length)
            {
                copy[i + 1] = HashSerial(copy[i + 1]);
            }
        }

        return string.Join(' ', copy.Select(QuoteAndRedactPath));
    }

    public string RedactText(string? text, params string?[] serials)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? "";
        }

        var result = text;
        foreach (var serial in serials)
        {
            if (string.IsNullOrWhiteSpace(serial))
            {
                continue;
            }

            result = result.Replace(serial, HashSerial(serial), StringComparison.OrdinalIgnoreCase);
        }

        result = PairCommand.Replace(result, "pair $1 ***");
        return RedactUserPaths(result);
    }

    private static string QuoteAndRedactPath(string value)
    {
        var redacted = RedactUserPaths(value);
        return redacted.Contains(' ', StringComparison.Ordinal) ? $"\"{redacted}\"" : redacted;
    }

    private static string RedactUserPaths(string text)
    {
        var result = text;
        ReplaceFolder(ref result, Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "%USERPROFILE%");
        ReplaceFolder(ref result, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "%LOCALAPPDATA%");
        return result;
    }

    private static void ReplaceFolder(ref string text, string folder, string token)
    {
        if (string.IsNullOrWhiteSpace(folder) || folder.Length < 4)
        {
            return;
        }

        text = text.Replace(folder, token, StringComparison.OrdinalIgnoreCase);
    }
}
