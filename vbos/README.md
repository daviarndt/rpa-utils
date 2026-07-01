# VBOs

General-purpose, reusable Blue Prism VBOs (Visual Business Objects) that are
**not tied to any specific client project** — utility actions meant to be dropped
into any Blue Prism environment.

Each VBO lives in its own folder containing:

- the exported VBO (`.bpobject` / `.bprelease`)
- a `README.md` documenting its Actions (inputs, outputs, Code Stage logic)
- the Blue Prism version it was exported from

## Available VBOs

| VBO | What it does |
|---|---|
| [`pdfpig-reader/`](./pdfpig-reader/) | PDF text extraction (page count, per-page text, keyword search, region/word-coordinate extraction) via the [PdfPig wrapper DLL](../dotnet-wrappers/pdfpig-reader/). Import the `.bprelease` and copy the DLLs into the Blue Prism folder. |

> This folder is populated incrementally. See the [Roadmap](../README.md#roadmap) in the root README.
