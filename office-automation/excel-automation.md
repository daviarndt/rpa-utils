# Blue Prism: Reliable Excel Automation with the MS Excel VBO

Blue Prism ships the **MS Excel VBO**, which drives Excel through COM automation. It works
well attended, then causes two recurring headaches in production: **orphaned `EXCEL.EXE`
processes** piling up on runtime resources, and Excel **failing to start at all under an
unattended service account**. This guide covers the object model, those two root causes, the
data-throughput choices, and how to verify a clean run.

> **Applies to:** Blue Prism 6.x / 7.x · Microsoft Excel 2016 / 2019 / 365 (desktop) ·
> Windows 10/11 & Windows Server. General-practice guide, not a single version-specific fix.

## Table of Contents

- [The MS Excel VBO model](#the-ms-excel-vbo-model)
- [Core actions](#core-actions)
- [Root cause 1: orphaned EXCEL.EXE processes](#root-cause-1-orphaned-excelexe-processes)
- [Root cause 2: Excel fails under an unattended service account](#root-cause-2-excel-fails-under-an-unattended-service-account)
- [Reading and writing data efficiently](#reading-and-writing-data-efficiently)
- [Regional and format pitfalls](#regional-and-format-pitfalls)
- [When you don't want Excel installed at all](#when-you-dont-want-excel-installed-at-all)
- [Verification](#verification)
- [Official references](#official-references)

## The MS Excel VBO model

The VBO is stateful and handle-based. A typical flow:

1. **Create Instance** → returns a `handle` identifying one Excel Application instance.
2. **Open Workbook** (or **Open**) with that handle and a file path → opens the workbook.
3. Do the work — read/write cells, ranges, worksheets.
4. **Save** / **Save As**.
5. **Close** the workbook, then **Close Instance** to terminate that Excel instance.

The single most important habit: **every `Create Instance` must be paired with a
`Close Instance`**, even on the error path. Skipping it is what leaves Excel running
invisibly.

## Core actions

| Action | Purpose |
|---|---|
| `Create Instance` | Start an Excel Application instance; returns a handle |
| `Open Workbook` / `Open` | Open a workbook file on that instance |
| `Show` | Make the instance visible (default is hidden) — useful for debugging only |
| `Get Worksheet As Collection` | Read a sheet into a Blue Prism Collection |
| `Write Collection` | Write a Collection into a sheet |
| `Get/Set Cell Value` | Single-cell read/write |
| `Get/Set Current Worksheet` | Select the active sheet |
| `Save` / `Save As` | Persist changes |
| `Close Workbook` | Close the workbook (optionally saving) |
| `Close Instance` | Terminate the Excel instance (releases the process) |

## Root cause 1: orphaned EXCEL.EXE processes

**Symptom:** after a few runs (especially failed ones), Task Manager shows several
background `EXCEL.EXE` processes. Eventually new instances hang, files stay locked, or the
resource runs out of memory.

**Cause:** an Excel COM instance is only released when it is explicitly closed *and* all COM
references to it are gone. If a process errors after `Create Instance` but before
`Close Instance` — or if a modal Excel dialog blocks a clean shutdown — the instance is
abandoned but the `EXCEL.EXE` process keeps running.

**Fix:**

1. **Always close on the error path.** Put `Close Instance` in a cleanup block that runs even
   when the main logic throws (Recover/Resume around the work, closing in the Recover path).
2. **Suppress dialogs that block shutdown.** Save explicitly so Excel doesn't pop a
   "Save changes?" prompt on close; avoid leaving unsaved state.
3. **Last-resort safety net:** as a *supervised* cleanup between runs, kill stray processes
   (`taskkill /F /IM EXCEL.EXE`). Use this deliberately — it will also kill any legitimately
   open Excel on that machine, so it belongs on a dedicated unattended runtime resource, not
   a shared desktop.

> The pattern mirrors the browser-automation lesson in this repo: guarantee a clean process
> state instead of hoping the previous run tidied up after itself.

## Root cause 2: Excel fails under an unattended service account

**Symptom:** the process works when you're logged in and test it, but on an unattended
runtime resource (or when Blue Prism runs under a service/system account) `Create Instance`
or `Open Workbook` throws a COM error such as *"Microsoft Excel cannot open or save any more
documents"* or a generic `0x800A03EC` / COM activation failure.

**Cause:** Office was designed to run interactively. When automated from a non-interactive
Windows session (Session 0 / a service account), Excel looks for a per-user **Desktop**
folder that doesn't exist for that account and fails to initialize.

**Fix — create the missing Desktop folders** for the system profile:

```
C:\Windows\System32\config\systemprofile\Desktop
C:\Windows\SysWOW64\config\systemprofile\Desktop
```

(Create both — 64-bit and 32-bit Office look in different ones.) Additional hardening for
unattended Office COM:

- Ensure Excel is **installed and activated** on the runtime resource (COM automation needs a
  real, licensed Excel — not just the viewer).
- Complete Excel's **first-run** prompts once interactively (license acceptance, "Get
  started" dialog) so they don't block the automated session.
- Review **DCOM** settings for the Microsoft Excel Application identity if activation is
  denied under the service account.
- Prefer running via **Login Agent** so there is a real interactive session, which sidesteps
  most Session 0 COM problems entirely.

## Reading and writing data efficiently

- **Bulk over cell-by-cell.** `Get Worksheet As Collection` / `Write Collection` move a whole
  sheet in one COM round-trip. Reading or writing cell-by-cell with `Get/Set Cell Value` in a
  loop is dramatically slower because every cell is a separate COM call.
- **Large datasets:** for read-heavy work on big files, an **OLEDB** query against the
  workbook (treating it like a database) is often faster and doesn't require Excel to be
  running at all. It has its own quirks (mixed-type columns, header detection), so test
  against representative data.
- **Turn off the expensive UI features** while writing a lot (screen updating, automatic
  calculation) and restore them afterward, if you're scripting Excel directly via Code Stage.

## Regional and format pitfalls

- **Dates and numbers** read back according to the runtime resource's locale. A date written
  as `03/04/2026` can round-trip as March 4 or April 3 depending on regional settings — pin
  the runtime resource's locale, or read/write dates as ISO text and convert explicitly.
- **Decimal separators** (`.` vs `,`) differ by locale and silently corrupt numeric parsing.
- **Leading zeros / long numbers** (IDs, phone numbers) can be mangled by Excel's automatic
  type coercion — format the cells as text or prefix appropriately.

## When you don't want Excel installed at all

If the runtime resource can't have Excel (licensing, hardening, or you just want to avoid COM
entirely), read/write `.xlsx` with a managed library instead — e.g. a small .NET wrapper DLL
over OpenXML/EPPlus exposed to Blue Prism as a VBO. That's the same pattern as the
[PdfPig reader](../dotnet-wrappers/pdfpig-reader/) in this repo: a dependency-light,
COM-free component that works on locked-down machines. (A candidate for a future wrapper
here.)

## Verification

1. Run the process end-to-end, then check Task Manager / `tasklist | findstr /i excel` —
   there should be **zero** leftover `EXCEL.EXE` after `Close Instance`.
2. Force an error mid-process (e.g. point at a missing file) and confirm the cleanup path
   still closes the instance — no orphan left behind.
3. Test on the **actual unattended runtime resource under its service account**, not just
   your interactive session — Root cause 2 only shows up there.

## Official references

Cross-check action names and parameters against the official Blue Prism documentation for
your version (the shipped MS Excel VBO occasionally gains or renames actions between
releases):

- Blue Prism documentation portal: <https://bpdocs.blueprism.com/>
- Look for the **"Excel Automation"** guide and the **MS Excel VBO** reference for your
  product version.
