using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Content;

public sealed class GuideCoachService : IGuideCoach
{
    public GuideScript For(WizardState state, bool hasSelectedFiles = false)
    {
        var action = string.IsNullOrWhiteSpace(state.Copy.PrimaryAction) ? "Continue" : state.Copy.PrimaryAction;
        var quest = state.Device?.IsQuest == true || state.Device?.Kind == DeviceKind.MetaQuest;
        var model = state.Device?.DisplayName ?? (quest ? "your headset" : "your device");

        return state.CurrentStep switch
        {
            WizardStep.Welcome => new GuideScript(
                "Hi. I am Pilot. I will walk you through this.",
                "You are about to install an app on a headset or phone. You only need to do one thing at a time.",
                "First we will plug in the device.",
                Press(action),
                "1 of 6",
                ["Have the headset or phone nearby.", "Have a USB-C cable that can copy files, not just charge."],
                "Calm"),
            WizardStep.ConnectDevice => new GuideScript(
                "Let's connect the device.",
                "Plug the headset or phone into this computer with a USB-C cable.",
                "Next we will check that this computer can see it.",
                Press(action),
                "2 of 6",
                [
                    "Use a cable that copies files. The short cable in a Quest box often only charges.",
                    "Plug into the computer itself, not a hub, keyboard, or monitor.",
                    "Wi-Fi is optional. You can set that up later."
                ],
                string.IsNullOrWhiteSpace(state.Health?.StatusChip) ? "Wait" : state.Health!.StatusTone == "Error" ? "Warn" : "Wait"),
            WizardStep.DeviceDetected => new GuideScript(
                quest ? "I found your headset." : "I found your phone.",
                quest
                    ? $"{model} is connected. Press Continue so we can check a permission."
                    : $"{model} is connected. Press Continue so we can check a permission.",
                "The next step happens on the device, not on this screen.",
                Press(action),
                "3 of 6",
                state.NeedsDevicePicker
                    ? ["If you see more than one device, pick the one you want first."]
                    : [],
                "Done"),
            WizardStep.Authorization when quest => new GuideScript(
                "This part happens inside the headset.",
                "Put the headset on. Look for a message asking to allow this computer. Choose Always allow, then Allow.",
                "After you allow it, come back to this computer.",
                Press(action),
                "4 of 6",
                ["Keep the cable plugged in.", "If you already closed the message, unplug and plug the cable back in."],
                "Wait"),
            WizardStep.Authorization => new GuideScript(
                "This part happens on the phone.",
                "Unlock the phone. Look for a message about USB debugging. Check Always allow from this computer, then tap Allow.",
                "After you allow it, come back to this computer.",
                Press(action),
                "4 of 6",
                ["If no message appears, unplug, plug back in, and choose File transfer if the phone asks."],
                "Wait"),
            WizardStep.DeveloperMode => new GuideScript(
                "The headset needs one setting turned on.",
                "On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on.",
                "Then plug the headset in again and put it on.",
                Press(action),
                "4 of 6",
                ["This uses the same Meta account that is signed into the headset.", "You need to be on a developer team."],
                "Wait"),
            WizardStep.ReadyToInstall when hasSelectedFiles => new GuideScript(
                $"{model} is ready.",
                "The file is selected. Press Install now and wait. Leave the device as it is.",
                "When it finishes, I will tell you where to find the app.",
                Press(action),
                "5 of 6",
                state.Device?.IsWireless == true
                    ? ["Keep the device awake on this Wi-Fi."]
                    : ["You can switch to Wi-Fi first if you want to unplug."],
                "Done"),
            WizardStep.ReadyToInstall => new GuideScript(
                $"{model} is ready.",
                "Press Add app files and choose the file you were sent. It usually ends with .apk.",
                "You can drop the file on the box, or use last files if you installed this before.",
                "Add the file, then press Install now.",
                "5 of 6",
                ["Your photos and other apps are not touched.", "If a file looks incomplete, add the other parts too."],
                "Calm"),
            WizardStep.Installing => new GuideScript(
                "Almost done.",
                state.Device?.IsWireless == true
                    ? "Please wait. Keep the device awake on the same Wi-Fi as this computer."
                    : "Please wait. Keep the cable plugged in. Do not unplug the device.",
                "This can take a minute. I will tell you when it is finished.",
                "You can press Cancel if you need to stop.",
                "6 of 6",
                [],
                "Work"),
            WizardStep.InstallProblem => new GuideScript(
                "Something did not finish. That is OK.",
                state.Copy.Headline,
                string.IsNullOrWhiteSpace(state.Copy.Body)
                    ? "Use the suggested action on the left. If that does not work, send a report."
                    : state.Copy.Body,
                Press(action),
                "6 of 6",
                ["You are not in trouble. Most problems are a cable, a permission, or an older copy of the app."],
                "Warn"),
            WizardStep.Complete => new GuideScript(
                "It worked.",
                quest
                    ? "Put on the headset. Open Library, then Unknown Sources, and look for the app."
                    : "Find the new app in the phone's app list and open it.",
                "You can install another app, or press Done.",
                Press(action),
                "6 of 6",
                quest
                    ? ["Headset menus move around. If you do not see it, check the Library filter."]
                    : [],
                "Done"),
            WizardStep.InstalledApps => new GuideScript(
                "These are extra apps on the device.",
                "Search if the list is long. Remove one app at a time. Removing deletes that app and its data.",
                "Press Back when you are finished.",
                Press(action),
                "Apps",
                ["Store apps can be installed again from the store.", "Apps you installed from a file need that file again."],
                "Calm"),
            WizardStep.Troubleshoot => Troubleshoot(state, action),
            _ => new GuideScript(
                "I am here to help.",
                state.Copy.Headline,
                state.Copy.Body,
                Press(action),
                "",
                [],
                "Calm")
        };
    }

    private static GuideScript Troubleshoot(WizardState state, string action)
    {
        var node = state.Troubleshoot?.CurrentNode ?? TroubleshootNode.PickDevice;
        var now = string.IsNullOrWhiteSpace(state.Copy.Body) ? state.Copy.Headline : state.Copy.Body;
        return new GuideScript(
            "Let's fix the connection. One small step.",
            now,
            "When this step is done, press the green button. I will take you to the next one.",
            Press(action),
            "Help",
            node == TroubleshootNode.PickDevice
                ? ["Quest is the headset you wear.", "Phone is a regular Android phone."]
                : [],
            "Wait");
    }

    private static string Press(string action) => $"When that is done, press {action}.";
}
