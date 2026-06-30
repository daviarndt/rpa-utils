# Blue Prism + Microsoft Edge: Native Messaging Host & Browser Extension Setup Guide

A complete, field-tested guide for getting Blue Prism's Edge browser extension and Native Messaging Host working reliably — including the root cause of the most common failure mode (`Die Browsererweiterung konnte nicht erkannt werden` / "Could not find an active native messaging host associated with the selected browser") and a stable, repeatable launch configuration for isolated/disposable browser sessions.

## Table of Contents

- [Background: Why This Document Exists](#background-why-this-document-exists)
- [The Core Problem](#the-core-problem)
- [Root Cause](#root-cause)
- [The Fix — Step by Step](#the-fix--step-by-step)
- [Final Command Line Configuration](#final-command-line-configuration)
- [Registry Configuration](#registry-configuration)
- [Verifying Everything Is Applied Correctly](#verifying-everything-is-applied-correctly)
- [Blue Prism Pre-Launch Code Stage (Process Cleanup)](#blue-prism-pre-launch-code-stage-process-cleanup)
- [Loading the Extension from Source (Unpacked)](#loading-the-extension-from-source-unpacked)
- [Native Messaging Host & Extension Forcelist — Reference](#native-messaging-host--extension-forcelist--reference)
- [Setup Checklist for a New Environment](#setup-checklist-for-a-new-environment)
- [Lessons Learned](#lessons-learned)

---

## Background: Why This Document Exists

Blue Prism's browser automation relies on a **Native Messaging Host** — a small executable (`BluePrism.MessagingHost.exe`, shipped with Blue Prism Automate) that bridges communication between the browser extension and the Blue Prism process. When this bridge fails to establish, the extension may appear to be installed and even visible in `edge://extensions`, but every Application Modeller action against HTML elements (Spy, Highlight, Identify) silently fails or throws an exception.

This is especially common in RPA environments where the browser is launched with:
- A custom `--user-data-dir` for session isolation
- A pre-launch step that deletes/recreates the profile folder for a "clean session"
- A long list of command-line flags to suppress popups, autofill, password prompts, etc.

The failure is **not caused by a missing or broken extension or Native Messaging Host registration**. It is caused by something far less obvious: **a zombie Edge background process silently swallowing all command-line arguments on launch.**

## The Core Problem

Typical symptom sequence:

1. Browser opens normally, page loads, automation appears to start
2. Application Modeller fails to Spy/Highlight/Identify any HTML element
3. Blue Prism throws:
   ```
   Internal: Error performing Step 1 in Navigate Stage 'Launch' on Page 'Launch' -
   The browser extension could not be detected. No active native messaging host
   associated with the selected browser could be found.
   ```
4. The extension is visibly installed in `edge://extensions`
5. Re-installing the extension, recreating the registry forcelist entry, or even a clean reinstall of Blue Prism on a separate machine does **not** fix it

If you're seeing this pattern, skip straight to [Root Cause](#root-cause) — you likely do **not** have a Native Messaging Host registration problem.

## Root Cause

**Microsoft Edge's Background Mode keeps a zombie `msedge.exe` process alive even after all visible windows are closed.**

When Blue Prism (or any automation) launches Edge with a full command line of flags (`--user-data-dir`, `--load-extension`, `--ignore-certificate-errors`, etc.), Windows checks whether an Edge process is already running. If a background/zombie process exists:

- Windows does **not** start a fresh process with your arguments
- Instead, it asks the *existing* process to simply open a new window
- The existing process was already initialized earlier (typically silently, by Edge's own background service) with **no relevant arguments** — visible in `edge://version` as:
  ```
  Command line: "...\msedge.exe" --no-startup-window --flag-switches-begin --flag-switches-end
  ```
- **Every single flag you passed is silently discarded.** Not partially — completely.

This explains every downstream symptom that looks like an extension/native-messaging problem:
- `--load-extension` is ignored → extension never actually loads in that window
- `--user-data-dir` / `--profile-directory` is ignored → wrong profile is used
- `--ignore-certificate-errors` is ignored → SSL warnings reappear
- `--restore-last-session=false` is ignored → old tabs get restored after a kill

You can prove this yourself in two steps:
1. Open `edge://version` after a launch
2. Check the **Command line** field — if your flags aren't listed there verbatim, none of them were applied, regardless of what you put in the launch configuration

## The Fix — Step by Step

### 1. Disable Edge Background Mode globally (registry policy)

This prevents Edge from keeping a process alive in the background after all windows are closed, which is the actual root cause.

```
Registry path: HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge
Value name:    BackgroundModeEnabled
Type:          REG_DWORD
Data:          0
```

### 2. Kill all Edge-related processes before every Launch (Blue Prism Code Stage)

Even with Background Mode disabled, add a "Kill Process" action for processes "msedge" and "msedgewebview2" as a safety net immediately before the `Launch` stage in your Application Modeller process. This guarantees that whatever happens, the next Edge process Blue Prism starts is a genuinely fresh one that will read and apply the full command line.

### 3. Verify in `edge://version`

After launch, open a new tab and navigate to:
```
edge://version
```
Check the **Command line** field. All flags you configured should appear verbatim. If `--no-startup-window` appears instead of your flags, Background Mode (or another zombie process source) is still active — repeat steps 1–2.

## Final Command Line Configuration

This is the validated, working command line for a fully isolated, disposable Edge session with the Blue Prism extension force-loaded:

```
--start-maximized
--ignore-certificate-errors
--load-extension="{here you should place the extension path}"
--user-data-dir="{here you should place the new Edge profile path}"
--no-first-run
--no-default-browser-check
--disable-notifications
--disable-infobars
--disable-session-crashed-bubble
--disable-features=msEdgeAutofill,TranslateUI,EdgeFollow,AutofillServerCommunication
--restore-last-session=false
--disable-popup-blocking
--disable-save-password-bubble
"https://your-target-site.example.com/"
```

On flag --user-data-dir, make sure that you use the action "Delete Directory" to delete this folder before every launch of Edge.

### Flag-by-flag reference

| Flag | Purpose |
|---|---|
| `--start-maximized` | Opens the browser window maximized, avoiding coordinate/element visibility issues caused by small or inconsistent window sizes. |
| `--ignore-certificate-errors` | Suppresses SSL certificate warnings (`NET::ERR_CERT_AUTHORITY_INVALID` and similar) for sites with self-signed, expired, or internally-issued certificates. **Security note:** this disables certificate validation for *all* sites in this browser instance — only use on dedicated automation profiles, never on a shared/personal profile. |
| `--load-extension="<path>"` | Forces Edge to load an **unpacked** extension directly from a folder on disk at startup, bypassing the need for store installation, extension signing, or persisted profile data. Critical for disposable profiles, since extensions installed "normally" live inside profile data and disappear if the profile is wiped. |
| `--user-data-dir="<path>"` | Sets a custom root folder for browser profile data (cookies, cache, extensions, preferences). Used here to fully isolate the automation session from any personal/default Edge profile and to allow the entire folder to be deleted before each run for a guaranteed clean session. |
| `--no-first-run` | Skips Edge's first-run setup wizard/welcome screens. |
| `--no-default-browser-check` | Suppresses the "set Edge as your default browser" prompt. |
| `--disable-notifications` | Blocks website push notification permission prompts. |
| `--disable-infobars` | Suppresses the info bar notifications Chromium sometimes shows below the address bar (e.g. "Edge is being controlled by automated software"). |
| `--disable-session-crashed-bubble` | Suppresses the "Edge didn't shut down correctly" restore-session bubble that appears after a non-graceful exit (e.g. a process kill). |
| `--disable-features=msEdgeAutofill,TranslateUI,EdgeFollow,AutofillServerCommunication` | Disables a list of Edge-specific UI features that can interfere with automation: autofill suggestions, the translate popup, the "Follow this site" feature, and autofill's server-side communication. |
| `--restore-last-session=false` | Explicitly tells Edge not to restore tabs from the previous session on launch. Works alongside `--disable-session-crashed-bubble`, but is **not sufficient on its own** if Background Mode/zombie processes are involved — see [Root Cause](#root-cause). |
| `--disable-popup-blocking` | Disables the popup blocker, useful when the automated site relies on `window.open()` or similar for legitimate functionality (e.g. downloads, secondary windows). |
| `--disable-save-password-bubble` | Suppresses the "Save password?" prompt after form submissions. |

## Registry Configuration

All values below live under:
```
HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge
```

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `BackgroundModeEnabled` | `REG_DWORD` | `0` | **Critical fix.** Prevents Edge from keeping a process alive in the background after the last window closes — this is the actual root cause of flags being silently ignored on launch. |
| `DownloadDirectory` | `REG_SZ` | `C:\temp\RPA\Downloads` | Sets a fixed, predictable download folder for the automation profile, instead of relying on the user's default Downloads folder. |
| `DownloadRestrictions` | `REG_DWORD` | `0` | Disables additional download restriction policies that can block certain file types from completing a download. `0` = no special restrictions. |
| `PromptForDownloadLocation` | `REG_DWORD` | `0` | Disables the "Where do you want to save this file?" picker, so downloads save automatically to `DownloadDirectory` above without user interaction. |
| `SmartScreenPuaEnabled` | `REG_DWORD` | `0` | Disables SmartScreen's "potentially unwanted application" / suspicious file warning (e.g. "this file can harm your device") for downloaded files. |

> **Note:** A separate `SmartScreenEnabled = 0` value can be added if SmartScreen's general reputation-based warnings still interfere with downloads (it was tested during this investigation but did not end up being strictly required once the above values were correctly applied — verify on your own environment).

After creating or changing any of these values:

```cmd
gpupdate /force
```

Then confirm they were applied without conflicts by opening:
```
edge://policy
```
and clicking **"Reload policies"**. Each value should show **Status: OK**. If a value shows **Warning/Conflict**, another policy source (typically a domain GPO — see `gpresult /h output.html` to check) is competing with your local registry value, and the local value may be overridden on the next policy refresh.

## Verifying Everything Is Applied Correctly

A quick end-to-end sanity check after setting everything up:

1. Launch Edge through Blue Prism (or manually with the exact same command line)
2. Go to `edge://version` → confirm **Command line** shows every flag you configured
3. Go to `edge://extensions` → confirm the Blue Prism extension is listed and enabled
4. Go to `edge://policy` → confirm all registry values above show **Status: OK**
5. In Blue Prism's Application Modeller, attempt Spy/Highlight/Identify on any HTML element of the target page

If step 2 fails (flags missing from Command line), nothing downstream will work correctly — go back to [The Fix](#the-fix--step-by-step) and confirm Background Mode is disabled and no zombie process survived from a previous run.

## Blue Prism Pre-Launch Code Stage (Process Cleanup)

Place the action "Kill Process" for processes "msedge" and "msedgewebview2" positioned immediately before the **Launch** Navigate stage in your Application Modeller process. This is the safety net that guarantees a clean process state regardless of registry policy refresh timing, GPO overrides, or leftover sessions from manual testing.

Optionally, delete the disposable profile folder right before launch for a fully clean session (only safe to do because the extension is force-loaded via `--load-extension` and does not depend on profile-persisted data).

## Loading the Extension from Source (Unpacked)

If the official `.crx`/`.pem` package fails to install via drag-and-drop with a signature validation error (common across Edge version upgrades that change the Chromium extension signing format), use the **unpacked** extension folder instead — this completely bypasses signature verification.

### Steps

1. Obtain the unpacked extension source folder. It should contain, at minimum:
   ```
   manifest.json
   bluePrismPlugin.js
   bluePrismServiceWorker.js
   getHtmlSource.js
   optionsHandler.js
   _metadata/
   crypto-js/
   extensionOptions/
   icons/
   ```
2. Place this folder somewhere **stable and outside of any profile folder that gets deleted**, e.g.:
   ```
   C:\BluePrism\BPBrowserExtension_7.4.1.15752_0
   ```
3. To verify manually (one-time check, not required for automated runs):
   - Open `edge://extensions`
   - Enable **Developer mode** (top-right toggle)
   - Click **Load unpacked**
   - Select the extension folder (the one directly containing `manifest.json`)
   - Confirm the extension loads without any signature error
4. For automated runs, reference the same folder via the `--load-extension="<path>"` command-line flag (see [Final Command Line Configuration](#final-command-line-configuration)) — this loads it automatically on every launch without requiring Developer mode to be manually enabled each time.

> **Important — Extension ID differs by load method:** an extension loaded via `--load-extension` (unpacked, no `.pem` key) generates a **different** extension ID than the one generated when installed from the signed `.crx`/`.pem` package. If you ever need to reference the extension ID (e.g. in a Native Messaging Host manifest's `allowed_origins`, or in an `ExtensionInstallForcelist` registry entry), always re-check the *currently active* ID in `edge://extensions` with Developer mode enabled — don't assume it matches an ID seen in documentation or a previous install method.

## Native Messaging Host & Extension Forcelist — Reference

This section collects field observations about how Blue Prism's extension and Native
Messaging Host are wired up. It's reference material for when the [Background Mode fix](#the-fix--step-by-step)
alone isn't enough and you need to reason about the extension/host plumbing directly.

### The official extension ID and the forcelist

Blue Prism's own Automate installer writes an `ExtensionInstallForcelist` policy that
force-installs the extension by ID:

```
HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge\ExtensionInstallForcelist
```

- The value is a **numbered** entry (`1`, `2`, ...), **not** the `(Default)` value — a
  malformed entry landing on `(Default)` will not install anything.
- The official Blue Prism extension ID is **`cakkjecedpllnfpjfihechphbakhgcme`** — this is
  the value the installer itself writes, which confirms it's the canonical ID.
- In locked-down environments **without access to the Microsoft Edge Add-ons Store**
  (network/policy blocked), the bare ID (no store update URL appended) is the form that
  works. Appending the store URL fails because the store is unreachable.

> **Unpacked load produces a different ID.** As noted in
> [Loading the Extension from Source](#loading-the-extension-from-source-unpacked), an
> extension loaded via `--load-extension` (no `.pem`) gets a *different* ID than the
> signed-package ID above. Always re-check the live ID in `edge://extensions` (Developer
> mode) before reusing it in a forcelist or Native Messaging manifest.

### The Native Messaging Host executable

The host that bridges the extension and the Blue Prism process ships with Automate:

```
C:\Program Files\Blue Prism Limited\Blue Prism Automate\BluePrism.MessagingHost.exe
```

It sits alongside `BluePrism.NativeMessaging.dll` and `BluePrism.BrowserAutomation.dll`.

### Things to verify when the host doesn't connect

If the extension shows up in `edge://extensions` but Application Modeller still can't
interact with HTML elements, check the Native Messaging registration:

```
HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Edge\NativeMessagingHosts
```

- This key holds one subkey per registered host, each pointing to a `.json` manifest whose
  `name` field must match what the extension calls via `chrome.runtime.connectNative(...)`.
- On environments where browser automation was misbehaving, this key sometimes contained
  **only unrelated hosts** (e.g. Microsoft Defender's) and no Blue Prism entry — a sign the
  host was not registered for that browser/user context.

> **Practical note:** in the cases documented here, fixing
> [Edge Background Mode](#root-cause) and forcing a clean process on every launch was what
> actually restored end-to-end communication. Treat the Native Messaging registry checks
> above as a secondary diagnostic — confirm the command line is being honored (`edge://version`)
> **before** going down the host-registration rabbit hole, because a zombie process makes
> every host check look broken regardless of how it's registered. If you have genuinely
> ruled out Background Mode and the host still won't connect, that's the point to open an
> official Blue Prism support case, since the public troubleshooting docs don't cover manual
> host registration.

## Setup Checklist for a New Environment

Use this as a quick-reference checklist when configuring a new development machine or production server.

- [ ] Confirm Edge version compatibility with your Blue Prism version (see the companion document: `edge-version-downgrade-and-update-control.md`)
- [ ] Apply registry values under `HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Edge`:
  - [ ] `BackgroundModeEnabled = 0`
  - [ ] `DownloadDirectory = <fixed path>`
  - [ ] `DownloadRestrictions = 0`
  - [ ] `PromptForDownloadLocation = 0`
  - [ ] `SmartScreenPuaEnabled = 0`
- [ ] Run `gpupdate /force`
- [ ] Confirm all values show **Status: OK** in `edge://policy` (no Warning/Conflict)
- [ ] Place the unpacked Blue Prism extension folder in a stable location outside any disposable profile path
- [ ] Configure the Edge launch command line in Blue Prism with all flags from [Final Command Line Configuration](#final-command-line-configuration)
- [ ] Add the process-kill Code Stage immediately before the Launch stage in Application Modeller
- [ ] Launch once and verify `edge://version` → Command line matches exactly
- [ ] Verify `edge://extensions` shows the extension as loaded and enabled
- [ ] Test Spy / Highlight / Identify on a known HTML element in Application Modeller
- [ ] If running on a domain-joined machine, run `gpresult /h gpresult.html` once and check for any conflicting domain GPO under "Microsoft Edge" policies before assuming a local fix is final — domain policy can silently override local registry values on the next refresh cycle

## Lessons Learned

A short summary of what *looked* like the problem during troubleshooting, but wasn't:

| Looked like the problem | Actually was |
|---|---|
| Extension not installed / missing from disposable profile | True symptom, but not the root cause — the extension was never the blocker once process handling was fixed |
| Native Messaging Host not registered in `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Edge\NativeMessagingHosts` | A real observation, but unrelated to this specific failure mode — extension communication failed before native messaging was ever relevant, because the extension itself wasn't even loading into the active window |
| `ExtensionInstallForcelist` misconfiguration | A genuine side issue (worth fixing for unattended store-based installs), but not the cause of the original Launch failure |
| `--user-data-dir` pointing outside the default Edge data folder | Not actually incompatible with Native Messaging — this was a red herring caused by the zombie process issue happening to coincide with profile path changes |
| SSL certificate / SmartScreen / download prompts | All real and worth fixing, but cosmetic relative to the core issue — none of them block Application Modeller's element interaction |

**The one fix that mattered most:** disabling Edge Background Mode and killing all Edge processes before every launch. Once that was in place, every other previously "fixed" workaround (custom profile directory, unpacked extension loading, certificate bypass, download policies) worked exactly as expected, because the command line was finally being read by a genuinely fresh process.
