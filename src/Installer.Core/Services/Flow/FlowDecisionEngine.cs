using Installer.Core.Models;

namespace Installer.Core.Services.Flow;

public sealed class FlowDecisionEngine
{
    private readonly QuestFlowStrategy _quest;
    private readonly AndroidPhoneFlowStrategy _phone;

    public FlowDecisionEngine(QuestFlowStrategy quest, AndroidPhoneFlowStrategy phone)
    {
        _quest = quest;
        _phone = phone;
    }

    public IWizardFlowStrategy StrategyFor(DeviceInfo? device)
    {
        if (device?.Kind == DeviceKind.MetaQuest || device?.IsQuest == true)
        {
            return _quest;
        }

        return _phone;
    }

    public WizardStep Decide(WizardState state, WizardTrigger trigger, DeviceInfo? device, InstallResult? installResult)
    {
        var active = device ?? state.Device;

        if (trigger == WizardTrigger.Done)
        {
            return WizardStep.Welcome;
        }

        if (trigger == WizardTrigger.Cancel && state.CurrentStep == WizardStep.Installing)
        {
            return WizardStep.ReadyToInstall;
        }

        if (trigger == WizardTrigger.InstallFinished)
        {
            return installResult?.Success == true ? WizardStep.Complete : WizardStep.InstallProblem;
        }

        if (trigger == WizardTrigger.Install)
        {
            return WizardStep.Installing;
        }

        if (trigger is WizardTrigger.Retry or WizardTrigger.AutoFix)
        {
            return WizardStep.Installing;
        }

        if (trigger == WizardTrigger.Start)
        {
            return WizardStep.ConnectDevice;
        }

        if (active is null || active.State == DeviceConnectionState.NotConnected)
        {
            if ((state.ConnectAttempts >= 2 || state.DeveloperModeLikelyRequired) &&
                state.CurrentStep is WizardStep.ConnectDevice or WizardStep.DeviceDetected or WizardStep.Authorization)
            {
                return WizardStep.DeveloperMode;
            }

            return state.CurrentStep == WizardStep.Welcome ? WizardStep.Welcome : WizardStep.ConnectDevice;
        }

        var strategy = StrategyFor(active);

        if (state.CurrentStep == WizardStep.ConnectDevice)
        {
            return WizardStep.DeviceDetected;
        }

        if (trigger == WizardTrigger.DeviceRefresh
            && state.CurrentStep == WizardStep.DeviceDetected
            && state.NeedsDevicePicker)
        {
            return WizardStep.DeviceDetected;
        }

        if (state.CurrentStep == WizardStep.DeviceDetected ||
            state.CurrentStep == WizardStep.Authorization ||
            state.CurrentStep == WizardStep.DeveloperMode ||
            trigger == WizardTrigger.DeviceRefresh)
        {
            return strategy.NextAfterDetection(active, state.ConnectAttempts);
        }

        if (state.CurrentStep == WizardStep.ReadyToInstall && trigger == WizardTrigger.Continue)
        {
            return WizardStep.Installing;
        }

        return state.CurrentStep;
    }
}
