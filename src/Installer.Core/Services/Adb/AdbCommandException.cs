using Installer.Core.Models;

namespace Installer.Core.Services.Adb;

/// <summary>adb ran but could not answer, so its output must not be read as "nothing found".</summary>
public sealed class AdbCommandException : Exception
{
    public AdbCommandException(string message, AdbProcessResult result)
        : base(message)
    {
        Result = result;
    }

    public AdbProcessResult Result { get; }
}
