using Installer.Core.Models;
using Installer.Core.Services.Content;
using Installer.Core.Services.Devices;

namespace Installer.Core.Tests.Devices;

public sealed class TroubleshootingServiceTests
{
    private readonly TroubleshootingService _service = new(new TroubleshootCopyDeck());

    [Fact]
    public void Start_without_evidence_asks_what_device()
    {
        var session = _service.Start(WizardStep.ConnectDevice, UsbEvidence.None, null, []);
        Assert.Equal(TroubleshootFamily.Unknown, session.Family);
        Assert.Equal(TroubleshootNode.PickDevice, session.CurrentNode);
        Assert.True(session.ShowFamilyPicker);
    }

    [Fact]
    public void Start_with_quest_usb_skips_cable()
    {
        var evidence = new UsbEvidence(true, false, false, false, true, false);
        var session = _service.Start(WizardStep.ConnectDevice, evidence, null, []);
        Assert.Equal(TroubleshootFamily.MetaQuest, session.Family);
        Assert.Equal(TroubleshootNode.WearHeadset, session.CurrentNode);
        Assert.DoesNotContain("ADB", string.Join(' ', session.GuideSteps), StringComparison.Ordinal);
    }

    [Fact]
    public void Start_with_missing_driver_opens_usb_helper()
    {
        var evidence = new UsbEvidence(true, false, true, true, false, false);
        var session = _service.Start(WizardStep.ConnectDevice, evidence, null, []);
        Assert.Equal(TroubleshootNode.UsbHelper, session.CurrentNode);
        Assert.Equal(TroubleshootActionKind.InstallUsbHelper, session.RecommendedAction);
    }

    [Fact]
    public void Unauthorized_quest_jumps_to_allow()
    {
        var device = Quest(DeviceConnectionState.Unauthorized);
        var session = _service.Start(WizardStep.ConnectDevice, UsbEvidence.None, device, [device]);
        Assert.Equal(TroubleshootNode.AllowComputer, session.CurrentNode);
    }

    [Fact]
    public void Confirm_walks_quest_nodes_and_skips_usb_helper_without_driver_gap()
    {
        var evidence = UsbEvidence.None;
        var session = _service.SelectFamily(
            _service.Start(WizardStep.ConnectDevice, evidence, null, []),
            TroubleshootFamily.MetaQuest,
            []);
        Assert.Equal(TroubleshootNode.CableAndPort, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.WearHeadset, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.DeveloperMode, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.MtpNotification, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.AllowComputer, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.RestartHelper, session.CurrentNode);
        Assert.Equal(TroubleshootActionKind.RestartAdbServer, session.RecommendedAction);
    }

    [Fact]
    public void Phone_path_uses_file_transfer_and_oem_helper()
    {
        var session = _service.SelectFamily(
            _service.Start(WizardStep.ConnectDevice, UsbEvidence.None, null, []),
            TroubleshootFamily.AndroidPhone,
            []);
        Assert.Equal(TroubleshootNode.CableAndPort, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.PhoneUnlock, session.CurrentNode);
        session = _service.Confirm(session, []);
        Assert.Equal(TroubleshootNode.PhoneUsbMode, session.CurrentNode);
        Assert.Contains("file transfer", string.Join(' ', session.GuideSteps), StringComparison.OrdinalIgnoreCase);
        while (session.CurrentNode != TroubleshootNode.PhoneOemDriver)
        {
            session = _service.Confirm(session, []);
        }

        Assert.Equal(TroubleshootActionKind.OpenPhoneUsbSupport, session.RecommendedAction);
    }

    [Fact]
    public void Picked_phone_with_quest_usb_sets_looks_like_quest()
    {
        var evidence = new UsbEvidence(true, false, false, false, true, false);
        var session = _service.Start(WizardStep.ConnectDevice, UsbEvidence.None, null, []);
        session = session with { Evidence = evidence };
        session = _service.SelectFamily(session, TroubleshootFamily.AndroidPhone, []);
        Assert.True(session.LooksLikeQuest);
    }

    [Fact]
    public void Back_restores_previous_node()
    {
        var session = _service.SelectFamily(
            _service.Start(WizardStep.Authorization, UsbEvidence.None, null, []),
            TroubleshootFamily.MetaQuest,
            []);
        var first = session.CurrentNode;
        session = _service.Confirm(session, []);
        session = _service.Back(session, []);
        Assert.Equal(first, session.CurrentNode);
    }

    [Fact]
    public void ApplyEvidence_jumps_to_allow_when_unauthorized()
    {
        var session = _service.SelectFamily(
            _service.Start(WizardStep.ConnectDevice, UsbEvidence.None, null, []),
            TroubleshootFamily.MetaQuest,
            []);
        var device = Quest(DeviceConnectionState.Unauthorized);
        session = _service.ApplyEvidence(session, UsbEvidence.None, device, [device]);
        Assert.Equal(TroubleshootNode.AllowComputer, session.CurrentNode);
    }

    [Fact]
    public void Primary_copy_does_not_say_adb()
    {
        var deck = new TroubleshootCopyDeck();
        foreach (var node in Enum.GetValues<TroubleshootNode>())
        {
            foreach (var family in new[] { TroubleshootFamily.MetaQuest, TroubleshootFamily.AndroidPhone })
            {
                var session = new TroubleshootSession(
                    family,
                    node,
                    UsbEvidence.None,
                    WizardStep.ConnectDevice,
                    null,
                    [],
                    TroubleshootActionKind.None,
                    "",
                    "Idle",
                    [],
                    "",
                    false);
                var copy = deck.Page(session);
                Assert.DoesNotContain("ADB", copy.Headline, StringComparison.Ordinal);
                Assert.DoesNotContain("ADB", copy.Body, StringComparison.Ordinal);
                Assert.DoesNotContain("ADB", copy.PrimaryAction, StringComparison.Ordinal);
                Assert.DoesNotContain("pnputil", copy.Body, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("sideload", copy.Body, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Samsung_usb_mode_mentions_transfer_files()
    {
        var phone = new DeviceInfo(
            "phone",
            "samsung",
            "Galaxy S24",
            "14",
            DeviceKind.AndroidPhone,
            DeviceConnectionState.NotConnected,
            false,
            false,
            new Dictionary<string, string>());
        var session = new TroubleshootSession(
            TroubleshootFamily.AndroidPhone,
            TroubleshootNode.PhoneUsbMode,
            UsbEvidence.None,
            WizardStep.ConnectDevice,
            phone,
            [],
            TroubleshootActionKind.None,
            "",
            "Idle",
            [],
            "",
            false);
        var body = new TroubleshootCopyDeck().Page(session).Body;
        Assert.Contains("Transfer files", body, StringComparison.OrdinalIgnoreCase);
    }

    private static DeviceInfo Quest(DeviceConnectionState state) =>
        new("quest-serial", "Oculus", "Quest 2", "12", DeviceKind.MetaQuest, state, state == DeviceConnectionState.ConnectedReady, true, new Dictionary<string, string>());
}
