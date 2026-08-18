using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public sealed class WizardFlowService : IWizardFlowService
{
    private readonly FlowDecisionEngine _engine;
    private readonly IContentService _copy;
    private readonly IRecoveryService _recovery;
    private readonly ITroubleshootingService _troubleshoot;

    public WizardFlowService(
        FlowDecisionEngine engine,
        IContentService copy,
        IRecoveryService recovery,
        ITroubleshootingService troubleshoot)
    {
        _engine = engine;
        _copy = copy;
        _recovery = recovery;
        _troubleshoot = troubleshoot;
    }

    public WizardState CreateInitialState(InstallManifest manifest)
    {
        var copy = _copy.GetCopy(WizardStep.Welcome, manifest, null);
        return new WizardState(WizardStep.Welcome, manifest, null, null, [], copy);
    }

    public WizardState Advance(
        WizardState state,
        WizardTrigger trigger,
        DeviceInfo? device = null,
        InstallResult? installResult = null,
        IReadOnlyList<DeviceInfo>? readyDevices = null,
        DeviceHealth? health = null,
        TroubleshootSession? troubleshoot = null)
    {
        var activeDevice = readyDevices is not null ? device : device ?? state.Device;
        var ready = readyDevices ?? state.ReadyDevices;
        var connectAttempts = state.ConnectAttempts;
        if (trigger is WizardTrigger.Continue or WizardTrigger.ConfirmAuthorization or WizardTrigger.ConfirmDeveloperMode or WizardTrigger.DeviceRefresh
            && state.CurrentStep == WizardStep.ConnectDevice
            && (activeDevice is null || activeDevice.State == DeviceConnectionState.NotConnected))
        {
            connectAttempts++;
        }

        var developerLikely = state.DeveloperModeLikelyRequired
                              || connectAttempts >= 2
                              || (activeDevice?.Kind == DeviceKind.MetaQuest && activeDevice.State == DeviceConnectionState.Offline);

        var nextStep = _engine.Decide(
            state with { ConnectAttempts = connectAttempts, DeveloperModeLikelyRequired = developerLikely, Device = activeDevice, ReadyDevices = ready },
            trigger,
            activeDevice,
            installResult);

        WizardStep? returnStep = state.ReturnStep;
        if (trigger == WizardTrigger.OpenInstalledApps
            && nextStep == WizardStep.InstalledApps
            && state.CurrentStep is WizardStep.ReadyToInstall or WizardStep.Complete)
        {
            returnStep = state.CurrentStep;
        }

        if (trigger == WizardTrigger.OpenTroubleshoot && nextStep == WizardStep.Troubleshoot)
        {
            returnStep = state.CurrentStep == WizardStep.Troubleshoot ? state.ReturnStep : state.CurrentStep;
        }

        if (nextStep == WizardStep.Troubleshoot
            && state.CurrentStep != WizardStep.Troubleshoot
            && trigger != WizardTrigger.OpenTroubleshoot)
        {
            returnStep = state.CurrentStep;
        }

        if (nextStep is not WizardStep.InstalledApps and not WizardStep.Troubleshoot)
        {
            returnStep = null;
        }

        var session = ResolveSession(
            state,
            trigger,
            nextStep,
            returnStep ?? WizardStep.ConnectDevice,
            activeDevice,
            ready ?? [],
            health,
            troubleshoot);

        var result = installResult ?? state.LastInstallResult;
        var actions = result is { Success: false, Error: not null }
            ? _recovery.Suggest(result.Error.Value, state.Manifest)
            : Array.Empty<RecoveryAction>();

        if (nextStep != WizardStep.InstallProblem)
        {
            actions = nextStep == WizardStep.Complete ? [] : state.SuggestedActions;
            if (nextStep is WizardStep.Welcome or WizardStep.ConnectDevice or WizardStep.ReadyToInstall or WizardStep.InstalledApps or WizardStep.Troubleshoot)
            {
                actions = [];
            }
        }

        var copy = _copy.GetCopy(nextStep, state.Manifest, activeDevice, result?.Error, health ?? state.Health, session);
        var busy = nextStep == WizardStep.Installing;
        var status = nextStep switch
        {
            WizardStep.Installing => "Installing",
            WizardStep.Complete => "Installed",
            _ => null
        };

        return new WizardState(
            nextStep,
            state.Manifest,
            activeDevice,
            result,
            actions,
            copy,
            busy,
            status,
            connectAttempts,
            developerLikely,
            ready,
            health ?? state.Health,
            returnStep,
            session);
    }

    private TroubleshootSession? ResolveSession(
        WizardState state,
        WizardTrigger trigger,
        WizardStep nextStep,
        WizardStep returnStep,
        DeviceInfo? device,
        IReadOnlyList<DeviceInfo> ready,
        DeviceHealth? health,
        TroubleshootSession? provided)
    {
        if (nextStep != WizardStep.Troubleshoot)
        {
            return null;
        }

        var evidence = health?.Evidence ?? state.Health?.Evidence ?? UsbEvidence.None;
        if (provided is not null)
        {
            return _troubleshoot.ApplyEvidence(provided, evidence, device, ready);
        }

        if (trigger == WizardTrigger.Continue && state.Troubleshoot is not null)
        {
            var updated = _troubleshoot.ApplyEvidence(state.Troubleshoot, evidence, device, ready);
            if (updated.CurrentNode != state.Troubleshoot.CurrentNode)
            {
                return updated;
            }

            return _troubleshoot.Confirm(updated, ready);
        }

        if (state.Troubleshoot is not null)
        {
            return _troubleshoot.ApplyEvidence(state.Troubleshoot, evidence, device, ready);
        }

        return _troubleshoot.Start(returnStep, evidence, device, ready);
    }
}
