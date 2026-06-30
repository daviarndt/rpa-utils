# ============================================================
# 2_copy_dlls.ps1
# Copies the PdfPig (net462) DLLs into the Blue Prism folder so
# they can be referenced by the VBO and by the compile step.
# Requires: 1_download_pdfpig.ps1 has already run.
# ============================================================

Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

$source = "C:\Temp\PdfPigSetup\extracted\lib\net462"
$dest   = "C:\Program Files\Blue Prism Limited\Blue Prism Automate"

$dlls = @(
    "UglyToad.PdfPig.dll",
    "UglyToad.PdfPig.Core.dll",
    "UglyToad.PdfPig.Fonts.dll",
    "UglyToad.PdfPig.Tokenization.dll",
    "UglyToad.PdfPig.Tokens.dll",
    "UglyToad.PdfPig.DocumentLayoutAnalysis.dll"
)

foreach ($dll in $dlls) {
    $src = Join-Path $source $dll
    $dst = Join-Path $dest $dll
    Copy-Item $src $dst -Force
    Write-Host "Copied: $dll"
}

Write-Host "Done."
