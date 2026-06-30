# Blue Prism + Microsoft Edge

Field-tested troubleshooting and configuration guides for running Blue Prism
browser automation against Microsoft Edge (Chromium) in corporate Windows
environments.

## Guides

| Document | What it covers |
|---|---|
| [`blue-prism-edge-native-messaging-fix.md`](./blue-prism-edge-native-messaging-fix.md) | Root-cause fix for the *"The browser extension could not be detected — no active native messaging host found"* error. Covers Edge Background Mode / zombie processes silently swallowing launch flags, the validated launch command line (flag-by-flag), required registry policies, a pre-launch process-cleanup Code Stage, loading the extension unpacked, and a Native Messaging Host / forcelist reference. |
| [`edge-version-downgrade-and-update-control.md`](./edge-version-downgrade-and-update-control.md) | How to pin Edge to a Blue Prism–compatible version and stop it auto-updating back. Includes the Blue Prism / Edge compatibility matrix, registry-policy / `gpedit.msc` (ADMX) / service-disabling methods, and a command reference. |

## Suggested reading order

1. Confirm version compatibility first with **`edge-version-downgrade-and-update-control.md`** — an incompatible Edge version breaks HTML automation no matter how perfectly everything else is configured.
2. Then work through **`blue-prism-edge-native-messaging-fix.md`** for the launch configuration and the extension/native-messaging setup.

> Validated primarily against **Blue Prism 7.4.x** and **Edge Chromium 148.x**. Re-verify
> against your exact product pairing — behavior changes across vendor releases.
