# Copy deck

Plain language only. Do not say ADB, sideload, package manager, or USB transport in primary copy. Put those terms in Advanced details.

Primary button is the only required action on each page. Help expander label: **Why am I seeing this?**

Testers get the Windows setup from the GitHub README download button (`SingularityApkInstaller-win-x64-setup.exe`). This deck is in-app copy after that setup has launched.

## Pilot helper

Always-on companion (docked on the right, or popped out as a second window). Plain language. One current action. Never says ADB, sideload, or USB transport.

- Name: Pilot
- Greeting: short, calm, first person
- Now: what to do this moment
- Then: what happens next
- Button hint: When that is done, press {Primary}
- Checks: 0–3 short safety notes
- Progress: 1 of 6 … 6 of 6 (Help / Apps for side paths)
- Hide helper / Show helper / Pop out / Dock. Closing the popped-out window docks it back. F1 opens Why am I seeing this?

When the helper is visible, the main column hides the long body, illustration, and help expanders so the step fits on one screen. The green primary button stays on the main window.

| Step | Now (summary) |
| --- | --- |
| Welcome | You are about to install an app. One thing at a time. |
| Connect | Plug in with a USB-C cable that can copy files. |
| Detected | The device is connected. Press Continue. |
| Authorization (Quest) | Put the headset on. Two messages: debugging Allow, not only files. I will notice when you allow it. |
| Authorization (phone) | Unlock the phone. Always allow, then Allow. |
| Developer mode | Meta Horizon app → Headset Settings → Developer Mode. |
| Choose apps (empty) | Press Add app files and choose the .apk you were sent. |
| Choose apps (ready) | Press Install now. Leave the device as it is. |
| Installing | Please wait. Do not unplug. |
| Problem | Use the suggested action. You are not in trouble. |
| Complete (Quest) | Library → Unknown Sources. |
| Complete (phone) | Find the app in the app list. |
| Installed apps | Remove one at a time, then Back. |

## Welcome

- Headline: Install apps on your device
- Body: Connect a headset or phone first. A USB-C data cable is the usual first step. After the device has approved this computer, you can switch to Wi-Fi and unplug. Then choose the APK files to install.
- Primary: Start
- Help: You will plug in the device, approve a permission if asked, then pick one or more APK files. Wi-Fi setup for Quest 2, Quest 3, Quest 3S, or Quest Pro is later, after the device has approved this computer. Privacy and Terms open from the header. **Send a report** is always available if something goes wrong.
- If a headset or phone is already connected when they press Start, skip Connect / Device detected and jump to Authorization or Choose apps.
- Advanced: No app is bundled. APK files are chosen after the device is connected.
- In-page (muted): A newer installer is available, with a Download link to the stable setup.exe. No auto-download.

## Connect device

- Headline: Connect your device
- Body: Plug in with a USB-C data cable, then tap I connected it. You can switch to Wi-Fi later on Choose apps.
- Primary: I connected it
- Help: Charge-only cables will not work. The cable that ships with Quest is often charge-only. Try another USB-C data cable and a USB port on the computer, not a hub. Quest 2, Quest 3, Quest 3S, and Quest Pro use the same cable-first path. For Wi-Fi, the headset and this computer must be on the same network. Pairing codes expire quickly and are only if you already have one. After a headset reboot, plug in with USB once more unless you pair again.
- Secondary: **Send a report** (footer)
- Secondary: **Need help connecting?** opens a compact connection helper window (Quest vs phone, one task per screen). Two failed **I connected it** attempts open the same helper. The helper closes when a device is ready, or when Leave helper is used. If exactly one device appears, skip Device detected.
- In-page health (after failed attempts): Windows sees a headset but this installer does not → data cable + Oculus ADB driver. Windows sees nothing → cable/hub. Status chip when Windows sees USB but this installer does not.
- In-page: **Connect over Wi-Fi** as a compact saved-address row when a last address is saved (one tap). Same network; after a reboot, plug in once more.
- In-page expander: **Connect over Wi-Fi instead** — address form, collapsed until opened. Wi-Fi guide and pairing expanders stay collapsed until opened.
- In-page expander: **How to set up Wi-Fi on Quest 2, Quest 3, Quest 3S, or Quest Pro** — numbered USB-once path (Horizon app Developer Mode → data cable → Quick Control → Settings → Developer → MTP Notification → Always allow → **Switch to Wi-Fi** on Choose apps).
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
- In-page: skipped when exactly one device is connected (goes straight to Authorization or Choose apps). When two or more ready devices are connected, a list (model, USB vs Wi-Fi). Continue uses the selected row.
- Advanced: Serial (hashed in logs), manufacturer, Android version, classification.

## Authorization (Quest)

- Headline: Put on your headset now
- Body: A permission message is waiting inside the headset. You may see two messages. Allow USB debugging and Always allow from this computer — not only the files message. Keep the headset on your head, or cover the sensor, so it does not sleep.
- Primary: I allowed it
- Secondary: **Send a report** (footer)
- Help: You do not have to rush back. This installer notices when the device allows this computer. **I allowed it** checks now. If you already dismissed the message, unplug and plug the cable back in.
- Advanced: Connection state `unauthorized`.

## Authorization (phone)

- Headline: Unlock your phone and allow this computer
- Body: Look for a USB debugging prompt on the phone. Check Always allow from this computer, then tap Allow.
- Primary: I allowed it
- Secondary: **Send a report** (footer)
- Help: You do not have to rush back. This installer notices when the phone allows this computer. Unlock the phone first. If no prompt appears, unplug, plug back in, and set USB mode to File transfer / MTP if the phone asks.
- Advanced: Connection state `unauthorized`.

## Developer mode (Quest only)

- Headline: Turn on developer mode
- Body: On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on.
- Primary: I turned it on
- Secondary: **Send a report** (footer)
- Help: You need a Meta developer account on a developer team. After turning it on, connect a USB-C data cable, put the headset on, open Quick Control → Settings → Developer, and turn on MTP Notification. When asked, choose Always allow from this computer. After the headset allows this computer, you can switch to Wi-Fi on Choose apps.
- Advanced: Link to Meta device-setup docs.

## Choose apps to install

- Headline: Choose apps to install
- Body (USB): {model} is ready. Add APK files, or switch to Wi-Fi and unplug.
- Body (Wi-Fi): {model} is ready over Wi-Fi. Add one or more APK files, then install.
- Primary: Install now (disabled until at least one APK is added)
- Secondary in page: Add app files (multi-select `*.apk;*.apks;*.xapk`; drag-and-drop onto the page)
- Secondary in page: Use last files (when previous paths still exist)
- Secondary in page: compact **Switch to Wi-Fi** (only when the device is USB and ready). After this you can unplug.
- In-page: muted **Connected over Wi-Fi** when the session is already wireless.
- Secondary in page: compact **Installed apps**. Opens the Installed apps step.
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
- Body: Use the suggested action below. If that does not work, tap Send a report and email it to the person who asked you to test.
- Primary: Try again (non-destructive). Signature mismatch still requires **Remove this app and install**.
- Secondary: **Send a report**
- In-page: **Replace this app** / **Remove this app and install** when the package id is known (already exists, downgrade, or signature mismatch)
- Help: Most failures are a missing permission, a full device, or an older build that cannot be replaced until it is removed.
- Advanced: Typed error, exit code, sanitized output.

### Problem titles (use instead of generic headline when known)

| Error | Title | Likely cause |
| --- | --- | --- |
| UnauthorizedDevice | Your device has not approved this computer yet | USB debugging prompt not accepted (includes unused DebuggingNotApproved) |
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
- Secondary: **Send a report**
- In-page: **Open on device** when the package id resolved (does not auto-launch)
- In-page: **Installed apps**
- Help: If the app does not appear, unplug, put the headset on, and search Library again. Then send a report.
- Advanced: Package id and version when parsed; otherwise package id is unknown.

## Installed apps

- Headline: Installed apps
- Body: {model} has these third-party apps. Search, then remove one at a time.
- Primary: Back
- Secondary: **Send a report** (footer). Cancel in the footer while a removal is in progress.
- In-page: search, refresh, list of third-party apps (name when known, package id in mono). **Remove** opens a confirm card.
- Confirm: Remove deletes the app and its data. Store apps can come back from the store. Sideloaded apps need the APK again.
- Help (Quest): Remove deletes the app and its data on the headset. Library → Unknown Sources may still show a tile until you refresh.
- Help (phone): Remove deletes the app and its data on the phone.
- Advanced: Third-party apps only. System apps are not listed.

## Troubleshoot

Owned modal helper window from Connect, Authorization, Developer mode, or a connection-lost Install problem. The main installer waits. Primary copy never says ADB. Leave helper or closing the window returns to the previous screen. A ready device closes the helper immediately.

- Headline / primary depend on the current node (What are you connecting?, cable, wear headset, Meta account, developer mode, MTP Notification, two-prompt allow, USB helper, restart helper, phone USB mode, Samsung Auto Blocker, Wi-Fi rescue, reboot, still stuck).
- Primary: **I plugged it in** / **I have it on** / **I checked it** / **I turned it on** / **I turned it off** / **I allowed it** / **Check again** for confirm-only nodes.
- Quest: **Check your Meta account** before developer mode when USB does not yet show the headset (18+, verified, developer team, same Horizon account). Skip if Windows already sees the headset.
- Allow: two popups — USB debugging / Always allow, not only the files message. Keep the headset on (or cover the sensor). Pilot: I will notice when you allow it.
- Restart helper: close Chrome/Edge SideQuest Web Installer, MQDH, SideQuest desktop, Android Studio.
- Phone: **Turn off Auto Blocker** on Samsung after file transfer (Settings → Security and privacy → Auto Blocker → Block commands by USB). USB debugging on Samsung uses About phone → Software information → Build number, then Developer options at the bottom of Settings.
- Wi-Fi rescue: same network, no guest, turn off VPN, plug in once more after reboot. Pairing is not the default.
- Primary on helper-action nodes: **Restart connection helper** (restarts the helper this installer uses, then rescan; stay on this node if it fails), **Install Quest USB support** / **Get Quest USB support**, **Open USB support page**. After USB helper opens or installs, **I installed it** confirms.
- In-page: family picker (Quest vs phone); numbered steps; **I installed it** after USB helper actions.
- Secondary: **Send a report** (helper footer). Opens a window asking for the email of the person who asked them to test, then opens their email app with the report attached. Includes `session-log.txt`. They still press Send in the email app.
- Help: account age / developer team for Quest developer mode; pairing port vs install address for Wi-Fi.
- Advanced: ADB/driver terms allowed here only.
