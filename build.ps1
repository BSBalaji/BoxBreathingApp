#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and (optionally) packages Box Breathing Tray.

.DESCRIPTION
    1. Runs `dotnet publish` to produce a self-contained single-file exe.
    2. Optionally compiles the Inno Setup installer if iscc.exe is found.

.PARAMETER Configuration
    Build configuration: Release (default) or Debug.

.PARAMETER SkipInstaller
    Skip the Inno Setup step even if iscc.exe is present.
#>
param(
    [string] $Configuration  = "Release",
    [switch] $SkipInstaller
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

Write-Host ""
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Box Breathing Tray — Build Script" -ForegroundColor Cyan
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""

# ── 1. dotnet publish ──────────────────────────────────────────────────────
$publishDir = Join-Path $root "publish"

Write-Host "▶ Publishing ($Configuration, self-contained, win-x64)…" -ForegroundColor Yellow
dotnet publish "$root\BoxBreathingTray.csproj" `
    --configuration $Configuration `
    --runtime win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    --output $publishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish failed." }

Write-Host "✔ Published to: $publishDir" -ForegroundColor Green
Write-Host ""

# ── 2. Inno Setup installer (optional) ────────────────────────────────────
if (-not $SkipInstaller) {
    $iscc = Get-Command iscc -ErrorAction SilentlyContinue

    if (-not $iscc) {
        # Common default install location
        $defaultIscc = "C:\Program Files (x86)\Inno Setup 6\iscc.exe"
        if (Test-Path $defaultIscc) { $iscc = $defaultIscc }
    }

    if ($iscc) {
        Write-Host "▶ Compiling Inno Setup installer…" -ForegroundColor Yellow
        & $iscc "$root\installer.iss"
        if ($LASTEXITCODE -ne 0) { throw "Inno Setup compilation failed." }
        Write-Host "✔ Installer written to: $root\installer\" -ForegroundColor Green
    } else {
        Write-Host "ℹ Inno Setup (iscc.exe) not found — skipping installer step." -ForegroundColor DarkCyan
        Write-Host "  Install from https://jrsoftware.org/isinfo.php and re-run to produce a .exe installer." -ForegroundColor DarkCyan
    }
}

Write-Host ""
Write-Host "Done! Deliverables:" -ForegroundColor Cyan
Write-Host "  EXE : $publishDir\BoxBreathingTray.exe"
if (Test-Path "$root\installer") {
    $pkg = Get-ChildItem "$root\installer\*.exe" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($pkg) { Write-Host "  PKG : $($pkg.FullName)" }
}
Write-Host ""
