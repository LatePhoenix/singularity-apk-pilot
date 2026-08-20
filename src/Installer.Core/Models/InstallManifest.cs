namespace Installer.Core.Models;

public sealed record SupportContact(string? ContactLabel, string? ContactEmail);

public sealed record InstallManifest(
    string AppId,
    string DisplayName,
    string BuildVersion,
    string ApkPath,
    IReadOnlyList<string> TargetPlatforms,
    InstallPolicy InstallPolicy,
    bool GrantPermissions,
    bool AllowTestApk,
    bool LaunchAfterInstall,
    IReadOnlyList<string> PreferredDeviceFamilies,
    IReadOnlyDictionary<string, IReadOnlyList<string>> PostInstallNotes,
    SupportContact? Support)
{
    public const string UserSelectedAppId = "user.selected";

    public bool CanVerifyPackage =>
        AppId.Contains('.', StringComparison.Ordinal)
        && !string.Equals(AppId, UserSelectedAppId, StringComparison.OrdinalIgnoreCase);

    public static InstallManifest Session { get; } = new(
        AppId: UserSelectedAppId,
        DisplayName: "apps",
        BuildVersion: "",
        ApkPath: "",
        TargetPlatforms: ["quest", "android"],
        InstallPolicy: InstallPolicy.ReinstallAllowDowngrade,
        GrantPermissions: true,
        AllowTestApk: true,
        LaunchAfterInstall: false,
        PreferredDeviceFamilies: ["meta-quest-2", "meta-quest-3", "meta-quest-3s", "meta-quest-pro", "pixel", "samsung"],
        PostInstallNotes: new Dictionary<string, IReadOnlyList<string>>
        {
            ["quest"] = ["Open Library.", "Open the filter menu.", "Select Unknown Sources.", "Find the app you installed."],
            ["android"] = ["Find the app in your app drawer and open it."]
        },
        Support: new SupportContact("Send diagnostics to support", "support@example.com"));

    public static InstallManifest Placeholder { get; } = Session with
    {
        AppId = "com.singularity.exampleapp",
        DisplayName = "Example App",
        BuildVersion = "0.0.0",
        ApkPath = "payloads/current/example-app.apk",
        PostInstallNotes = new Dictionary<string, IReadOnlyList<string>>
        {
            ["quest"] = ["Open Library.", "Open the filter menu.", "Select Unknown Sources.", "Launch Example App."],
            ["android"] = ["Find Example App in your app drawer and open it."]
        }
    };

    public static InstallManifest ForSelectedApk(string apkPath, InstallManifest policy) =>
        policy with
        {
            AppId = UserSelectedAppId,
            DisplayName = Path.GetFileName(apkPath),
            BuildVersion = "",
            ApkPath = apkPath
        };

    public static InstallManifest ForSelectedApks(IReadOnlyList<string> apkPaths, InstallManifest policy)
    {
        if (apkPaths.Count == 1)
        {
            return ForSelectedApk(apkPaths[0], policy);
        }

        return policy with
        {
            AppId = UserSelectedAppId,
            DisplayName = $"{apkPaths.Count} apps",
            BuildVersion = "",
            ApkPath = apkPaths[0]
        };
    }

    public static InstallManifest ForInstallSet(InstallSet set, InstallManifest policy) =>
        policy with
        {
            AppId = set.CanVerify ? set.PackageId : UserSelectedAppId,
            DisplayName = set.DisplayName,
            BuildVersion = set.VersionName,
            ApkPath = set.PrimaryPath,
            LaunchAfterInstall = set.CanVerify
        };
}
