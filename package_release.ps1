param(
    [string]$OutputDir = "releases"
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$releasePath = Join-Path $repoRoot $OutputDir
$artifactFiles = @(
    "manifest.json",
    "README.md",
    "icon.png",
    "IceItemIcons.dll"
)

foreach ($file in $artifactFiles) {
    $fullPath = Join-Path $repoRoot $file
    if (-not (Test-Path $fullPath)) {
        throw "Required release file not found: $fullPath"
    }
}

New-Item -ItemType Directory -Force -Path $releasePath | Out-Null
$zipPath = Join-Path $releasePath "IceItemIcons.zip"
if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

Compress-Archive -Path ($artifactFiles | ForEach-Object { Join-Path $repoRoot $_ }) -DestinationPath $zipPath -CompressionLevel Optimal -Force

Write-Host "Created release package: $zipPath"
