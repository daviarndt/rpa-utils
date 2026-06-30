# Contributing to rpa-utils

Thanks for considering a contribution. This repository is a shared, field-tested
knowledge base for Blue Prism (and general RPA) development — troubleshooting
guides, custom VBOs, .NET wrapper DLLs, and configuration runbooks. The goal is
that the next person who hits the same wall (including future-you) doesn't have to
start from zero.

These guidelines keep everything consistent, readable, and safe to publish.
Please read them before opening a pull request.

## Table of Contents

- [Golden Rules](#golden-rules)
- [Language](#language)
- [Repository Structure](#repository-structure)
- [Document Standards](#document-standards)
- [Code, VBOs & DLLs](#code-vbos--dlls)
- [Sanitizing Client & Environment Data](#sanitizing-client--environment-data)
- [File & Folder Naming](#file--folder-naming)
- [Commit Messages](#commit-messages)
- [Pull Requests & Issues](#pull-requests--issues)
- [Adding a Brand-New Topic](#adding-a-brand-new-topic)

## Golden Rules

1. **English only.** Every document, comment, commit message, and identifier is in English.
2. **Explain the *why*, not just the *what*.** A copy-paste command list is not enough — document the root cause and why the fix works.
3. **No client or confidential data.** Anonymize everything that ties content to a real organization, machine, domain, or URL.
4. **Ship the source, not just the binary.** A DLL or VBO without rebuildable source code is not accepted.
5. **Say when you're unsure.** Mark version-specific, environment-specific, or unverified findings clearly so the next reader can re-validate.

## Language

All content must be written in **English**, including:

- Markdown documentation
- Code comments and XML doc comments
- PowerShell / shell script messages (`Write-Host`, `echo`, etc.)
- Commit messages and pull request descriptions
- Variable, method, file, and folder names

If you are drafting from notes in another language, translate before opening the PR.

## Repository Structure

Content is grouped by topic, one folder per area:

```
rpa-utils/
├── browser-automation/      Browser automation troubleshooting (Edge, Chrome, ...)
│   └── edge/
├── dotnet-wrappers/         Custom .NET/VB.NET wrapper DLLs + their VBOs
│   └── pdfpig-reader/
├── vbos/                    General-purpose, reusable, client-agnostic VBOs
├── CONTRIBUTING.md
└── README.md
```

Rules:

- Each top-level topic folder, and each self-contained component folder, **must have its own `README.md`** that explains what it is and how to use it.
- Keep one topic per folder. If a guide grows large, split it and cross-link with relative Markdown links.
- Don't tie a folder to a specific client project. If it's only useful to one client, it doesn't belong here.

## Document Standards

A good guide in this repo follows this shape:

1. **Title + one-paragraph summary** — what this document solves, in plain terms.
2. **Table of Contents** — for anything longer than a couple of screens.
3. **Context / when you need this** — the symptom or scenario that brings someone here.
4. **Root cause** — the actual underlying reason, separated from the symptoms.
5. **The fix, step by step** — reproducible, with exact commands, registry paths, or flags.
6. **Why this works** — the reasoning, so the reader can adapt it to a slightly different situation.
7. **Verification** — how to confirm the fix actually took effect.
8. **Lessons learned / red herrings** *(optional but valued)* — what looked like the problem but wasn't.

Formatting conventions:

- Use fenced code blocks for commands, registry paths, and code. Specify the language where it helps (` ```cmd `, ` ```powershell `, ` ```vb `, ` ```csharp `).
- Use tables for flag-by-flag references, registry value lists, and compatibility matrices.
- Always note the **product versions** a finding was validated against. Behavior changes across vendor releases, so make this explicit and easy to spot. Put a **`Tested with`** line near the top of every guide (and fill the same field in the issue/PR templates), e.g.:

  ```
  > **Tested with:** Blue Prism 7.4.1 · Microsoft Edge 148.x · Windows Server
  ```
- Prefer relative links between docs so they keep working inside forks.

## Code, VBOs & DLLs

- **Include the source.** For a wrapper DLL, commit the `.cs`/`.vb` source and the build script(s) — not only the compiled `.dll`.
- **Make it rebuildable from scratch.** Provide enough setup notes (dependencies, versions, build commands) for someone else to reproduce the binary.
- **Pin dependency versions.** State the exact NuGet/package version a build was made against.
- **Avoid duplicating source.** If a build script needs the source code, have it read the canonical source file rather than embedding a second copy that can drift.
- **VBOs:** when you add an exported VBO (`.bpobject` / `.bprelease`), document its Actions (inputs, outputs, and the Code Stage logic) in an accompanying Markdown file, and note the Blue Prism version it was exported from.

## Sanitizing Client & Environment Data

This repo is public-facing. Before committing, **remove anything that identifies a real environment**:

| Replace this | With something like |
|---|---|
| Real company / domain names (`contoso.int`, `AMK_Browsersettings`) | `example.int`, `<OrgPrefix>_Browsersettings` |
| Real internal/site URLs | `https://your-target-site.example.com/` |
| Real server names, usernames, OU names | `<server-name>`, `<user>`, `<OU>` |
| Real absolute paths that leak structure | a neutral path like `C:\temp\RPA\...` |
| License keys, tokens, credentials, certificates | **never commit these — remove entirely** |

Extension IDs, public product version numbers, and vendor documentation URLs are fine to keep.

## File & Folder Naming

- Folders and Markdown files: **lowercase `kebab-case`** (e.g. `edge-version-downgrade-and-update-control.md`).
- No spaces, accents, or language-specific characters in file or folder names.
- Names should be descriptive and self-explanatory — `native-messaging-fix.md`, not `fix2.md`.
- Source files follow their language's convention (e.g. PascalCase for C# types/files like `BluePrismPdfHelper.cs`).

## Commit Messages

This repo uses [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <short summary in imperative mood>
```

Common types:

| Type | Use for |
|---|---|
| `feat` | A new VBO, DLL, tool, or guide |
| `docs` | Documentation additions or edits |
| `fix` | Correcting an error in a guide, script, or code |
| `refactor` | Reorganizing files/folders without changing meaning |
| `chore` | Tooling, `.gitignore`, repo housekeeping |

Scope is the topic area, e.g. `edge`, `pdfpig`, `vbos`, `readme`.

Examples:

```
docs(edge): add Blue Prism / Edge compatibility matrix
feat(pdfpig): add Search Text action to PDF helper VBO
refactor(pdfpig): split source, build scripts and docs into subfolders
```

Keep the summary under ~72 characters; add a body if context helps the next reader.

## Pull Requests & Issues

- **Issues** are welcome for corrections, version-specific behavior differences, or "this no longer works on version X" reports. A lot of this material is the result of trial and error against a specific product pairing, and vendor updates change behavior.
- **Pull requests** should be focused (one topic per PR), follow the document and naming standards above, and pass a quick self-review against the [Golden Rules](#golden-rules).
- If you're contradicting something already documented, say so explicitly and explain the environment difference — both versions may be correct for different setups.

## Adding a Brand-New Topic

1. Create a topic folder (or reuse an existing one) following [Repository Structure](#repository-structure).
2. Add a `README.md` to any new self-contained folder.
3. Write the guide following [Document Standards](#document-standards).
4. Add a row/entry to the **"What's in here right now"** section of the root [`README.md`](./README.md) so it's discoverable.
5. Open a PR with a Conventional Commit title.

Thanks for helping the RPA community trade a few hours of pain for a few minutes of reading.
