param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$launcherRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Split-Path -Parent $launcherRoot
$launcherProject = Join-Path $launcherRoot 'Fable2PubGames.Launcher\Fable2PubGames.Launcher.csproj'
$gameBuild = Join-Path $projectRoot 'out\build\win-amd64-release'
$releaseRoot = Join-Path $projectRoot 'out\release\Fable2PubGames'

$requiredGameFiles = @(
    'Fable2PubGames.exe',
    'rexruntime.dll',
    'rexgpu-xenos.dll',
    'Fable2PubGames.toml'
)

foreach ($file in $requiredGameFiles) {
    $path = Join-Path $gameBuild $file
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required game build file is missing: $path"
    }
}

# This is also the local launcher test folder. Never remove user-created data,
# saves, achievements, cache, logs, backups or launcher settings when republishing.
if (-not (Test-Path -LiteralPath $releaseRoot)) {
    New-Item -ItemType Directory -Path $releaseRoot | Out-Null
}

dotnet publish $launcherProject `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $releaseRoot

if ($LASTEXITCODE -ne 0) {
    throw "Launcher publish failed with exit code $LASTEXITCODE"
}

foreach ($file in $requiredGameFiles) {
    Copy-Item -LiteralPath (Join-Path $gameBuild $file) -Destination $releaseRoot -Force
}

Write-Host "Release created at $releaseRoot"
