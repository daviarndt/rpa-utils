# PDFPig Reader — VBO

A ready-to-import Blue Prism VBO that exposes PDF **text extraction** (page count, per-page
text, keyword search, region/word-coordinate extraction) through the `BluePrismPdfHelper`
.NET wrapper built on [PdfPig](https://github.com/UglyToad/PdfPig).

- **Release:** [`pdfpig-reader-v1.0.bprelease`](./pdfpig-reader-v1.0.bprelease)
- **VBO version:** 1.0
- **Exported from:** Blue Prism 7.4.1
- **Wrapper DLL version:** 1.0.0 (PdfPig 0.1.9, `net462`)

## What it does

Same capabilities as the wrapper: `Get Page Count`, `Get All Text`, `Get Page Text`,
`Get All Pages As Table`, `Search Text`, `Get Text By Region`, `Get Words With Coordinates`
(plus `Get Version`). Full Action-by-Action documentation:
[dotnet-wrappers/pdfpig-reader/docs/vbo-actions-reference.md](../../dotnet-wrappers/pdfpig-reader/docs/vbo-actions-reference.md).

## Install

1. **Copy the DLLs.** Place these into
   `C:\Program Files\Blue Prism Limited\Blue Prism Automate\`:
   - `BluePrismPdfHelper.dll`
   - the six `UglyToad.PdfPig.*.dll` files

   You can take the prebuilt ones from
   [dotnet-wrappers/pdfpig-reader/dist/](../../dotnet-wrappers/pdfpig-reader/dist/), or build
   them yourself with the scripts in
   [dotnet-wrappers/pdfpig-reader/build/](../../dotnet-wrappers/pdfpig-reader/build/).

2. **Import the release.** In the Blue Prism client:
   **System → Release Manager → Import**, then select
   [`pdfpig-reader-v1.0.bprelease`](./pdfpig-reader-v1.0.bprelease).

3. **Check references.** Open the imported VBO and confirm, under **Edit → References**,
   that `BluePrismPdfHelper.dll` and the six `UglyToad.PdfPig.*.dll` files are listed
   (see the [action reference](../../dotnet-wrappers/pdfpig-reader/docs/vbo-actions-reference.md#dependencies)).

## Related

- **Source, build scripts, and prebuilt DLLs:**
  [dotnet-wrappers/pdfpig-reader/](../../dotnet-wrappers/pdfpig-reader/)
- **Notes & limitations** (text-based PDFs only, coordinate system, etc.): see the
  [wrapper README](../../dotnet-wrappers/pdfpig-reader/README.md#notes--limitations).

> **Tested with:** Blue Prism 7.4.1 · .NET Framework 4.6.2 · PdfPig 0.1.9
