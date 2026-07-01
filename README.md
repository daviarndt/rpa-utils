# rpa-utils

[![License: MIT + CC BY 4.0](https://img.shields.io/badge/license-MIT%20%2B%20CC%20BY%204.0-blue.svg)](./LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Built for Blue Prism](https://img.shields.io/badge/built%20for-Blue%20Prism-2b3a8c.svg)](https://www.blueprism.com/)
[![YouTube: RPA Hour](https://img.shields.io/badge/YouTube-RPA%20Hour-FF0000.svg?logo=youtube&logoColor=white)](https://www.youtube.com/@rpahour)

A growing collection of practical utilities, troubleshooting guides, custom VBOs, .NET wrapper DLLs, and configuration runbooks for **Blue Prism** RPA development — built from real production issues, debugged step by step, and documented so the next person (including future-me) doesn't have to start from zero.

This is not official Blue Prism documentation. It's field notes: the stuff that actually happens when you try to run Blue Prism against a real corporate Edge environment, a flaky SOAP API, a PDF you need to parse with a custom .NET DLL, or a GPO that quietly overrides your registry fix. Some of it confirms what the official docs say. Some of it contradicts them, because production doesn't always behave like the docs.

Whether you're setting up your first Blue Prism dev environment or you're a few years in and stuck on a weird browser automation bug at 6pm on a Friday — there's a good chance something here saves you a few hours.

## Who this is for

- **Beginners** setting up a Blue Prism environment for the first time and looking for a sane starting checklist instead of trial and error
- **Experienced developers** who hit an obscure issue (browser extension not detected, native messaging host silently failing, Edge ignoring every command-line flag you pass it) and want the root cause, not just a workaround
- Anyone building **custom .NET/VB.NET components** for Blue Prism (wrapper DLLs, Code Stages, reusable VBOs) who wants real examples instead of starting from a blank Class Library

## Repository structure

```
rpa-utils/
├── browser-automation/
│   └── edge/
│       ├── README.md
│       ├── blue-prism-edge-native-messaging-fix.md
│       └── edge-version-downgrade-and-update-control.md
├── dotnet-wrappers/
│   └── pdfpig-reader/
│       ├── README.md
│       ├── src/                      (DLL source code)
│       ├── build/                    (PowerShell build scripts)
│       ├── dist/                     (prebuilt DLLs, ready to deploy)
│       └── docs/                     (VBO action reference)
├── vbos/
│   └── pdfpig-reader/                (importable .bprelease + README)
├── .github/                           (issue/PR templates, CODEOWNERS, CI workflows)
├── CONTRIBUTING.md
├── LICENSE                            (dual-license overview: MIT + CC BY 4.0)
└── README.md                          (this file)
```

> Structure will expand as more material is added. Each subfolder has its own `README.md` with setup instructions specific to that piece.

## What's in here right now

### Browser Automation — Microsoft Edge

| Document | What it covers |
|---|---|
| [`blue-prism-edge-native-messaging-fix.md`](./browser-automation/edge/blue-prism-edge-native-messaging-fix.md) | Root-cause fix for the classic *"The browser extension could not be detected — no active native messaging host found"* error. Spoiler: it's very often **not** a native messaging host problem — it's Edge's Background Mode keeping a zombie process alive that silently ignores every command-line flag you pass on launch. Includes the full validated launch command line (flag-by-flag explained), required registry policies, a Blue Prism pre-launch Code Stage to guarantee a clean process state, and how to load the extension unpacked when the signed `.crx` fails signature validation. |
| [`edge-version-downgrade-and-update-control.md`](./browser-automation/edge/edge-version-downgrade-and-update-control.md) | How to downgrade Edge to a Blue Prism–compatible version and **actually stop it from auto-updating back**. Includes the **Blue Prism / Edge compatibility matrix**, three ways to lock the version (registry policy, `gpedit.msc` with Edge ADMX templates, and disabling the Edge Update services/tasks), a CMD/PowerShell command reference (`gpupdate /force`, `gpresult /h`, `taskkill`, etc.) and how to check an extension's real ID via Developer Mode. |

### .NET Wrappers

| Component | What it covers |
|---|---|
| [`dotnet-wrappers/pdfpig-reader/`](./dotnet-wrappers/pdfpig-reader/) | Custom .NET wrapper DLL built on top of [PdfPig](https://github.com/UglyToad/PdfPig) for extracting text from PDF files inside Blue Prism — page counts, per-page text, keyword search, and word-level coordinates for region extraction. Includes the C# source ([`src/`](./dotnet-wrappers/pdfpig-reader/src/)), the PowerShell build scripts ([`build/`](./dotnet-wrappers/pdfpig-reader/build/)), **prebuilt DLLs** ([`dist/`](./dotnet-wrappers/pdfpig-reader/dist/)) for locked-down environments, and a full Action-by-Action reference ([`docs/vbo-actions-reference.md`](./dotnet-wrappers/pdfpig-reader/docs/vbo-actions-reference.md)). Built via PowerShell `Add-Type` — no Visual Studio required. |

### VBOs

General-purpose, reusable VBOs not tied to a specific client project — utility actions meant to be dropped into any Blue Prism environment.

| VBO | What it covers |
|---|---|
| [`vbos/pdfpig-reader/`](./vbos/pdfpig-reader/) | Importable Blue Prism release (`.bprelease`) for the PDF text-extraction VBO backed by the [PdfPig wrapper](./dotnet-wrappers/pdfpig-reader/). Import it, copy the DLLs into the Blue Prism folder, and the PDF Actions are ready to use. Exported from Blue Prism 7.4.1. |

## Quick start

If you just want the fastest path to a working Blue Prism + Edge browser automation setup:

1. Read [`blue-prism-edge-native-messaging-fix.md`](./browser-automation/edge/blue-prism-edge-native-messaging-fix.md) in full — particularly the [Setup Checklist](./browser-automation/edge/blue-prism-edge-native-messaging-fix.md#setup-checklist-for-a-new-environment) section
2. If your Edge version is incompatible with your Blue Prism version, follow [`edge-version-downgrade-and-update-control.md`](./browser-automation/edge/edge-version-downgrade-and-update-control.md) first, then come back to step 1
3. Apply the registry policies, the launch command line, and the pre-launch Code Stage exactly as documented
4. Verify via `edge://version` and `edge://policy` before assuming anything is broken at the Blue Prism level

## Contributing / extending this repo

This repo grows as real issues get solved, and contributions are welcome. Full guidelines
live in [`CONTRIBUTING.md`](./CONTRIBUTING.md) — the essentials:

- **English only**, for docs, code comments, and commit messages.
- **Explain the *why*, not just the *what*** — root cause, fix, and a "why this works" section, not a copy-paste command list.
- **Ship the source, not just the binary** — for a VBO or DLL, commit the source code and build script, plus enough notes to rebuild from scratch.
- **No client or confidential data** — anonymize company names, internal URLs, servers, and credentials before committing (see the sanitizing checklist in `CONTRIBUTING.md`).
- **Commits follow [Conventional Commits](https://www.conventionalcommits.org/)** (e.g. `docs(edge): add compatibility matrix`).
- If something here turns out to be wrong, outdated, or version-specific, open an issue/PR with the correction — a lot of this is the result of trial and error against a specific product version pairing, and vendor updates can change behavior.

## License

This repository is **dual-licensed** to fit its mixed content:

- **Code** (C#, PowerShell, VBO source/exports, CI config) — [MIT License](./LICENSE-CODE)
- **Documentation** (the Markdown guides and written content) — [CC BY 4.0](./LICENSE-DOCS)

You can use, adapt, and redistribute everything here, including commercially — keep the MIT
notice for code, and give attribution (a link back here) for documentation. See
[`LICENSE`](./LICENSE) for the details.

## Disclaimer

This is independent, community-style documentation based on real troubleshooting sessions. It is **not affiliated with or endorsed by Blue Prism Limited or Microsoft**. Always cross-check against official vendor documentation for your specific product version, and test any registry/policy change in a non-production environment first.

## Roadmap

- [ ] Add general-purpose VBOs (string utilities, file system helpers, retry/error-handling patterns)
- [ ] Add more .NET wrapper DLL examples (OCR via Tesseract, OAuth-capable HTTP client)
- [ ] Add Blue Prism + Chrome equivalent of the Edge native messaging guide, if/when needed
- [ ] Add a general "Blue Prism dev environment setup from scratch" checklist (independent of browser automation)
- [ ] Add versioning/Git workflow notes for managing `.cs` source files and `.ps1` build scripts alongside Blue Prism release objects

## About the Author

Hi, I'm **Davi Arndt** — an RPA developer working primarily with **Blue Prism** in
corporate Windows environments. I build this repository from real production
troubleshooting, so the next person (often future-me) doesn't have to rediscover the
same fixes from scratch.

I also create RPA content and training:

- 🎥 **YouTube — [RPA Hour](https://www.youtube.com/@rpahour)**: a complete, full Blue Prism
  course plus standalone videos on RPA development and troubleshooting.
- 🎓 **Udemy — [Blue Prism AD01 Certification Prep](https://www.udemy.com/course/preparacao-certificacao-ad01-blue-prism/?referralCode=6DB62EB15EEC270138D0)**:
  a course built to prepare you for Blue Prism's **AD01** certification exam.

If this repo saved you a few hours, the channel and course are good next stops — and
contributions back here are always welcome (see [`CONTRIBUTING.md`](./CONTRIBUTING.md)).

---

*Maintained as a personal/professional knowledge base by Davi Arndt, working primarily with Blue Prism in corporate Windows environments. Updated whenever something new gets debugged and is worth keeping.*
