# Blue Prism: Send Keys vs. Global Send Keys (and Send Key Events)

Blue Prism gives you four different ways to inject keystrokes, and picking the wrong one is
the usual reason keystrokes "go nowhere", land in the wrong field, or fire without their
modifier (Ctrl/Alt/Shift). This guide explains how each method actually delivers keys, when
to use which, and how to fix the most common failures.

> **Applies to:** Blue Prism 6.x / 7.x · Windows 10/11 & Windows Server. This is a
> general-practice guide, not a single version-specific fix.

## Table of Contents

- [The four methods at a glance](#the-four-methods-at-a-glance)
- [How each one actually delivers keys](#how-each-one-actually-delivers-keys)
- [Which one should I use?](#which-one-should-i-use)
- [Key notation](#key-notation)
- [Common failure modes and fixes](#common-failure-modes-and-fixes)
- [Why Global Send Keys often works when Send Keys doesn't](#why-global-send-keys-often-works-when-send-keys-doesnt)
- [Verification](#verification)
- [Official references](#official-references)

## The four methods at a glance

| Method | Level | Targeted? | Needs window focused? | Typical use |
|---|---|---|---|---|
| **Send Keys** | Application (.NET `SendKeys`) | Yes (to the modelled app) | The element/window must have focus | Standard typing into a spied Win32/Java/browser app |
| **Send Key Events** | Application, key-by-key (down/up) | Yes | Yes | When you need to hold a modifier, or the app needs discrete key-down/key-up events |
| **Global Send Keys** | OS (Win32 input) | No — goes to whatever is in the foreground | Yes — target must be the foreground window | Apps that ignore standard Send Keys (many Java, Citrix, custom controls) |
| **Global Send Key Events** | OS, key-by-key (down/up) | No | Yes | OS-level equivalent of Send Key Events: hold modifiers, games, stubborn controls |

"Global" = sent at the operating-system input level to the **active foreground window**.
Non-global ("Send Keys" / "Send Key Events") = sent through the **modelled application
element**, so they can be aimed at a specific window even if it isn't perfectly in front.

## How each one actually delivers keys

- **Send Keys** uses the .NET `System.Windows.Forms.SendKeys` mechanism. Keys are posted to
  the focused control of the targeted application. It's convenient and fast, but it depends
  on the application accepting synthesized `WM_CHAR`/`WM_KEYDOWN` messages — some frameworks
  (older Java, some Citrix/virtualized or custom-drawn controls) simply ignore them.

- **Send Key Events** sends each key as an explicit **key-down** and **key-up** event rather
  than a single "type this string" call. That's what lets you *hold* a modifier down across
  several keys, or satisfy apps that react to discrete key events (e.g. a control that only
  responds on key-up).

- **Global Send Keys / Global Send Key Events** bypass the application layer and inject input
  at the OS level (Win32 input, comparable to `SendInput`). Whatever window is in the
  **foreground** receives them. This is why they succeed against apps that discard
  message-level Send Keys — the input arrives the same way a physical keyboard's would.

## Which one should I use?

1. **Start with Send Keys** for a normal, well-modelled application. It's the simplest and
   most targeted.
2. **If nothing arrives**, or only some keys register, switch to **Global Send Keys** — the
   app is likely ignoring message-level input. Make sure the target window is in the
   foreground first (see below).
3. **If a modifier combo misbehaves** (e.g. `Ctrl+Shift+Home` selects nothing), move to
   **Send Key Events** or **Global Send Key Events** so the modifier is genuinely held down
   for the duration of the combo.

## Key notation

**Send Keys / Send Key Events** use the standard .NET `SendKeys` notation:

| Token | Meaning |
|---|---|
| `^` | Ctrl |
| `%` | Alt |
| `+` | Shift |
| `{ENTER}` / `~` | Enter |
| `{TAB}` | Tab |
| `{ESC}` | Escape |
| `{BACKSPACE}` `{DELETE}` | Backspace / Delete |
| `{UP}` `{DOWN}` `{LEFT}` `{RIGHT}` | Arrow keys |
| `{F1}`…`{F12}` | Function keys |

Examples: `^c` = Ctrl+C, `%{F4}` = Alt+F4, `+{END}` = Shift+End, `^s` then `{ENTER}` = save
and confirm. Wrap a group with parentheses to apply a modifier to several keys: `^(ac)` =
Ctrl+A then Ctrl+C.

**Global Send Keys / Global Send Key Events** use Blue Prism's own key-name identifiers
(not the `^%+` shorthand). The concepts are the same — modifier keys, function keys, and
navigation keys — but the exact tokens are enumerated in the official reference. Because the
token names differ from the .NET notation, **do not assume a Send Keys string works verbatim
as a Global Send Keys string** — re-check it against the official key-code list linked below.

## Common failure modes and fixes

| Symptom | Likely cause | Fix |
|---|---|---|
| Nothing happens at all | Target window/element isn't focused (Send Keys) or isn't the foreground window (Global) | Activate the application first (an `Activate Application` / click on a known element), then send. For Global, guarantee the window is genuinely in front. |
| Keys land in the wrong field | Focus moved, or a dialog stole focus between stages | Set focus immediately before sending; don't rely on focus surviving a wait or a previous stage. |
| Modifier combo doesn't register | Single-call Send Keys releases the modifier too early for that app | Use **Send Key Events** / **Global Send Key Events** so the modifier is held for the whole combo. |
| Characters dropped or interleaved | Keys sent faster than the app can consume | Add a small interval between keys / split into smaller sends; slow, stubborn apps need pacing. |
| Wrong characters on some machines | Keyboard layout / locale differences | Prefer Global methods for layout-sensitive input, and test on a runtime resource with the production keyboard layout. |
| Works attended, fails unattended | No interactive desktop / locked session on the runtime resource | Run via Login Agent so there's an unlocked interactive session; verify screen isn't locked and resolution/DPI match. |

## Why Global Send Keys often works when Send Keys doesn't

Send Keys relies on the target application processing synthesized window **messages**. A
control that is custom-drawn, sandboxed, or virtualized (common with Java apps, Citrix/RDP
published apps, and some kiosk-style software) may never read those messages. Global Send
Keys injects input at the OS level, so the keystrokes reach the foreground window through the
same path as a real keyboard — which the application cannot distinguish from a human. The
trade-off is that Global input is **not targeted**: it goes to whatever is in front, so you
must make the correct window the foreground window first, and nothing else can grab focus
mid-send.

## Verification

1. Send into **Notepad** (or the target's own input field) first — if the string appears
   correctly there but not in your target app, the problem is the app ignoring message-level
   input, not your notation. That alone tells you to switch to Global.
2. For modifier combos, verify the *effect* (text selected, menu opened), not just that "no
   error was thrown" — Send Keys rarely errors even when the app ignored the input.
3. On an unattended runtime resource, confirm the session is logged in and unlocked (Login
   Agent) before blaming the notation.

## Official references

Cross-check the exact key-code tokens and per-action behavior against the official Blue Prism
documentation for your version:

- Blue Prism documentation portal: <https://bpdocs.blueprism.com/>
- Look for the guide **"Send Keys and Send Key Events"** and the **key-code reference** for
  Global Send Keys, under the automation/spying section for your product version.
