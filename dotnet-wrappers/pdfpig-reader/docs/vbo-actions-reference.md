# PDF Helper VBO — Action Reference

**Blue Prism 7.4.1 | BluePrismPdfHelper v1.0.0**

Reference for every Blue Prism Action that wraps `BluePrismPdfHelper.dll`. For build
and deployment instructions, see the [pdfpig-reader README](../README.md).

## Dependencies

The following files must be present in
`C:\Program Files\Blue Prism Limited\Blue Prism Automate\`:

- `BluePrismPdfHelper.dll`
- `UglyToad.PdfPig.dll`
- `UglyToad.PdfPig.Core.dll`
- `UglyToad.PdfPig.Fonts.dll`
- `UglyToad.PdfPig.Tokenization.dll`
- `UglyToad.PdfPig.Tokens.dll`
- `UglyToad.PdfPig.DocumentLayoutAnalysis.dll`

## VBO Configuration

In **Edit > References**, add all of the DLLs listed above.

## Actions

### Get Version

Returns which version of the DLL is loaded in the environment.

**Inputs:** none

| Output | Type |
|---|---|
| `DllVersion` | Text |

```vb
DllVersion = BluePrismPdfHelper.PdfHelper.Version
```

### Get Page Count

Returns the total number of pages in the PDF.

| Input | Type |
|---|---|
| `PdfPath` | Text |

| Output | Type |
|---|---|
| `PageCount` | Number |

```vb
PageCount = BluePrismPdfHelper.PdfHelper.GetPageCount(PdfPath)
```

### Get All Text

Extracts all text from the PDF in one call.

| Input | Type |
|---|---|
| `PdfPath` | Text |

| Output | Type |
|---|---|
| `PdfText` | Text |

```vb
PdfText = BluePrismPdfHelper.PdfHelper.GetAllText(PdfPath)
```

### Get Page Text

Extracts the text of a specific page.

| Input | Type |
|---|---|
| `PdfPath` | Text |
| `PageNumber` | Number |

| Output | Type |
|---|---|
| `PageText` | Text |

```vb
PageText = BluePrismPdfHelper.PdfHelper.GetPageText(PdfPath, CInt(PageNumber))
```

### Get All Pages As Table

Returns all pages as a Collection with the number and text of each page.

| Input | Type |
|---|---|
| `PdfPath` | Text |

| Output | Type |
|---|---|
| `Pages` | Collection |

**Collection columns:** `PageNumber` (Number), `PageText` (Text)

```vb
Pages = BluePrismPdfHelper.PdfHelper.GetAllPagesAsTable(PdfPath)
```

### Search Text

Searches the PDF for a keyword and returns the pages where it was found
(case-insensitive).

| Input | Type |
|---|---|
| `PdfPath` | Text |
| `Keyword` | Text |

| Output | Type |
|---|---|
| `Results` | Collection |

**Collection columns:** `PageNumber` (Number), `Context` (Text)

```vb
Results = BluePrismPdfHelper.PdfHelper.SearchText(PdfPath, Keyword)
```

### Get Text By Region

Extracts text from a specific area of a page using coordinates in points.
Use **Get Words With Coordinates** first to discover the coordinates.

| Input | Type |
|---|---|
| `PdfPath` | Text |
| `PageNumber` | Number |
| `X` | Number |
| `Y` | Number |
| `Width` | Number |
| `Height` | Number |

| Output | Type |
|---|---|
| `RegionText` | Text |

```vb
RegionText = BluePrismPdfHelper.PdfHelper.GetTextByRegion(PdfPath, CInt(PageNumber), CDbl(X), CDbl(Y), CDbl(Width), CDbl(Height))
```

### Get Words With Coordinates

Returns every word on a page together with its exact coordinates.
Useful to map the layout and define regions for **Get Text By Region**.

| Input | Type |
|---|---|
| `PdfPath` | Text |
| `PageNumber` | Number |

| Output | Type |
|---|---|
| `Words` | Collection |

**Collection columns:** `Word` (Text), `X` (Number), `Y` (Number), `Width` (Number), `Height` (Number)

```vb
Words = BluePrismPdfHelper.PdfHelper.GetWordsWithCoordinates(PdfPath, CInt(PageNumber))
```

## Extra Actions (no DLL recompile needed)

These build on the methods above using only inline Code Stage logic.

### PDF Contains Keyword

Returns True/False indicating whether a keyword exists anywhere in the PDF.

**Inputs:** `PdfPath` (Text), `Keyword` (Text)
**Output:** `Found` (Flag)

```vb
Dim results As System.Data.DataTable = BluePrismPdfHelper.PdfHelper.SearchText(PdfPath, Keyword)
Found = (results.Rows.Count > 0)
```

### Get Text From Page Range

Extracts text from a range of pages.

**Inputs:** `PdfPath` (Text), `StartPage` (Number), `EndPage` (Number)
**Output:** `RangeText` (Text)

```vb
Dim sb As New System.Text.StringBuilder()
For i As Integer = CInt(StartPage) To CInt(EndPage)
    Dim pageText As String = BluePrismPdfHelper.PdfHelper.GetPageText(PdfPath, i)
    sb.AppendLine(pageText)
Next
RangeText = sb.ToString()
```

## Changelog

| Version | Date | Changes |
|---|---|---|
| 1.0.0 | 2026-06 | Initial version — 7 base methods. |
