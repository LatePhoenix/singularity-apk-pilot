using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public sealed class WizardFlowService : IWizardFlowService
{
    private readonly FlowDecisionEngine _engine;
    private readonly IContentService _copy;
    private readonly IRecoveryService _recovery;

    public WizardFlowService(FlowDecisionEngine engine, IContentService copy, IRecoveryService recovery)
    {
        _engine = engine;
        _copy = copy;
        _recovery = recovery;
    }

    public WizardState CreateInitialState(InstallManifest manifest)
    {
        var copy = _copy.GetCopy(WizardStep.Welcome, manifest, null);
        return new WizardState(WizardStep.Welcome, manifest, null, null, [], copy);
    }

    public WizardState Advance(WizardState state, WizardTrigger trigger, DeviceInfo? device = null, InstallResult? installResult = null)
    {
        var activeDevice = device ?? state.Device;
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
            state with { ConnectAttempts = connectAttempts, DeveloperModeLikelyRequired = developerLikely, Device = activeDevice },
            trigger,
            activeDevice,
            installResult);

        var result = installResult ?? state.LastInstallResult;
        var actions = result is { Success: false, Error: not null }
            ? _recovery.Suggest(result.Error.Value, state.Manifest)
            : Array.Empty<RecoveryAction>();

        if (nextStep != WizardStep.InstallProblem)
        {
            actions = nextStep == WizardStep.Complete ? [] : state.SuggestedActions;
            if (nextStep is WizardStep.Welcome or WizardStep.ConnectDevice or WizardStep.ReadyToInstall)
            {
                actions = [];
            }
        }

        var copy = _copy.GetCopy(nextStep, state.Manifest, activeDevice, result?.Error);
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
            developerLikely);
    }
}
