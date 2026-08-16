# Copy deck

Plain language only. Do not say ADB, sideload, package manager, or USB transport in primary copy. Put those terms in Advanced details.

Primary button is the only required action on each page. Help expander label: **Why am I seeing this?**

Testers get the Windows setup from the GitHub README download button (`SingularityApkInstaller-win-x64-setup.exe`). This deck is in-app copy after that setup has launched.

## Welcome

- Headline: Install {displayName}
- Body: This tool installs {displayName} ({buildVersion}) on your headset or phone. You only need a USB cable and about two minutes.
- Primary: Start
- Help: You will plug in the device, approve a permission if asked, then this app does the rest.
- Advanced: Shows bundled app id, policy, and payload path.

## Connect device

- Headline: Connect your device
- Body: Plug the headset or phone into this computer with a USB cable that can transfer files, then wait a moment.
- Primary: I connected it
- Help: Charge-only cables will not work. The cable that ships with Quest is often charge-only. Try another USB-C data cable and a USB port on the computer, not a hub.
- Advanced: Last detection status and raw device list.

## Device detected

- Headline (Quest): {model} detected
- Headline (phone): {model} detected
- Body (Quest): Your headset is connected. Next we will check that it has approved this computer.
- Body (phone): Your phone is connected. Next we will check that it has approved this computer.
- Primary: Continue
- Help: If this is the wrong device, unplug extras so only one device is connected.
- Advanced: Serial (hashed in logs), manufacturer, Android version, classification.

## Authorization (Quest)

- Headline: Put on your headset now
- Body: A permission message is waiting inside the headset. Select Always allow from this computer, then choose Allow.
- Primary: I allowed it
- Help: The headset must be on your head, awake, and showing the USB debugging prompt. If you already dismissed it, unplug and plug the cable back in.
- Advanced: Connection state `unauthorized`.

## Authorization (phone)

- Headline: Unlock your phone and allow this computer
- Body: Look for a USB debugging prompt on the phone. Check Always allow from this computer, then tap Allow.
- Primary: I allowed it
- Help: Unlock the phone first. If no prompt appears, unplug, plug back in, and set USB mode to File transfer / MTP if the phone asks.
- Advanced: Connection state `unauthorized`.

## Developer mode (Quest only)

- Headline: Turn on developer mode
- Body: On your phone, open the Meta Horizon app. Tap the headset icon, then your headset, then Headset Settings, then Developer Mode, and turn it on.
- Primary: I turned it on
- Help: You need a Meta developer account on a developer team. After turning it on, connect a USB-C data cable, put the headset on, open Quick Control → Settings → Developer, and turn on MTP Notification. When asked, choose Always allow from this computer.
- Advanced: Link to Meta device-setup docs.

## Ready to install

- Headline: Ready to install {displayName}
- Body: {displayName} {buildVersion} will be installed on {model}. This usually takes less than a minute.
- Primary: Install now
- Help: Existing test builds may be replaced according to the selected install mode. Your photos and other apps are not touched.
- Advanced: Policy name and install flags.

## Installing

- Headline: Installing {displayName}
- Body: Keep the cable connected. Do not unplug the device.
- Status labels: Starting → Sending app → Installing → Verifying
- Primary: none (page is busy)
- Secondary: Cancel
- Help: If this sits on one step for several minutes, wait until it finishes or fails. Cancel stops the current attempt.
- Advanced: Current command display name and elapsed time.

## Install problem

- Headline: We could not finish installing
- Body: Use the suggested action below. If that does not work, export a diagnostics file and send it to support.
- Primary: Try automatic fix (when an auto-fix exists) or Try again
- Secondary: Export diagnostics
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
| UnknownInstallFailure | The install did not complete | See advanced details |

## Complete

- Headline: {displayName} is installed
- Body (Quest): Put on the headset and look for {displayName} under Unknown Sources in Library. Headset menus move between software updates, so check the Library filter if you do not see it.
- Body (phone): Find {displayName} in your app drawer and open it.
- Primary: Done
- Secondary: Export diagnostics
- Help: If the app does not appear, unplug, put the headset on, and search Library again. Then export diagnostics.
- Advanced: Installed package id and version if known.
