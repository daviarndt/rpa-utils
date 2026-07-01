# Third-Party Notices

The prebuilt binaries under [`dist/`](./dist/) include third-party components that are
redistributed here for convenience (so locked-down RPA environments without package-manager
access can deploy the VBO without fetching dependencies).

## PdfPig

- Files: `UglyToad.PdfPig.dll`, `UglyToad.PdfPig.Core.dll`, `UglyToad.PdfPig.Fonts.dll`,
  `UglyToad.PdfPig.Tokenization.dll`, `UglyToad.PdfPig.Tokens.dll`,
  `UglyToad.PdfPig.DocumentLayoutAnalysis.dll`
- Version: 0.1.9
- Project: https://github.com/UglyToad/PdfPig
- License: **Apache License 2.0** — https://github.com/UglyToad/PdfPig/blob/master/LICENSE
- Copyright © the PdfPig / UglyToad contributors.

These DLLs are unmodified redistributions of the official NuGet package. They can also be
obtained directly by running [`build/1_download_pdfpig.ps1`](./build/1_download_pdfpig.ps1).

## BluePrismPdfHelper.dll

`BluePrismPdfHelper.dll` is the wrapper built from this repository's own source
([`src/BluePrismPdfHelper.cs`](./src/BluePrismPdfHelper.cs)) and is covered by this
repository's [code license (MIT)](../../LICENSE-CODE).
