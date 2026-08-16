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
    public void Policy_override_wins()
    {
        var manifest = InstallManifest.Placeholder with { InstallPolicy = InstallPolicy.InstallFresh, GrantPermissions = false, AllowTestApk = false };
        var plan = _planner.Create(new InstallRequest(manifest, Device("Pixel 9", DeviceKind.AndroidPhone, DeviceConnectionState.ConnectedReady), InstallPolicy.ReinstallAllowDowngrade));
        Assert.Equal(["-r", "-d"], plan.AdbFlags);
    }

    private static DeviceInfo Device(string model, DeviceKind kind, DeviceConnectionState state) =>
        new("serial", kind == DeviceKind.MetaQuest ? "Meta" : "Google", model, "14", kind, state, state == DeviceConnectionState.ConnectedReady, kind == DeviceKind.MetaQuest, new Dictionary<string, string>());
}
