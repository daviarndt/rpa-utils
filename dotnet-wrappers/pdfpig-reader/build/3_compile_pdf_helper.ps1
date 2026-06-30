# ============================================================
# 3_compile_pdf_helper.ps1
# Compiles ../src/BluePrismPdfHelper.cs into BluePrismPdfHelper.dll
# and writes it into the Blue Prism Automate folder.
#
# Single source of truth: this script reads the canonical .cs file
# from the src/ folder, so the code is never duplicated here.
#
# Requires: 2_copy_dlls.ps1 has already run (PdfPig DLLs present in $bpPath).
# ============================================================

Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

$bpPath = "C:\Program Files\Blue Prism Limited\Blue Prism Automate"

# Resolve the canonical source file relative to this script's location.
$sourceFile = Join-Path $PSScriptRoot "..\src\BluePrismPdfHelper.cs"
if (-not (Test-Path $sourceFile)) {
    throw "Source file not found: $sourceFile"
}
$code = Get-Content -Path $sourceFile -Raw

$refs = @(
    "System.Data",
    "System.Xml",
    "System.Core",
    "$bpPath\UglyToad.PdfPig.dll",
    "$bpPath\UglyToad.PdfPig.Core.dll",
    "$bpPath\UglyToad.PdfPig.Fonts.dll",
    "$bpPath\UglyToad.PdfPig.Tokenization.dll",
    "$bpPath\UglyToad.PdfPig.Tokens.dll",
    "$bpPath\UglyToad.PdfPig.DocumentLayoutAnalysis.dll"
)

Add-Type -TypeDefinition $code `
    -ReferencedAssemblies $refs `
    -OutputAssembly "$bpPath\BluePrismPdfHelper.dll" `
    -OutputType Library

Write-Host "BluePrismPdfHelper.dll compiled successfully (v1.0.0)."
