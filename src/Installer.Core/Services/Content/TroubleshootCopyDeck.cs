using Installer.Core.Abstractions;
using Installer.Core.Models;

namespace Installer.Core.Services.Content;

public sealed class TroubleshootCopyDeck
{
    public WizardCopy Page(TroubleshootSession session)
    {
        return (session.Family, session.CurrentNode) switch
        {
            (_, TroubleshootNode.PickDevice) => new WizardCopy(
                "What are you connecting?",
                "Pick the device you want to set up. You can change this later.",
                "Continue",
                "Quest 2, Quest 3, Quest 3S, and Quest Pro use the Meta Horizon phone app for developer mode. Phones use the developer settings on the phone itself.",
                "Device family picker."),
            (TroubleshootFamily.MetaQuest, TroubleshootNode.CableAndPort) => new WizardCopy(
                "Use a USB-C data cable",
                "Plug the headset into a USB port on this computer. Do not use a hub, dock, or the short cable from the Quest box unless you know it can transfer files.",
                "I plugged it in",
                "A charge-only cable can still power the headset. Windows will not see it. Try a rear port on a desktop.",
                "USB data cable and motherboard port."),
            (TroubleshootFamily.AndroidPhone, TroubleshootNode.CableAndPort) => new WizardCopy(
                "Use a USB-C data cable",
                "Plug the phone into a USB port on this computer. Do not use a hub or a charge-only cable.",
                "I plugged it in",
                "Try a port on the computer itself, not a keyboard or monitor hub.",
                "USB data cable and motherboard port."),
            (_, TroubleshootNode.WearHeadset) => new WizardCopy(
                "Put the headset on",
                "Keep it on and awake. The next permission appears inside the headset, not on this computer.",
                "I have it on",
                "If the headset sleeps, press the power button and put it on again. Leave the cable plugged in.",
                "Headset must be worn for the USB debugging prompt."),
            (_, TroubleshootNode.DeveloperAccount) => new WizardCopy(
                "Check your Meta account",
                "You need to be 18 or older, with a verified Meta account, on a developer team. The Meta Horizon app on your phone must be signed in with that same account.",
                "I checked it",
                "A work or school Meta account is not enough. Use the same account that is signed into the headset.",
                "Meta developer team membership."),
            (_, TroubleshootNode.DeveloperMode) => new WizardCopy(
                "Turn on developer mode",
                "On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on. Restart the headset if you just switched it on.",
                "I turned it on",
                "You need a Meta account that is at least 18, verified, and on a developer team. Use the same account that is signed into the headset.",
                "Meta Horizon app: Headset Settings → Developer Mode."),
            (_, TroubleshootNode.MtpNotification) => new WizardCopy(
                "Turn on MTP Notification",
                "On the headset, open Quick Control, then Settings (gear), then Developer, and turn on MTP Notification.",
                "I turned it on",
                "This is a headset setting, not a phone setting. Leave the cable plugged in while you do it.",
                "Quick Control → Settings → Developer → MTP Notification."),
            (_, TroubleshootNode.AllowComputer) => new WizardCopy(
                "Allow this computer",
                "Put the headset on. You may see two messages. Allow USB debugging and Always allow from this computer — not only the files message. Keep the headset on your head, or cover the sensor, so it does not sleep.",
                "I allowed it",
                "The debugging message is only in the headset. Stay in the headset. This installer notices when you allow it. If you already dismissed it, unplug and plug the cable back in.",
                "USB debugging authorization prompt, not MTP file-access."),
            (_, TroubleshootNode.UsbHelper) => new WizardCopy(
                "Install Quest USB support",
                "Windows can see the headset, but this installer still cannot talk to it. Install Meta’s USB helper on this computer, then check again.",
                "I installed it",
                "This is a one-time Windows step. You may see a permission prompt from Windows. After it finishes, keep the headset plugged in.",
                "Oculus ADB drivers / android_winusb.inf via pnputil."),
            (_, TroubleshootNode.RestartHelper) => new WizardCopy(
                "Restart the connection helper",
                session.Evidence.CompetingAdbProcess
                    ? "Another Android tool may be using the connection. Close Chrome or Edge if SideQuest Web Installer is open, and close Meta Quest Developer Hub, SideQuest, or Android Studio. Then restart the helper."
                    : "Restart the helper this installer uses, then we will look for the device again. Keep the device plugged in and awake.",
                "Restart connection helper",
                "If Chrome, Edge, SideQuest, Meta Quest Developer Hub, or Android Studio is open, close it first. Then tap Restart connection helper.",
                "adb kill-server / start-server."),
            (_, TroubleshootNode.PhoneUnlock) => new WizardCopy(
                "Unlock your phone",
                "Wake the phone and unlock it. Keep it unlocked while we finish setup.",
                "The phone is unlocked",
                "A locked phone often hides the USB permission.",
                "Phone must be unlocked."),
            (_, TroubleshootNode.PhoneUsbMode) => new WizardCopy(
                "Turn on file transfer",
                PhoneUsbModeBody(session.Device),
                "I turned on file transfer",
                "If you only see charging, tap the USB notification on the phone and pick file transfer.",
                "MTP / file transfer USB mode."),
            (_, TroubleshootNode.PhoneDebugging) => new WizardCopy(
                "Turn on USB debugging",
                PhoneDebugBody(session.Device),
                "I turned it on",
                "A work phone may block this. Use a personal Galaxy or ask IT. Do not try to bypass a work profile.",
                "Android developer options → USB debugging."),
            (_, TroubleshootNode.PhoneAutoBlocker) => new WizardCopy(
                "Turn off Auto Blocker",
                "On a Samsung phone, Auto Blocker can stop this computer from installing apps. Open Settings, then Security and privacy, then Auto Blocker, and turn off Block commands by USB. The wording may vary.",
                "I turned it off",
                "A work phone may block this. Use a personal Galaxy or ask IT. Do not try to bypass a work profile.",
                "Samsung Auto Blocker / Knox USB restriction."),
            (_, TroubleshootNode.PhoneAllow) => new WizardCopy(
                "Allow this computer",
                "Look on the phone for a USB debugging prompt. Check Always allow from this computer, then tap Allow.",
                "I allowed it",
                "Unlock the phone first. Unplug and plug back in if the prompt is gone.",
                "USB debugging authorization prompt."),
            (_, TroubleshootNode.PhoneOemDriver) => new WizardCopy(
                "Install phone USB support",
                "Windows may need a USB helper from the phone maker. Open the support page, install it, then check again.",
                "I installed it",
                "Pixel phones often work after Google’s USB helper. Samsung phones usually need Samsung’s USB helper.",
                "OEM USB driver."),
            (_, TroubleshootNode.WifiRescue) => new WizardCopy(
                "Try Wi-Fi after USB works",
                "Keep the headset and this computer on the same Wi-Fi. Guest networks will not work. Turn off a VPN if one is on. After a headset reboot, plug in with a cable once more. Pairing codes are only if you already have one, and they expire quickly.",
                "Check again",
                "Wi-Fi setup needs the headset to allow this computer over a cable first. Then use Switch to Wi-Fi on Choose apps.",
                "USB-first wireless: adb tcpip then connect. Pairing port is not the install port."),
            (_, TroubleshootNode.RebootDevice) => new WizardCopy(
                "Restart the device and this computer",
                session.Family == TroubleshootFamily.AndroidPhone
                    ? "Restart the phone, then restart this computer. Plug in with a data cable after both have started."
                    : "Restart the headset, then restart this computer. Plug in with a data cable after both have started, and put the headset on.",
                "I restarted them",
                "Wait until Windows has finished starting before you open this installer again.",
                "Reboot device and PC."),
            (_, TroubleshootNode.StillStuck) => new WizardCopy(
                "We still cannot see the device",
                "Send a report to the person who asked you to install the app. Keep the device plugged in.",
                "Check again",
                "The report does not include your name or the raw device serial. Press Send in your email app.",
                "Export diagnostics ZIP including session-log.txt."),
            _ => new WizardCopy(
                "Need help connecting?",
                "Follow the step on this page, then check again.",
                "Check again",
                "You can leave this helper and return to Connect.",
                "Troubleshoot side-flow.")
        };
    }

    public IReadOnlyList<string> Steps(TroubleshootSession session)
    {
        return session.CurrentNode switch
        {
            TroubleshootNode.CableAndPort when session.Family == TroubleshootFamily.MetaQuest =>
            [
                "Find a USB-C cable that can transfer files, not the short one from the Quest box unless you know it works.",
                "Plug one end into the headset and the other into a USB port on this computer, not a hub.",
                "Wait a few seconds. Windows may ask what to do with the device — that is fine."
            ],
            TroubleshootNode.CableAndPort =>
            [
                "Use a USB-C cable that can transfer files.",
                "Plug into a USB port on this computer, not a hub.",
                "Unlock the phone if it is locked."
            ],
            TroubleshootNode.WearHeadset =>
            [
                "Put the headset on.",
                "If the display is dark, press the power button.",
                "Keep the cable plugged in."
            ],
            TroubleshootNode.DeveloperAccount =>
            [
                "On your phone, open the Meta Horizon app.",
                "Confirm you are signed in with the same account as the headset.",
                "That account must be 18 or older, verified, and on a developer team."
            ],
            TroubleshootNode.DeveloperMode =>
            [
                "Open the Meta Horizon app on your phone.",
                "Tap the headset icon, then your Quest, then Headset Settings.",
                "Turn on Developer Mode. Restart the headset if you just turned it on."
            ],
            TroubleshootNode.MtpNotification =>
            [
                "Put the headset on, still plugged in.",
                "Open Quick Control, then Settings (gear), then Developer.",
                "Turn on MTP Notification."
            ],
            TroubleshootNode.AllowComputer =>
            [
                "Put the headset on and keep it awake.",
                "Allow USB debugging, then Always allow from this computer. Skip the files-only popup if that is all you see.",
                "Stay in the headset. This installer notices when you allow it."
            ],
            TroubleshootNode.UsbHelper =>
            [
                "Tap Install Quest USB support or Get Quest USB support.",
                "If Windows asks for permission, choose Yes.",
                "Keep the headset plugged in, then tap I installed it."
            ],
            TroubleshootNode.RestartHelper => session.Evidence.CompetingAdbProcess
                ?
                [
                    "Close Chrome or Edge if SideQuest Web Installer is open.",
                    "Close Meta Quest Developer Hub, SideQuest, and Android Studio if they are open.",
                    "Keep the device plugged in and awake, then tap Restart connection helper."
                ]
                :
                [
                    "Keep the device plugged in and awake.",
                    "Tap Restart connection helper."
                ],
            TroubleshootNode.PhoneUnlock =>
            [
                "Wake the phone.",
                "Unlock it with your PIN, pattern, or fingerprint.",
                "Leave it unlocked."
            ],
            TroubleshootNode.PhoneUsbMode => PhoneUsbModeSteps(session.Device),
            TroubleshootNode.PhoneDebugging => PhoneDebugSteps(session.Device),
            TroubleshootNode.PhoneAutoBlocker =>
            [
                "Settings → Security and privacy → Auto Blocker.",
                "Turn off Block commands by USB. The wording may vary.",
                "Use a personal Galaxy if this is a work phone."
            ],
            TroubleshootNode.PhoneAllow =>
            [
                "Look on the phone screen for USB debugging.",
                "Check Always allow from this computer.",
                "Tap Allow."
            ],
            TroubleshootNode.PhoneOemDriver =>
            [
                "Tap Open USB support page.",
                "Install the helper they provide.",
                "Plug the phone back in, then tap I installed it."
            ],
            TroubleshootNode.WifiRescue =>
            [
                "Same Wi-Fi as this computer. Not a guest network. Turn off VPN.",
                "Plug in with a data cable first and allow this computer if asked.",
                "After this installer shows the headset is ready, use Switch to Wi-Fi on Choose apps."
            ],
            TroubleshootNode.RebootDevice =>
            [
                "Restart the headset or phone.",
                "Restart this computer.",
                "Open this installer, plug in with a data cable, and put the headset on if you are using a Quest."
            ],
            TroubleshootNode.StillStuck =>
            [
                "Keep the device plugged in.",
                "Tap Send a report on this page, or Send a report below.",
                "Enter their email. Press Send in your email app."
            ],
            _ => []
        };
    }

    public string ActionLabel(TroubleshootSession session, bool canInstallInf)
    {
        return session.RecommendedAction switch
        {
            TroubleshootActionKind.RestartAdbServer => "Restart connection helper",
            TroubleshootActionKind.InstallUsbHelper when canInstallInf => "Install Quest USB support",
            TroubleshootActionKind.InstallUsbHelper => "Get Quest USB support",
            TroubleshootActionKind.OpenDriverDownload => "Get Quest USB support",
            TroubleshootActionKind.OpenPhoneUsbSupport => "Open USB support page",
            TroubleshootActionKind.ExportDiagnostics => "Send a report",
            _ => ""
        };
    }

    public (string Chip, string Tone) Status(TroubleshootSession session, IReadOnlyList<DeviceInfo> devices)
    {
        if (devices.Any(d => d.State == DeviceConnectionState.ConnectedReady))
        {
            return ("Device ready", "Live");
        }

        if (devices.Any(d => d.State == DeviceConnectionState.Unauthorized))
        {
            return session.Family == TroubleshootFamily.AndroidPhone
                ? ("This computer sees the phone, but the phone has not allowed it yet.", "Warning")
                : ("This computer sees the headset, but the headset has not allowed it yet.", "Warning");
        }

        if (session.Evidence.AdbDriverMissing)
        {
            return ("This computer sees the headset, but USB support is missing.", "Warning");
        }

        if (session.Evidence.MtpOnly || (session.Evidence.QuestUsbPresent && !session.Evidence.AdbInterfacePresent))
        {
            return ("This computer sees the headset, but the headset has not allowed this installer yet.", "Warning");
        }

        if (session.Evidence.CompetingAdbProcess)
        {
            return ("Another Android tool may be blocking the connection.", "Warning");
        }

        if (session.Evidence.WindowsSeesUsb)
        {
            return ("This computer sees a device, but this installer does not.", "Warning");
        }

        return ("This computer does not see a headset or phone.", "Idle");
    }

    private static string PhoneUsbModeBody(DeviceInfo? device)
    {
        if (IsSamsung(device))
        {
            return "On a Samsung phone, pull down the USB notification and choose Transfer files / Android Auto, or File transfer. Charging only will not work.";
        }

        if (IsPixel(device))
        {
            return "On a Pixel, pull down the USB notification and choose File transfer / Android Auto. Charging only will not work.";
        }

        return "Pull down the USB notification on the phone and choose File transfer or MTP. Charging only will not work. Samsung and Pixel labels differ slightly.";
    }

    private static IReadOnlyList<string> PhoneUsbModeSteps(DeviceInfo? device)
    {
        if (IsSamsung(device))
        {
            return
            [
                "Unlock the phone.",
                "Open the USB notification.",
                "Choose Transfer files / Android Auto."
            ];
        }

        if (IsPixel(device))
        {
            return
            [
                "Unlock the phone.",
                "Open the USB notification.",
                "Choose File transfer / Android Auto."
            ];
        }

        return
        [
            "Unlock the phone.",
            "Open the USB notification.",
            "Choose File transfer, MTP, or Transfer files."
        ];
    }

    private static IReadOnlyList<string> PhoneDebugSteps(DeviceInfo? device)
    {
        if (IsSamsung(device))
        {
            return
            [
                "Settings → About phone → Software information.",
                "Tap Build number seven times.",
                "Settings → Developer options → USB debugging on."
            ];
        }

        return
        [
            "Settings → About phone.",
            "Tap Build number seven times.",
            "System → Developer options → USB debugging on."
        ];
    }

    private static string PhoneDebugBody(DeviceInfo? device)
    {
        if (IsSamsung(device))
        {
            return "On a Samsung phone, open Settings, then About phone, then Software information. Tap Build number seven times. Go back, open Developer options at the bottom of Settings, and turn on USB debugging.";
        }

        return "On the phone, open Settings, then About phone, tap Build number seven times, go back to System, then Developer options, and turn on USB debugging.";
    }

    private static bool IsSamsung(DeviceInfo? device) =>
        Contains(device?.Manufacturer, "samsung") || Contains(device?.Model, "SM-") || Contains(device?.Model, "Galaxy");

    private static bool IsPixel(DeviceInfo? device) =>
        Contains(device?.Manufacturer, "google") || Contains(device?.Model, "Pixel");

    private static bool Contains(string? value, string needle) =>
        !string.IsNullOrEmpty(value) && value.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
