using Installer.Core.Abstractions;
using Installer.Core.Models;
using Installer.Core.Services.Support;

namespace Installer.Core.Services.Content;

public sealed class CopyDeckService : IContentService
{
    private readonly FriendlyMessageService _messages;
    private readonly TroubleshootCopyDeck _troubleshootCopy;

    public CopyDeckService(FriendlyMessageService messages, TroubleshootCopyDeck? troubleshootCopy = null)
    {
        _messages = messages;
        _troubleshootCopy = troubleshootCopy ?? new TroubleshootCopyDeck();
    }

    public WizardCopy GetCopy(
        WizardStep step,
        InstallManifest manifest,
        DeviceInfo? device,
        InstallError? error = null,
        DeviceHealth? health = null,
        TroubleshootSession? troubleshoot = null)
    {
        var name = manifest.DisplayName;
        var version = manifest.BuildVersion;
        var model = device?.DisplayName ?? "your device";
        var quest = device?.IsQuest == true || device?.Kind == DeviceKind.MetaQuest;
        var userPicked = string.Equals(manifest.AppId, InstallManifest.UserSelectedAppId, StringComparison.OrdinalIgnoreCase);
        var healthHint = health?.Hint;

        if (step == WizardStep.Troubleshoot)
        {
            return _troubleshootCopy.Page(troubleshoot ?? new TroubleshootSession(
                TroubleshootFamily.Unknown,
                TroubleshootNode.PickDevice,
                UsbEvidence.None,
                WizardStep.ConnectDevice,
                device,
                [],
                TroubleshootActionKind.None,
                "",
                "Idle",
                [],
                "",
                false));
        }

        return step switch
        {
            WizardStep.Welcome => new WizardCopy(
                "Install apps on your device",
                "Connect a headset or phone first. A USB-C data cable is the usual first step. After the device has approved this computer, you can switch to Wi-Fi and unplug. Then choose the APK files to install.",
                "Start",
                "You will plug in the device, approve a permission if asked, then pick one or more APK files. Wi-Fi setup for Quest 2, Quest 3, Quest 3S, or Quest Pro is later, after the device has approved this computer. Privacy and Terms open from the header. Send a report is available if something goes wrong.",
                "No app is bundled. APK files are chosen after the device is connected."),
            WizardStep.ConnectDevice => new WizardCopy(
                "Connect your device",
                "Plug in with a USB-C data cable, then tap I connected it. You can switch to Wi-Fi later on Choose apps.",
                "I connected it",
                AppendHealth("Charge-only cables will not work. The cable that ships with Quest is often charge-only. Try another USB-C data cable and a USB port on the computer, not a hub. Quest 2, Quest 3, Quest 3S, and Quest Pro use the same cable-first path. For Wi-Fi, the headset and this computer must be on the same network. Pairing codes expire quickly and are only if you already have one. After a headset reboot, plug in with USB once more unless you pair again.", healthHint),
                "Waiting for a connected device."),
            WizardStep.DeviceDetected => new WizardCopy(
                $"{model} detected",
                quest
                    ? "Your headset is connected. Next we will check that it has approved this computer."
                    : "Your phone is connected. Next we will check that it has approved this computer.",
                "Continue",
                "If this is the wrong device, pick it in the list or unplug extras.",
                $"Manufacturer: {device?.Manufacturer}; Android: {device?.AndroidVersion}"),
            WizardStep.Authorization when quest => new WizardCopy(
                "Put on your headset now",
                "A permission message is waiting inside the headset. You may see two messages. Allow USB debugging and Always allow from this computer — not only the files message. Keep the headset on your head, or cover the sensor, so it does not sleep.",
                "I allowed it",
                "You do not have to rush back. This installer notices when the device allows this computer. I allowed it checks now. If you already dismissed the message, unplug and plug the cable back in.",
                "Connection state: unauthorized"),
            WizardStep.Authorization => new WizardCopy(
                "Unlock your phone and allow this computer",
                "Look for a USB debugging prompt on the phone. Check Always allow from this computer, then tap Allow.",
                "I allowed it",
                "You do not have to rush back. This installer notices when the phone allows this computer. Unlock the phone first. If no prompt appears, unplug, plug back in, and set USB mode to File transfer / MTP if the phone asks.",
                "Connection state: unauthorized"),
            WizardStep.DeveloperMode => new WizardCopy(
                "Turn on developer mode",
                "On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on.",
                "I turned it on",
                AppendHealth("You need a Meta developer account on a developer team. After turning it on, connect a USB-C data cable, put the headset on, open Quick Control → Settings → Developer, and turn on MTP Notification. When asked, choose Always allow from this computer. After the headset allows this computer, you can switch to Wi-Fi on Choose apps.", healthHint),
                "Meta Horizon app path: Headset Settings → Developer Mode"),
            WizardStep.ReadyToInstall => new WizardCopy(
                "Choose apps to install",
                device?.IsWireless == true
                    ? $"{model} is ready over Wi-Fi. Add one or more APK files, then install."
                    : $"{model} is ready. Add APK files, or switch to Wi-Fi and unplug.",
                "Install now",
                "Existing copies of the same app may be replaced. Your photos and other apps are not touched. If a file looks like only part of an app, add the other files or an .apks package. Switch to Wi-Fi only after the device has approved this computer.",
                $"Install mode: {manifest.InstallPolicy}"),
            WizardStep.Installing => new WizardCopy(
                userPicked && (string.IsNullOrWhiteSpace(name) || name == "apps") ? "Installing" : $"Installing {name}",
                device?.IsWireless == true
                    ? "Keep the device awake and on the same Wi-Fi as this computer."
                    : "Keep the cable connected. Do not unplug the device.",
                "Cancel",
                "If this sits on one step for several minutes, wait until it finishes or fails. Cancel stops the current attempt.",
                "Sending the selected APK files."),
            WizardStep.InstallProblem => new WizardCopy(
                error is null ? "We could not finish installing" : _messages.TitleFor(error.Value),
                error is null
                    ? "Use the suggested action below. If that does not work, tap Send a report and email it to the person who asked you to test."
                    : _messages.CauseFor(error.Value),
                "Try again",
                "Most failures are a missing permission, a full device, or an older build that cannot be replaced until it is removed.",
                error?.ToString() ?? "UnknownInstallFailure"),
            WizardStep.Complete => new WizardCopy(
                manifest.CanVerifyPackage ? $"{name} is installed" : "Install complete",
                quest
                    ? "Put on the headset and look under Unknown Sources in Library. Headset menus move between software updates, so check the Library filter if you do not see the app."
                    : "Find the app in your app drawer and open it.",
                "Done",
                "If the app does not appear, put the headset on and search Library again. Then send a report.",
                manifest.CanVerifyPackage ? $"Package: {manifest.AppId} {version}" : "Package id is unknown for this file."),
            WizardStep.InstalledApps => new WizardCopy(
                "Installed apps",
                $"{model} has these third-party apps. Search, then remove one at a time.",
                "Back",
                quest
                    ? "Remove deletes the app and its data on the headset. Store apps can come back from the store. Sideloaded apps need the APK again. Library → Unknown Sources may still show a tile until you refresh."
                    : "Remove deletes the app and its data on the phone. Store apps can come back from the store. Sideloaded apps need the APK again.",
                "Third-party apps only. System apps are not listed."),
            _ => new WizardCopy(name, "", "Continue", "", "")
        };
    }

    private static string AppendHealth(string help, string? healthHint) =>
        string.IsNullOrWhiteSpace(healthHint) ? help : help + " " + healthHint;
}
