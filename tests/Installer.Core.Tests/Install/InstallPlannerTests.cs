using Installer.Core.Models;
using Installer.Core.Services.Install;

namespace Installer.Core.Tests.Install;

public sealed class InstallPlannerTests
{
    private readonly InstallPlanner _planner = new();

    [Theory]
    [InlineData(InstallPolicy.InstallFresh, new string[] { "-t", "-g" }, false)]
    [InlineData(InstallPolicy.ReinstallKeepData, new[] { "-r", "-t", "-g" }, false)]
    [InlineData(InstallPolicy.ReinstallAllowDowngrade, new[] { "-r", "-d", "-t", "-g" }, false)]
    [InlineData(InstallPolicy.UninstallThenInstall, new[] { "-t", "-g" }, true)]
    [InlineData(InstallPolicy.InstallTestBuild, new[] { "-r", "-t", "-g" }, false)]
    public void Builds_flags_from_policy(InstallPolicy policy, string[] expectedFlags, bool uninstallFirst)
    {
        var manifest = InstallManifest.Placeholder with { InstallPolicy = policy, GrantPermissions = true, AllowTestApk = true };
        var plan = _planner.Create(new InstallRequest(manifest, Device("Quest 3", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady)));
        Assert.Equal(expectedFlags, plan.AdbFlags);
        Assert.Equal(uninstallFirst, plan.RequiresUninstallFirst);
        Assert.True(plan.VerifyAfterInstall);
    }

    [Fact]
    public void User_selected_apk_skips_package_verify()
    {
        var manifest = InstallManifest.ForSelectedApk(@"C:\tmp\game.apk", InstallManifest.Session);
        var plan = _planner.Create(new InstallRequest(manifest, Device("Quest 3", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady)));
        Assert.False(plan.VerifyAfterInstall);
        Assert.Equal(@"C:\tmp\game.apk", plan.ApkPath);
        Assert.Equal(["-r", "-d", "-t", "-g"], plan.AdbFlags);
    }

    [Fact]
    public void Policy_override_wins()
    {
        var manifest = InstallManifest.Placeholder with { InstallPolicy = InstallPolicy.InstallFresh, GrantPermissions = false, AllowTestApk = false };
        var plan = _planner.Create(new InstallRequest(manifest, Device("Pixel 9", DeviceKind.AndroidPhone, DeviceConnectionState.ConnectedReady), InstallPolicy.ReinstallAllowDowngrade));
        Assert.Equal(["-r", "-d"], plan.AdbFlags);
    }

    [Fact]
    public void Install_set_enables_verify_and_multiple_files()
    {
        var set = new InstallSet(
            "com.singularity.demo",
            "Demo",
            "1.2.3",
            ["base.apk", "config.apk"],
            true,
            false,
            ".MainActivity",
            null);
        var manifest = InstallManifest.ForInstallSet(set, InstallManifest.Session);
        var plan = _planner.Create(new InstallRequest(manifest, Device("Quest 3", DeviceKind.MetaQuest, DeviceConnectionState.ConnectedReady), Set: set));
        Assert.True(plan.VerifyAfterInstall);
        Assert.True(plan.UsesMultiple);
        Assert.Equal("com.singularity.demo", plan.PackageId);
        Assert.Equal(".MainActivity", plan.LauncherActivity);
        Assert.True(plan.OfferLaunchAfterInstall);
    }

    private static DeviceInfo Device(string model, DeviceKind kind, DeviceConnectionState state) =>
        new("serial", kind == DeviceKind.MetaQuest ? "Meta" : "Google", model, "14", kind, state, state == DeviceConnectionState.ConnectedReady, kind == DeviceKind.MetaQuest, new Dictionary<string, string>());
}
