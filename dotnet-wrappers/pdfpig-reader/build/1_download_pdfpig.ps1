# ============================================================
# 1_download_pdfpig.ps1
# Downloads the PdfPig NuGet package and extracts its DLLs.
# Run this first, then 2_copy_dlls.ps1, then 3_compile_pdf_helper.ps1.
# ============================================================

Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Working folder
New-Item -ItemType Directory -Force -Path "C:\Temp\PdfPigSetup" | Out-Null

# Download the PdfPig NuGet package (pin the version intentionally)
$url = "https://www.nuget.org/api/v2/package/PdfPig/0.1.9"
$output = "C:\Temp\PdfPigSetup\pdfpig.nupkg"
Invoke-WebRequest -Uri $url -OutFile $output

# A .nupkg is a ZIP archive: copy to .zip and extract
$zip = "C:\Temp\PdfPigSetup\pdfpig.zip"
Copy-Item $output $zip -Force
Expand-Archive -Path $zip -DestinationPath "C:\Temp\PdfPigSetup\extracted" -Force

Write-Host "Download complete. Extracted DLLs:"
Get-ChildItem "C:\Temp\PdfPigSetup\extracted" -Recurse -Filter "*.dll" | Select-Object FullName
