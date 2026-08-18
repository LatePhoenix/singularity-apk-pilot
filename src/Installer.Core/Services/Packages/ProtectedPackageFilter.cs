using Installer.Core.Services.Adb;

namespace Installer.Core.Services.Packages;

public static class ProtectedPackageFilter
{
    private static readonly string[] Exact =
    [
        "android",
        "com.android.systemui",
        "com.oculus.vrshell",
        "com.oculus.systemux",
        "com.oculus.guardian",
        "com.oculus.os.vrlockscreen"
    ];

    private static readonly string[] Prefixes =
    [
        "android.",
        "com.android.",
        "com.google.android.",
        "com.oculus.os.",
        "com.oculus.system",
        "com.qualcomm.",
        "com.meta.horizon"
    ];

    public static bool IsProtected(string? packageId)
    {
        if (!AdbOutputParser.IsSafePackageId(packageId))
        {
            return true;
        }

        foreach (var exact in Exact)
        {
            if (packageId!.Equals(exact, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        foreach (var prefix in Prefixes)
        {
            if (packageId!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
