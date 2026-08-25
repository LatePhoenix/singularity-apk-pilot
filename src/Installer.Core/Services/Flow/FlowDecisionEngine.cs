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

        if (trigger == WizardTrigger.StartOver)
        {
            return WizardStep.ConnectDevice;
        }

        if (trigger == WizardTrigger.ResumeSetup)
        {
            return AfterConnect(state, active);
        }

        if (trigger == WizardTrigger.Back)
        {
            return BackFrom(state);
        }

        if (trigger == WizardTrigger.Done)
        {
            return WizardStep.Welcome;
        }

        if (trigger == WizardTrigger.DeviceRefresh && state.HoldStep)
        {
            if (active is null || active.State == DeviceConnectionState.NotConnected)
            {
                return state.CurrentStep == WizardStep.Welcome ? WizardStep.Welcome : WizardStep.ConnectDevice;
            }

            return state.CurrentStep;
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
            return AfterConnect(state, active);
        }

        if (trigger == WizardTrigger.OpenInstalledApps)
        {
            return active is { State: DeviceConnectionState.ConnectedReady }
                ? WizardStep.InstalledApps
                : state.CurrentStep;
        }

        if (trigger == WizardTrigger.CloseInstalledApps)
        {
            return state.ReturnStep is WizardStep.ReadyToInstall or WizardStep.Complete
                ? state.ReturnStep.Value
                : WizardStep.ReadyToInstall;
        }

        if (state.CurrentStep == WizardStep.InstalledApps)
        {
            if (active is null || active.State != DeviceConnectionState.ConnectedReady)
            {
                return WizardStep.ConnectDevice;
            }

            return WizardStep.InstalledApps;
        }

        if (trigger == WizardTrigger.OpenTroubleshoot)
        {
            return WizardStep.Troubleshoot;
        }

        if (trigger == WizardTrigger.CloseTroubleshoot)
        {
            return state.ReturnStep
                   is WizardStep.ConnectDevice
                   or WizardStep.Authorization
                   or WizardStep.DeveloperMode
                   or WizardStep.InstallProblem
                   or WizardStep.InstalledApps
                ? state.ReturnStep.Value
                : WizardStep.ConnectDevice;
        }

        if (state.CurrentStep == WizardStep.Troubleshoot)
        {
            if (active is { State: DeviceConnectionState.ConnectedReady })
            {
                var readyCount = (state.ReadyDevices ?? []).Count(d => d.State == DeviceConnectionState.ConnectedReady);
                if (readyCount >= 2)
                {
                    return WizardStep.DeviceDetected;
                }

                return StrategyFor(active).NextAfterDetection(active, state.ConnectAttempts);
            }

            return WizardStep.Troubleshoot;
        }

        if (active is null || active.State == DeviceConnectionState.NotConnected)
        {
            if ((state.ConnectAttempts >= 2 || state.DeveloperModeLikelyRequired) &&
                state.CurrentStep is WizardStep.ConnectDevice or WizardStep.DeviceDetected or WizardStep.Authorization or WizardStep.DeveloperMode)
            {
                return WizardStep.Troubleshoot;
            }

            return state.CurrentStep == WizardStep.Welcome ? WizardStep.Welcome : WizardStep.ConnectDevice;
        }

        if ((state.ConnectAttempts >= 2 || state.DeveloperModeLikelyRequired)
            && trigger is WizardTrigger.Continue or WizardTrigger.ConfirmAuthorization or WizardTrigger.ConfirmDeveloperMode)
        {
            if (active.State == DeviceConnectionState.Unauthorized
                && state.CurrentStep is WizardStep.Authorization or WizardStep.ConnectDevice or WizardStep.DeviceDetected)
            {
                return WizardStep.Troubleshoot;
            }

            if (active.State == DeviceConnectionState.Offline && state.CurrentStep == WizardStep.DeveloperMode)
            {
                return WizardStep.Troubleshoot;
            }
        }

        var strategy = StrategyFor(active);

        if (state.CurrentStep == WizardStep.ConnectDevice)
        {
            return AfterConnect(state, active);
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

    private WizardStep AfterConnect(WizardState state, DeviceInfo? active)
    {
        if (active is null || active.State == DeviceConnectionState.NotConnected)
        {
            return WizardStep.ConnectDevice;
        }

        var readyCount = (state.ReadyDevices ?? []).Count(d => d.State == DeviceConnectionState.ConnectedReady);
        if (readyCount >= 2)
        {
            return WizardStep.DeviceDetected;
        }

        return StrategyFor(active).NextAfterDetection(active, state.ConnectAttempts);
    }

    private static WizardStep BackFrom(WizardState state) => state.CurrentStep switch
    {
        WizardStep.ConnectDevice => WizardStep.Welcome,
        WizardStep.DeviceDetected => WizardStep.ConnectDevice,
        WizardStep.Authorization => state.NeedsDevicePicker ? WizardStep.DeviceDetected : WizardStep.ConnectDevice,
        WizardStep.DeveloperMode => WizardStep.ConnectDevice,
        WizardStep.ReadyToInstall => state.NeedsDevicePicker ? WizardStep.DeviceDetected : WizardStep.ConnectDevice,
        WizardStep.InstallProblem => WizardStep.ReadyToInstall,
        WizardStep.Complete => WizardStep.ReadyToInstall,
        WizardStep.InstalledApps => state.ReturnStep is WizardStep.ReadyToInstall or WizardStep.Complete
            ? state.ReturnStep.Value
            : WizardStep.ReadyToInstall,
        _ => state.CurrentStep
    };
}
