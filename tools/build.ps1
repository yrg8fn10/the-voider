param([string]$GameDir = "")
$ErrorActionPreference = "Continue"
$root = Split-Path -Parent $PSScriptRoot
Write-Host "=== THE VOIDER BUILD ===" -ForegroundColor Cyan
Write-Host ""

# 1) Check for the .NET SDK (the compiler)
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "PROBLEM: the .NET SDK is not installed on this PC." -ForegroundColor Red
    Write-Host "  FIX: install the free SDK from https://dotnet.microsoft.com/download"
    Write-Host "  Then run build.bat again."
    Read-Host "Press Enter to close"
    exit 1
}
Write-Host "[1/4] .NET SDK found: $((dotnet --version) 2>$null)"

# 2) Find the Subnautica install
$possible = @()
$steam = (Get-ItemProperty "HKCU:\Software\Valve\Steam" -ErrorAction SilentlyContinue).SteamPath
if (-not $steam) { $steam = (Get-ItemProperty "HKLM:\SOFTWARE\WOW6432Node\Valve\Steam" -ErrorAction SilentlyContinue).InstallPath }
if ($steam) {
    $possible += Join-Path $steam "steamapps\common\Subnautica"
    $vdf = Join-Path $steam "steamapps\libraryfolders.vdf"
    if (Test-Path $vdf) {
        Get-Content $vdf | Select-String '"path"' | ForEach-Object {
            $p = (($_.Line -split '"')[3]) -replace '\\\\', '\'
            if ($p) { $possible += Join-Path $p "steamapps\common\Subnautica" }
        }
    }
}
$possible += @(
    "${env:ProgramFiles(x86)}\Steam\steamapps\common\Subnautica",
    "${env:ProgramFiles(x86)}\SteamLibrary\steamapps\common\Subnautica",
    "D:\SteamLibrary\steamapps\common\Subnautica",
    "E:\SteamLibrary\steamapps\common\Subnautica",
    "C:\Program Files\Epic Games\Subnautica"
)
if ($GameDir -ne "") { $possible = @($GameDir) + $possible }
$game = $possible | Where-Object { $_ -and (Test-Path (Join-Path $_ "Subnautica_Data\Managed")) } | Select-Object -First 1
if (-not $game) {
    Write-Host "PROBLEM: couldn't find your Subnautica install automatically." -ForegroundColor Red
    Write-Host "  FIX: run this command with your game folder (Steam > right-click Subnautica > Manage > Browse local files):"
    Write-Host '  powershell -NoProfile -ExecutionPolicy Bypass -File tools\build.ps1 -GameDir "C:\Program Files (x86)\Steam\steamapps\common\Subnautica"'
    Read-Host "Press Enter to close"
    exit 1
}
Write-Host "[2/4] Subnautica found: $game"

# 3) Stage the game files the compiler needs
$managed = Join-Path $game "Subnautica_Data\Managed"
$libs = Join-Path $root "libs"
New-Item -ItemType Directory -Force -Path $libs | Out-Null
$wanted = @(
    "UnityEngine.dll", "UnityEngine.CoreModule.dll", "UnityEngine.PhysicsModule.dll",
    "UnityEngine.InputLegacyModule.dll", "UnityEngine.AudioModule.dll", "UnityEngine.UIModule.dll",
    "UnityEngine.TextRenderingModule.dll", "Assembly-CSharp.dll", "Assembly-CSharp-firstpass.dll"
)
$missing = @()
foreach ($d in $wanted) {
    $src = Join-Path $managed $d
    if (Test-Path $src) { Copy-Item $src (Join-Path $libs $d) -Force } else { $missing += $d }
}
if ($missing.Count -gt 0) {
    Write-Host ("NOTE: not found in the game folder (usually harmless): " + ($missing -join ", ")) -ForegroundColor Yellow
}
Write-Host "[3/4] Game files staged into libs\"

# 4) Build
Write-Host "[4/4] Building VoiderMod..."
dotnet build (Join-Path $root "src\VoiderMod\VoiderMod.csproj") -c Release --nologo
if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "SUCCESS: the mod built." -ForegroundColor Green
    Write-Host "  The DLL is at: src\VoiderMod\bin\Release\net48\VoiderMod.dll"
    Write-Host "  For a quick test: copy VoiderMod.dll into your game's BepInEx\plugins folder."
    Write-Host "  (Full installs come with the next release, which will include the DLL.)"
} else {
    Write-Host ""
    Write-Host "PROBLEM: the build failed." -ForegroundColor Red
    Write-Host "  FIX: copy the red error lines above and paste them to Fo."
}
Read-Host "Press Enter to close"
exit $LASTEXITCODE
