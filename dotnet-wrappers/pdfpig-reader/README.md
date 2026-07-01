# PDF Reader for Blue Prism (PdfPig wrapper)

A small custom .NET wrapper DLL built on top of [PdfPig](https://github.com/UglyToad/PdfPig)
that gives Blue Prism reliable, dependency-light **text extraction from PDF files** —
including page counts, per-page text, keyword search, and word-level coordinates for
region-based extraction.

PdfPig is fully managed (no native dependencies, no Adobe/iText licensing), which makes it
a clean fit for locked-down RPA runtimes.

- **DLL version:** 1.0.0
- **Target framework:** .NET Framework 4.6.2 (`net462`)
- **PdfPig version:** 0.1.9 (pinned)
- **Validated with:** Blue Prism 7.4.1

## Folder layout

```
pdfpig-reader/
├── README.md                       (this file)
├── THIRD-PARTY-NOTICES.md          attribution for redistributed PdfPig binaries
├── src/
│   └── BluePrismPdfHelper.cs        canonical wrapper source (single source of truth)
├── build/
│   ├── 1_download_pdfpig.ps1        download + extract the PdfPig NuGet DLLs
│   ├── 2_copy_dlls.ps1              copy PdfPig DLLs into the Blue Prism folder
│   └── 3_compile_pdf_helper.ps1     compile src/BluePrismPdfHelper.cs into the DLL
├── dist/                           prebuilt DLLs, ready to drop into Blue Prism
│   ├── BluePrismPdfHelper.dll
│   └── UglyToad.PdfPig.*.dll        (6 PdfPig dependency DLLs)
└── docs/
    └── vbo-actions-reference.md     every Blue Prism Action, with inputs/outputs/Code Stage
```

The importable VBO release that wraps this DLL lives under
[`vbos/pdfpig-reader/`](../../vbos/pdfpig-reader/).

## What it does

| Method | Returns | Purpose |
|---|---|---|
| `Version` | Text | Which DLL build is deployed |
| `GetPageCount` | Number | Total pages in the PDF |
| `GetAllText` | Text | All text, page by page |
| `GetPageText` | Text | Text of a single page (1-based) |
| `GetAllPagesAsTable` | DataTable | One row per page (`PageNumber`, `PageText`) |
| `SearchText` | DataTable | Pages where a keyword appears, with context (case-insensitive) |
| `GetTextByRegion` | Text | Text inside a coordinate box on a page |
| `GetWordsWithCoordinates` | DataTable | Every word + its bounding-box coordinates |

Full Action-by-Action documentation (inputs, outputs, and the exact Code Stage for each)
is in [`docs/vbo-actions-reference.md`](./docs/vbo-actions-reference.md).

## Build & deploy

> **Just want to use it?** The prebuilt DLLs are in [`dist/`](./dist/). Copy them into
> `C:\Program Files\Blue Prism Limited\Blue Prism Automate\` and import the VBO from
> [`vbos/pdfpig-reader/`](../../vbos/pdfpig-reader/) — no build required. Attribution for the
> redistributed PdfPig binaries is in [`THIRD-PARTY-NOTICES.md`](./THIRD-PARTY-NOTICES.md).
> Build from source (below) only if you want to change the wrapper or verify the binaries.

The build was originally done **on the target Windows machine** using the bundled
PowerShell scripts (PowerShell's `Add-Type` compiles the DLL with the C# compiler that
ships with the .NET Framework — no Visual Studio required).

Run the three scripts **in order**, as Administrator (they write into
`C:\Program Files\...`):

```powershell
# 1. Download PdfPig 0.1.9 and extract its DLLs to C:\Temp\PdfPigSetup
.\build\1_download_pdfpig.ps1

# 2. Copy the net462 PdfPig DLLs into the Blue Prism Automate folder
.\build\2_copy_dlls.ps1

# 3. Compile src/BluePrismPdfHelper.cs -> BluePrismPdfHelper.dll (into the Blue Prism folder)
.\build\3_compile_pdf_helper.ps1
```

After step 3 you should have, in
`C:\Program Files\Blue Prism Limited\Blue Prism Automate\`:

- `BluePrismPdfHelper.dll` (this wrapper)
- the six `UglyToad.PdfPig.*.dll` files (its dependencies)

> **Single source of truth:** `3_compile_pdf_helper.ps1` reads and compiles
> `src/BluePrismPdfHelper.cs` directly. To change behavior, edit the `.cs` file and
> re-run step 3 — the code is never duplicated inside the script.

## Use it in Blue Prism

1. Create (or import) a VBO and, in **Edit > References**, add `BluePrismPdfHelper.dll`
   and the six `UglyToad.PdfPig.*.dll` files.
2. Add Code Stages calling the methods above (the namespace/class is
   `BluePrismPdfHelper.PdfHelper`).
3. See [`docs/vbo-actions-reference.md`](./docs/vbo-actions-reference.md) for the
   ready-to-paste Code Stage of every Action.

## Notes & limitations

- **Text-based PDFs only.** PdfPig reads the text layer; it does **not** OCR scanned/image
  PDFs. For scanned documents you need an OCR step (e.g. Tesseract) first.
- **Coordinates are in PDF points**, with the origin at the **bottom-left** of the page
  (PDF convention) — `Y` increases upward. Use `GetWordsWithCoordinates` to discover real
  values before calling `GetTextByRegion`.
- Word ordering follows PdfPig's reading order, which is usually but not always
  left-to-right, top-to-bottom depending on the document's internal structure.
