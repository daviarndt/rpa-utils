# Microsoft Edge: Version Downgrade & Automatic Update Control

A guide for pinning Microsoft Edge to a specific version and preventing automatic updates — primarily relevant when a Blue Prism (or other RPA tool's) browser extension is incompatible with the latest Edge release.

## Table of Contents

- [When You Need This](#when-you-need-this)
- [Blue Prism / Edge Compatibility Matrix](#blue-prism--edge-compatibility-matrix)
- [Step 1 — Identify the Compatible Version](#step-1--identify-the-compatible-version)
- [Step 2 — Uninstall the Current Edge Version](#step-2--uninstall-the-current-edge-version)
- [Step 3 — Download and Install the Target Version](#step-3--download-and-install-the-target-version)
- [Step 4 — Prevent Automatic Updates](#step-4--prevent-automatic-updates)
- [Verifying the Configuration](#verifying-the-configuration)
- [Useful Commands Reference](#useful-commands-reference)
- [Checking the Extension ID](#checking-the-extension-id)
- [Notes on Production/Server Environments](#notes-on-productionserver-environments)

## When You Need This

Browser extensions built on Chromium's extension APIs (including RPA vendor extensions like Blue Prism's) occasionally break across major Edge version jumps, either due to:
- Manifest version changes (e.g. Manifest V2 → V3 deprecations)
- Native Messaging API behavior changes
- Extension signing/validation format changes

If a previously-working RPA process suddenly fails after Edge auto-updates (commonly noticeable when the version jumps to a new major release, e.g. 149.x), and the vendor has not yet certified compatibility with that version, downgrading to the last known-compatible version is the standard mitigation while waiting for an official fix or update.

A typical symptom of an incompatible Edge version with Blue Prism: **UIA-based interaction still works, but HTML element interaction (Application Modeller Spy/Highlight/Identify) silently stops finding elements.** If you see that immediately after a major Edge version jump, version compatibility is the first thing to check.

## Blue Prism / Edge Compatibility Matrix

Blue Prism's Manifest V3 browser extension is only certified against specific Edge Chromium version ranges. The table below is a **point-in-time snapshot** — always confirm against the live vendor page before pinning a version.

| Blue Prism | Supported Edge (Manifest V3) |
|---|---|
| 7.5.x | 143.x – 148.x |
| 7.4.x | 111.x – 148.x (except 140.x) |
| 7.3.x | 111.x – 148.x (except 140.x) |
| 7.2.x | 111.x – 148.x (except 140.x) |

Source: <https://documentation.blueprism.com/en-us/browser-compatibility.htm> (verify for your exact product version — vendor support ranges shift over time).

In practice this means Edge **149.x and later** breaks HTML automation on Blue Prism 7.4.x/7.5.x, and `148.x` is the last broadly-compatible line at the time of writing.

## Step 1 — Identify the Compatible Version

Check your RPA vendor's official compatibility matrix or release notes for the exact Edge version range supported by your current product version. For Blue Prism specifically, check the official troubleshooting documentation for your version, e.g.:
```
https://bpdocs.blueprism.com/bp-<version>/<locale>/Guides/chrome-firefox/troubleshooting.htm
```

Make a note of the full version number you need (e.g. `148.0.xxxx.xx`), not just the major version — minor build differences can still matter.

## Step 2 — Uninstall the Current Edge Version

> **Caution:** Microsoft Edge is a core OS component on Windows 11 and cannot always be fully removed through normal uninstall flows. On many systems, "uninstalling" only removes the user-facing shortcuts while the underlying installation remains, and Edge Update will silently restore it. Plan for **Step 4 (preventing auto-update)** regardless of whether uninstall fully succeeds.

1. Close all Edge windows and background processes (see [Useful Commands Reference](#useful-commands-reference) for killing processes via command line)
2. Open **Settings → Apps → Installed apps**
3. Search for "Microsoft Edge"
4. Click the three-dot menu → **Uninstall**, if available

If the standard uninstall option is greyed out or unavailable (common on Windows 11 due to Edge being a protected system component), use the Edge uninstaller directly:

```cmd
"C:\Program Files (x86)\Microsoft\Edge\Application\<current_version>\Installer\setup.exe" --uninstall --force-uninstall --system-level
```

Replace `<current_version>` with the actual version folder name present on disk (check `C:\Program Files (x86)\Microsoft\Edge\Application\`).

## Step 3 — Download and Install the Target Version

1. Download the exact `.msi` or standalone installer for your target version from Microsoft's official Edge enterprise distribution channel (search "Microsoft Edge for Business" download archive, or your organization's internal software repository if one is maintained for compliance/version-pinning purposes)
2. Run the installer as Administrator
3. Verify the installed version:
   ```
   edge://version
   ```

## Step 4 — Prevent Automatic Updates

This is the step that matters most — without it, Edge Update will silently restore the latest version within hours or days, undoing the downgrade.

### Option A — Registry policy (recommended, most reliable)

```
Registry path: HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\EdgeUpdate
```

| Value Name | Type | Data | Purpose |
|---|---|---|---|
| `UpdateDefault` | `REG_DWORD` | `0` | Disables automatic updates for all Microsoft products covered by Edge Update, unless overridden per-product below. |
| `Update{56EB18F8-8008-4CBD-B6D2-8C97FE7E9062}` | `REG_DWORD` | `0` | Disables automatic updates specifically for Microsoft Edge (stable channel). The GUID `{56EB18F8-8008-4CBD-B6D2-8C97FE7E9062}` is Edge's stable-channel App ID — do not substitute a different GUID. |
| `TargetVersionPrefix{56EB18F8-8008-4CBD-B6D2-8C97FE7E9062}` | `REG_SZ` | `148.0` (example) | Pins Edge to a specific version prefix, so that even if updates are re-enabled later, Edge will only update within that version line, never past it. Use the major.minor version you need to stay on. |

### Option B — Disable the Edge Update scheduled tasks and services

As a secondary/defense-in-depth measure alongside Option A:

```cmd
sc config edgeupdate start= disabled
sc config edgeupdatem start= disabled
sc stop edgeupdate
sc stop edgeupdatem
```

Also disable the scheduled tasks (Task Scheduler → Microsoft → EdgeUpdate folder), or via command line:

```cmd
schtasks /Change /TN "MicrosoftEdgeUpdateTaskMachineCore" /Disable
schtasks /Change /TN "MicrosoftEdgeUpdateTaskMachineUA" /Disable
```

### Option C — Group Policy Editor (gpedit.msc) with Edge ADMX templates

The GUI equivalent of Option A. Useful when you'd rather manage the setting through the
Local Group Policy Editor than write the registry directly — it also makes the chosen
state visible to anyone auditing the machine's policy.

1. Download the **Microsoft Edge policy templates** (and the Edge Update templates) from
   the enterprise download page: <https://www.microsoft.com/en-us/edge/business/download>
2. Copy the template files into the local policy definition store:
   ```
   C:\Windows\PolicyDefinitions\MicrosoftEdge.admx
   C:\Windows\PolicyDefinitions\MicrosoftEdgeUpdate.admx
   C:\Windows\PolicyDefinitions\en-US\MicrosoftEdge.adml
   C:\Windows\PolicyDefinitions\en-US\MicrosoftEdgeUpdate.adml
   ```
   > If the OS is in another language and `gpedit.msc` throws a missing-resource error,
   > placing the `.adml` files under the `en-US` folder is enough — the editor will fall
   > back to the English strings.
3. Open the editor (`Win + R` → `gpedit.msc`) and navigate to:
   ```
   Computer Configuration
     → Administrative Templates
       → Microsoft Edge Update
         → Applications
           → Microsoft Edge
   ```
   (On a localized Windows install the path names are translated, but the tree structure is identical.)
4. Open **"Update policy override"**, set it to **Enabled**, and choose
   **"Updates disabled"** in the dropdown, then **OK**.
5. Optionally also disable the two Edge Update services via `services.msc`
   (**Microsoft Edge Update Service `edgeupdate`** and **`edgeupdatem`** → Startup type:
   **Disabled**) — this is the GUI equivalent of Option B.

### Apply the changes

```cmd
gpupdate /force
```

> If `gpupdate` asks to log off or restart to finish applying a setting, you can answer
> **N** — the update-control policies above take effect on the next policy refresh
> without a logoff.

## Verifying the Configuration

1. Open `edge://policy`
2. Click **Reload policies**
3. Confirm the EdgeUpdate-related policies appear with **Status: OK**
4. Open `edge://version` and confirm the version matches your intended pinned version
5. Wait a full update cycle (or manually trigger one) and confirm the version does **not** change:
   ```cmd
   "C:\Program Files (x86)\Microsoft\EdgeUpdate\MicrosoftEdgeUpdate.exe" /c
   ```
   (this manually triggers an update check — if your policy is correctly applied, it should report no eligible update or skip Edge entirely)

## Useful Commands Reference

A consolidated list of commands referenced throughout this guide and its companion document.

| Command | Purpose |
|---|---|
| `gpupdate /force` | Forces an immediate refresh of all Group Policy and local registry-based policies, without waiting for the normal refresh interval (commonly 90–120 minutes). Run this after any registry policy change. |
| `gpresult /h output.html` | Generates an HTML report of all currently applied Group Policy settings, separating **Local** policy from **Group Policy (domain)** sources. Essential for diagnosing conflicts on domain-joined machines — if a setting you configured locally is being overridden, it will show up here as coming from a domain GPO instead of "Local". |
| `taskkill /F /IM msedge.exe /T` | Force-kills all `msedge.exe` processes and their child processes. Useful as a manual equivalent of the Blue Prism pre-launch Code Stage. |
| `taskkill /F /IM msedgewebview2.exe /T` | Same as above, for the WebView2 runtime process that Edge and Edge-embedded apps spawn. |
| `tasklist \| findstr /i edge` | Lists all currently running processes with "edge" in the name — useful to confirm no zombie process survived a kill attempt. |
| `sc query edgeupdate` | Checks the current status of the Edge Update service. |
| `schtasks /query /tn "MicrosoftEdgeUpdateTaskMachineCore"` | Checks whether the Edge Update scheduled task is enabled or disabled. |

## Checking the Extension ID

Whenever you need to confirm an extension's exact ID (for registry forcelist entries, Native Messaging Host manifests, or troubleshooting documentation), always check it live rather than relying on a previously-documented value — the ID can differ depending on how the extension was loaded (see note in the companion document about unpacked vs. signed `.crx` installs).

1. Open Edge
2. Navigate to:
   ```
   edge://extensions
   ```
3. Enable **Developer mode** (toggle in the top-right corner)
4. Once enabled, every installed extension displays an **ID** field directly beneath its name and version — a 32-character lowercase string (e.g. `cakkjecedpllnfpjfihechphbakhgcme`)
5. While Developer mode is active, this is also where you confirm whether extensions can be loaded from external/local sources:
   - **Load unpacked** button becomes available — allows loading an extension directly from a folder on disk (no `.crx` packaging or signature required)
   - This is the same toggle required before testing manual installation of an unpacked extension, or before dragging a `.crx`/`.pem` pair onto the extensions page

> Developer mode only needs to be enabled for **manual verification or one-time installs**. Automated/production launches using `--load-extension="<path>"` do **not** require Developer mode to be active, since that flag loads the extension at the process level regardless of the UI toggle state.

## Notes on Production/Server Environments

- On domain-joined production servers, always run `gpresult /h output.html` **before** assuming a local registry fix will hold — a domain GPO managing Edge (commonly named something like `<OrgPrefix>_Browsersettings` or similar) may override local values on the next policy refresh cycle, silently undoing version pins or update-disable settings.
- If a domain GPO is found to conflict, the version pin and update-disable policies should be requested through whichever team manages domain GPOs, rather than maintained purely at the local registry level — local values can be overridden at any refresh interval without warning.
- Consider documenting the exact validated Edge version + Blue Prism version pairing in your internal deployment runbook, so future environment rebuilds don't have to rediscover compatibility constraints from scratch.
- Re-validate this entire setup whenever either Blue Prism or the target Edge version receives a major update — extension/Native Messaging compatibility is not guaranteed to remain stable across vendor releases on either side.
