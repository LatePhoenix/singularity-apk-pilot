using Installer.Core.Models;
using Installer.Core.Services.Content;

namespace Installer.Core.Tests.Content;

public sealed class GuideCoachServiceTests
{
    private readonly GuideCoachService _coach = new();
    private readonly CopyDeckService _copy = new(new Installer.Core.Services.Support.FriendlyMessageService());

    [Fact]
    public void Welcome_tells_the_user_to_press_start_without_jargon()
    {
        var script = _coach.For(State(WizardStep.Welcome));
        Assert.Equal("1 of 6", script.Progress);
        Assert.Contains("Start", script.ButtonHint, StringComparison.Ordinal);
        Assert.DoesNotContain("ADB", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("sideload", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.True(script.HasChecks);
    }

    [Fact]
    public void Connect_asks_to_plug_in_a_data_cable()
    {
        var script = _coach.For(State(WizardStep.ConnectDevice));
        Assert.Equal("2 of 6", script.Progress);
        Assert.Contains("USB-C", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("I connected it", script.ButtonHint, StringComparison.Ordinal);
        Assert.Contains("charge", string.Join(' ', script.Checks), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Quest_authorization_sends_the_user_into_the_headset()
    {
        var device = Quest(DeviceConnectionState.Unauthorized);
        var script = _coach.For(State(WizardStep.Authorization, device));
        Assert.Contains("headset", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Always allow", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("I allowed it", script.ButtonHint, StringComparison.Ordinal);
        Assert.Equal("Wait", script.Mood);
    }

    [Fact]
    public void Ready_without_files_asks_to_add_an_apk()
    {
        var device = Quest(DeviceConnectionState.ConnectedReady);
        var script = _coach.For(State(WizardStep.ReadyToInstall, device), hasSelectedFiles: false);
        Assert.Contains("Add app files", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(".apk", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Install now", script.ButtonHint, StringComparison.Ordinal);
    }

    [Fact]
    public void Ready_with_files_asks_to_install_now()
    {
        var device = Quest(DeviceConnectionState.ConnectedReady);
        var script = _coach.For(State(WizardStep.ReadyToInstall, device), hasSelectedFiles: true);
        Assert.Contains("Install now", script.Now, StringComparison.Ordinal);
        Assert.DoesNotContain("Add app files", script.Now, StringComparison.Ordinal);
    }

    [Fact]
    public void Complete_quest_points_to_unknown_sources()
    {
        var device = Quest(DeviceConnectionState.ConnectedReady);
        var script = _coach.For(State(WizardStep.Complete, device));
        Assert.Contains("Library", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Unknown Sources", script.Now, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("Done", script.Mood);
    }

    [Fact]
    public void Problem_stays_calm_and_uses_the_error_title()
    {
        var copy = _copy.GetCopy(WizardStep.InstallProblem, InstallManifest.Session, Quest(DeviceConnectionState.ConnectedReady), InstallError.UnauthorizedDevice);
        var state = new WizardState(
            WizardStep.InstallProblem,
            InstallManifest.Session,
            Quest(DeviceConnectionState.ConnectedReady),
            null,
            [],
            copy);
        var script = _coach.For(state);
        Assert.Equal("Warn", script.Mood);
        Assert.Equal(copy.Headline, script.Now);
        Assert.DoesNotContain("ADB", script.Now, StringComparison.OrdinalIgnoreCase);
    }

    private WizardState State(WizardStep step, DeviceInfo? device = null)
    {
        var copy = _copy.GetCopy(step, InstallManifest.Session, device);
        return new WizardState(step, InstallManifest.Session, device, null, [], copy);
    }

    private static DeviceInfo Quest(DeviceConnectionState state) =>
        new("quest-serial", "Oculus", "Quest 3", "14", DeviceKind.MetaQuest, state, state == DeviceConnectionState.ConnectedReady, true, new Dictionary<string, string>());
}
