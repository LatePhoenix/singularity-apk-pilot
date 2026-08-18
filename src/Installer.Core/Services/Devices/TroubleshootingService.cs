using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Content;

namespace Installer.Core.Services.Devices;

public sealed class TroubleshootingService : ITroubleshootingService
{
    public static readonly IReadOnlyList<TroubleshootNode> QuestSequence =
    [
        TroubleshootNode.PickDevice,
        TroubleshootNode.CableAndPort,
        TroubleshootNode.WearHeadset,
        TroubleshootNode.DeveloperMode,
        TroubleshootNode.MtpNotification,
        TroubleshootNode.AllowComputer,
        TroubleshootNode.UsbHelper,
        TroubleshootNode.RestartHelper,
        TroubleshootNode.WifiRescue,
        TroubleshootNode.RebootDevice,
        TroubleshootNode.StillStuck
    ];

    public static readonly IReadOnlyList<TroubleshootNode> PhoneSequence =
    [
        TroubleshootNode.PickDevice,
        TroubleshootNode.CableAndPort,
        TroubleshootNode.PhoneUnlock,
        TroubleshootNode.PhoneUsbMode,
        TroubleshootNode.PhoneDebugging,
        TroubleshootNode.PhoneAllow,
        TroubleshootNode.PhoneOemDriver,
        TroubleshootNode.RestartHelper,
        TroubleshootNode.RebootDevice,
        TroubleshootNode.StillStuck
    ];

    private readonly TroubleshootCopyDeck _copy;

    public TroubleshootingService(TroubleshootCopyDeck copy)
    {
        _copy = copy;
    }

    public TroubleshootSession Start(
        WizardStep returnStep,
        UsbEvidence evidence,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> devices)
    {
        var family = InferFamily(evidence, device);
        var node = family == TroubleshootFamily.Unknown
            ? TroubleshootNode.PickDevice
            : FirstFailingNode(family, evidence, devices);
        return Build(family, node, evidence, returnStep, device, devices, []);
    }

    public TroubleshootSession SelectFamily(
        TroubleshootSession session,
        TroubleshootFamily family,
        IReadOnlyList<DeviceInfo> devices)
    {
        var history = session.CurrentNode == TroubleshootNode.PickDevice
            ? Append(session.History, TroubleshootNode.PickDevice)
            : session.History;
        var node = FirstFailingNode(family, session.Evidence, devices);
        return Build(family, node, session.Evidence, session.ReturnStep, session.Device, devices, history);
    }

    public TroubleshootSession Confirm(TroubleshootSession session, IReadOnlyList<DeviceInfo> devices)
    {
        var next = NextNode(session);
        var history = Append(session.History, session.CurrentNode);
        return Build(session.Family, next, session.Evidence, session.ReturnStep, session.Device, devices, history);
    }

    public TroubleshootSession Back(TroubleshootSession session, IReadOnlyList<DeviceInfo> devices)
    {
        if (session.History.Count > 0)
        {
            var previous = session.History[^1];
            var history = session.History.Take(session.History.Count - 1).ToList();
            var family = previous == TroubleshootNode.PickDevice ? TroubleshootFamily.Unknown : session.Family;
            return Build(family, previous, session.Evidence, session.ReturnStep, session.Device, devices, history);
        }

        if (session.CurrentNode != TroubleshootNode.PickDevice)
        {
            return Build(
                TroubleshootFamily.Unknown,
                TroubleshootNode.PickDevice,
                session.Evidence,
                session.ReturnStep,
                session.Device,
                devices,
                []);
        }

        return session;
    }

    public TroubleshootSession ApplyEvidence(
        TroubleshootSession session,
        UsbEvidence evidence,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> devices)
    {
        var family = session.Family;
        var node = session.CurrentNode;

        if (family != TroubleshootFamily.Unknown)
        {
            var allow = AllowNode(family);
            if (HasUnauthorized(devices) && node != allow && node != TroubleshootNode.PickDevice)
            {
                node = allow;
            }
        }

        return Build(family, node, evidence, session.ReturnStep, device ?? session.Device, devices, session.History);
    }

    public TroubleshootNode FirstFailingNode(
        TroubleshootFamily family,
        UsbEvidence evidence,
        IReadOnlyList<DeviceInfo> devices)
    {
        if (HasUnauthorized(devices))
        {
            return AllowNode(family);
        }

        if (family == TroubleshootFamily.MetaQuest)
        {
            if (!evidence.QuestUsbPresent && !evidence.AndroidUsbPresent)
            {
                return TroubleshootNode.CableAndPort;
            }

            if (evidence.AdbDriverMissing)
            {
                return TroubleshootNode.UsbHelper;
            }

            if (evidence.QuestUsbPresent && !evidence.AdbInterfacePresent)
            {
                return TroubleshootNode.WearHeadset;
            }

            return TroubleshootNode.RestartHelper;
        }

        if (!evidence.AndroidUsbPresent && !evidence.QuestUsbPresent)
        {
            return TroubleshootNode.CableAndPort;
        }

        if (evidence.AdbDriverMissing)
        {
            return TroubleshootNode.PhoneOemDriver;
        }

        return TroubleshootNode.PhoneUnlock;
    }

    private TroubleshootSession Build(
        TroubleshootFamily family,
        TroubleshootNode node,
        UsbEvidence evidence,
        WizardStep returnStep,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> devices,
        IReadOnlyList<TroubleshootNode> history)
    {
        if (family != TroubleshootFamily.Unknown && ShouldSkip(node, family, evidence))
        {
            node = NextNode(family, node, evidence);
        }

        var looksLikeQuest = family == TroubleshootFamily.AndroidPhone && evidence.QuestUsbPresent;
        var action = ActionFor(node);
        var draft = new TroubleshootSession(
            family,
            node,
            evidence,
            returnStep,
            device,
            history,
            action,
            "",
            "Idle",
            [],
            "",
            looksLikeQuest);
        var (chip, tone) = _copy.Status(draft, devices);
        var label = _copy.ActionLabel(draft, false);
        var steps = _copy.Steps(draft);
        return draft with
        {
            StatusChip = chip,
            StatusTone = tone,
            GuideSteps = steps,
            InPageActionLabel = label
        };
    }

    private TroubleshootNode NextNode(TroubleshootSession session) =>
        NextNode(session.Family, session.CurrentNode, session.Evidence);

    private static TroubleshootNode NextNode(TroubleshootFamily family, TroubleshootNode current, UsbEvidence evidence)
    {
        var sequence = Sequence(family);
        var index = sequence.ToList().IndexOf(current);
        if (index < 0)
        {
            return TroubleshootNode.StillStuck;
        }

        for (var i = index + 1; i < sequence.Count; i++)
        {
            if (!ShouldSkip(sequence[i], family, evidence))
            {
                return sequence[i];
            }
        }

        return TroubleshootNode.StillStuck;
    }

    private static bool ShouldSkip(TroubleshootNode node, TroubleshootFamily family, UsbEvidence evidence)
    {
        if (node == TroubleshootNode.PickDevice)
        {
            return family != TroubleshootFamily.Unknown;
        }

        if (node == TroubleshootNode.CableAndPort)
        {
            return family == TroubleshootFamily.MetaQuest
                ? evidence.QuestUsbPresent || evidence.AndroidUsbPresent
                : evidence.WindowsSeesUsb;
        }

        if (node == TroubleshootNode.UsbHelper)
        {
            return !evidence.AdbDriverMissing;
        }

        return false;
    }

    private static IReadOnlyList<TroubleshootNode> Sequence(TroubleshootFamily family) =>
        family == TroubleshootFamily.AndroidPhone ? PhoneSequence : QuestSequence;

    private static TroubleshootFamily InferFamily(UsbEvidence evidence, DeviceInfo? device)
    {
        if (device?.Kind == DeviceKind.MetaQuest || device?.IsQuest == true || evidence.QuestUsbPresent)
        {
            return TroubleshootFamily.MetaQuest;
        }

        if (device?.Kind == DeviceKind.AndroidPhone || evidence.AndroidUsbPresent)
        {
            return TroubleshootFamily.AndroidPhone;
        }

        return TroubleshootFamily.Unknown;
    }

    private static bool HasUnauthorized(IReadOnlyList<DeviceInfo> devices) =>
        devices.Any(d => d.State == DeviceConnectionState.Unauthorized);

    private static TroubleshootNode AllowNode(TroubleshootFamily family) =>
        family == TroubleshootFamily.AndroidPhone ? TroubleshootNode.PhoneAllow : TroubleshootNode.AllowComputer;

    private static TroubleshootActionKind ActionFor(TroubleshootNode node) =>
        node switch
        {
            TroubleshootNode.RestartHelper => TroubleshootActionKind.RestartAdbServer,
            TroubleshootNode.UsbHelper => TroubleshootActionKind.InstallUsbHelper,
            TroubleshootNode.PhoneOemDriver => TroubleshootActionKind.OpenPhoneUsbSupport,
            TroubleshootNode.StillStuck => TroubleshootActionKind.ExportDiagnostics,
            _ => TroubleshootActionKind.None
        };

    private static IReadOnlyList<TroubleshootNode> Append(IReadOnlyList<TroubleshootNode> history, TroubleshootNode node)
    {
        var list = history.ToList();
        list.Add(node);
        return list;
    }
}
