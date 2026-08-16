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
    public static InstallManifest Placeholder { get; } = new(
        AppId: "com.singularity.exampleapp",
        DisplayName: "Example App",
        BuildVersion: "0.0.0",
        ApkPath: "payloads/current/example-app.apk",
        TargetPlatforms: ["quest", "android"],
        InstallPolicy: InstallPolicy.ReinstallAllowDowngrade,
        GrantPermissions: true,
        AllowTestApk: true,
        LaunchAfterInstall: false,
        PreferredDeviceFamilies: ["meta-quest-2", "meta-quest-3", "pixel", "samsung"],
        PostInstallNotes: new Dictionary<string, IReadOnlyList<string>>
        {
            ["quest"] = ["Open Library.", "Open the filter menu.", "Select Unknown Sources.", "Launch Example App."],
            ["android"] = ["Find Example App in your app drawer and open it."]
        },
        Support: new SupportContact("Send diagnostics to support", "support@example.com"));
}
