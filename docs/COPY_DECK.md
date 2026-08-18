# Copy deck

Plain language only. Do not say ADB, sideload, package manager, or USB transport in primary copy. Put those terms in Advanced details.

Primary button is the only required action on each page. Help expander label: **Why am I seeing this?**

Testers get the Windows setup from the GitHub README download button (`SingularityApkInstaller-win-x64-setup.exe`). This deck is in-app copy after that setup has launched.

## Welcome

- Headline: Install apps on your device
- Body: Connect a headset or phone first. A USB-C data cable is the usual first step. After the device has approved this computer, you can switch to Wi-Fi and unplug. Then choose the APK files to install.
- Primary: Start
- Help: You will plug in the device, approve a permission if asked, then pick one or more APK files. Wi-Fi setup for Quest 2 and Quest 3 is on the next screen. Privacy and Terms open from the header.
- Advanced: No app is bundled. APK files are chosen after the device is connected.
- In-page (muted): A newer installer is available, with a Download link to the stable setup.exe. No auto-download.

## Connect device

- Headline: Connect your device
- Body: Plug in with a USB-C data cable, or connect over Wi-Fi using the Quest 2 / Quest 3 steps below.
- Primary: I connected it
- Help: Charge-only cables will not work. The cable that ships with Quest is often charge-only. Try another USB-C data cable and a USB port on the computer, not a hub. For Wi-Fi, the headset and this computer must be on the same network. Pairing codes expire quickly. After a headset reboot, plug in with USB once more unless you pair again.
- Secondary: Export diagnostics (footer)
- In-page health (after failed attempts): Windows sees a headset but this installer does not → data cable + Oculus ADB driver. Windows sees nothing → cable/hub.
- In-page: USB card (cable, then **I connected it**). Wi-Fi card is always visible.
- In-page: **Connect over Wi-Fi** as a raised card when a last address is saved (one tap). Same network; after a reboot, plug in once more.
- In-page expander (open when no saved address): **How to set up Wi-Fi on Quest 2 or Quest 3** — numbered USB-once path (Horizon app Developer Mode → data cable → Quick Control → Settings → Developer → MTP Notification → Always allow → **Switch to Wi-Fi** on Choose apps).
- In-page expander: **I already have a pairing code from the headset** — same Wi-Fi, Settings → System → Developer → wireless debugging, install address vs pairing port, then the form.
- In-page form: install address (IP, port 5555 if omitted), optional pairing port and six-digit code, then **Connect over Wi-Fi**. Pairing numbers are not the install port.
- Advanced: Last detection status and raw device list.

## Device detected

- Headline (Quest): {model} detected
- Headline (phone): {model} detected
- Body (Quest): Your headset is connected. Next we will check that it has approved this computer.
- Body (phone): Your phone is connected. Next we will check that it has approved this computer.
- Primary: Continue
- Help: If this is the wrong device, pick it in the list or unplug extras so only one device is connected.
- In-page: when two or more ready devices are connected, a list (model, USB vs Wi-Fi). Continue uses the selected row.
- Advanced: Serial (hashed in logs), manufacturer, Android version, classification.

## Authorization (Quest)

- Headline: Put on your headset now
- Body: A permission message is waiting inside the headset. Select Always allow from this computer, then choose Allow.
- Primary: I allowed it
- Secondary: Export diagnostics (footer)
- Help: The headset must be on your head, awake, and showing the USB debugging prompt. If you already dismissed it, unplug and plug the cable back in.
- Advanced: Connection state `unauthorized`.

## Authorization (phone)

- Headline: Unlock your phone and allow this computer
- Body: Look for a USB debugging prompt on the phone. Check Always allow from this computer, then tap Allow.
- Primary: I allowed it
- Secondary: Export diagnostics (footer)
- Help: Unlock the phone first. If no prompt appears, unplug, plug back in, and set USB mode to File transfer / MTP if the phone asks.
- Advanced: Connection state `unauthorized`.

## Developer mode (Quest only)

- Headline: Turn on developer mode
- Body: On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on.
- Primary: I turned it on
- Secondary: Export diagnostics (footer)
- Help: You need a Meta developer account on a developer team. After turning it on, connect a USB-C data cable, put the headset on, open Quick Control → Settings → Developer, and turn on MTP Notification. When asked, choose Always allow from this computer. After the headset allows this computer, you can switch to Wi-Fi on Choose apps.
- Advanced: Link to Meta device-setup docs.

## Choose apps to install

- Headline: Choose apps to install
- Body (USB): {model} is ready. Add APK files, or switch to Wi-Fi and unplug.
- Body (Wi-Fi): {model} is ready over Wi-Fi. Add one or more APK files, then install.
- Primary: Install now (disabled until at least one APK is added)
- Secondary in page: Add app files (multi-select `*.apk;*.apks;*.xapk`; drag-and-drop onto the page)
- Secondary in page: Use last files (when previous paths still exist)
- Secondary in page: **Switch to Wi-Fi** card (only when the device is USB and ready). After this you can unplug.
- In-page: **Connected over Wi-Fi** card when the session is already wireless.
- Secondary in page: **Installed apps** card. Opens the Installed apps step.
- Warning: This looks like only part of an app. Add the other files or an .apks package.
- Help: Existing copies of the same app may be replaced. Your photos and other apps are not touched. Switch to Wi-Fi only after the device has approved this computer.
- Advanced: Policy name, package id, and split set.

## Installing

- Headline: Installing {label or “N apps”}
- Body: Keep the cable connected. Do not unplug the device. (Wi-Fi: Keep the device awake and on the same Wi-Fi as this computer.)
- Status labels: Installing 1 of N: package label or filename
- Primary: none (page is busy)
- Secondary: Cancel
- Help: If this sits on one step for several minutes, wait until it finishes or fails. Cancel stops the current attempt.
- Advanced: Current command display name and elapsed time.

## Install problem

- Headline: We could not finish installing
- Body: Use the suggested action below. If that does not work, export a diagnostics file and send it to support.
- Primary: Try automatic fix (when an auto-fix exists) or Try again
- Secondary: Export diagnostics
- In-page: **Replace this app** / **Remove this app and install** when the package id is known (already exists, downgrade, or signature mismatch)
- Help: Most failures are a missing permission, a full device, or an older build that cannot be replaced until it is removed.
- Advanced: Typed error, exit code, sanitized output.

### Problem titles (use instead of generic headline when known)

| Error | Title | Likely cause |
| --- | --- | --- |
| UnauthorizedDevice | Your device has not approved this computer yet | USB debugging prompt not accepted |
| OfflineDevice | The device disconnected | Cable, sleep, or USB mode |
| NoDevicesFound | No device was found | Cable, driver, or developer mode |
| VersionDowngrade | An older incompatible version is already installed | Test build number went backwards |
| PackageAlreadyExists | This app is already installed | Reinstall policy needed |
| SignatureMismatch | A different copy of this app is already installed | Old build signed with another key |
| InsufficientStorage | The device does not have enough free space | Headset/phone storage full |
| DeveloperModeLikelyDisabled | Developer mode may still be off | Quest not visible to this computer |
| CableOrUsbModeIssue | The cable or USB mode looks wrong | Charge-only cable or USB mode |
| MissingPayload | The APK file could not be found | Selected file missing or moved |
| MissingSplit | This looks like only part of the app | Base APK or remaining splits missing |
| WirelessConnectFailed | Wi-Fi connection did not work | Wrong network, reboot cleared wireless, or pairing port vs connect port |
| UninstallFailed | The app could not be removed | Device asleep, protected app, or removal blocked |
| UnknownInstallFailure | The install did not complete | See advanced details |

## Complete

- Headline: Install complete (or {name} is installed when the package id is known)
- Body (Quest): Put on the headset and look under Unknown Sources in Library. Headset menus move between software updates, so check the Library filter if you do not see the app.
- Body (phone): Find the app in your app drawer and open it.
- Primary: Done
- Secondary: Export diagnostics
- In-page: **Open on device** when the package id resolved (does not auto-launch)
- In-page: **Installed apps**
- Help: If the app does not appear, unplug, put the headset on, and search Library again. Then export diagnostics.
- Advanced: Package id and version when parsed; otherwise package id is unknown.

## Installed apps

- Headline: Installed apps
- Body: {model} has these third-party apps. Search, then remove one at a time.
- Primary: Back
- Secondary: Export diagnostics (footer). Cancel in the footer while a removal is in progress.
- In-page: search, refresh, list of third-party apps (name when known, package id in mono). **Remove** opens a confirm card.
- Confirm: Remove deletes the app and its data. Store apps can come back from the store. Sideloaded apps need the APK again.
- Help (Quest): Remove deletes the app and its data on the headset. Library → Unknown Sources may still show a tile until you refresh.
- Help (phone): Remove deletes the app and its data on the phone.
- Advanced: Third-party apps only. System apps are not listed.
