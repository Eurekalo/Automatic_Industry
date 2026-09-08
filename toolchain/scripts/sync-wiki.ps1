# Synchronizes local wiki documentation to GitHub Wiki repository (https://github.com/Eurekalo/Automatic_Industry/wiki)
[CmdletBinding()]
param (
    [string]$Token = $env:GITHUB_TOKEN,
    [string]$RepoUrl = "github.com/Eurekalo/Automatic_Industry.wiki.git"
)

$ErrorActionPreference = "Stop"

$WorkspaceRoot = Resolve-Path "$PSScriptRoot\..\.."
$WikiSource = Join-Path $WorkspaceRoot "wiki"
$TempWikiDir = Join-Path $env:TEMP ("Automatic_Industry_wiki_" + [System.Guid]::NewGuid().ToString("N"))

Write-Host "=== Syncing Automatic Industry Wiki ===" -ForegroundColor Cyan
Write-Host "Source directory: $WikiSource"

if (-not (Test-Path $WikiSource)) {
    Write-Error "Wiki source directory '$WikiSource' does not exist!"
}

if ($Token) {
    $RemoteUrl = "https://$($Token)@$RepoUrl"
} else {
    $RemoteUrl = "https://$RepoUrl"
}

try {
    Write-Host "Cloning wiki git repository..." -ForegroundColor Yellow
    git clone $RemoteUrl $TempWikiDir
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to clone wiki repository directly."
        Write-Warning "NOTE: If this is the first time using the GitHub Wiki, you must create at least one page via the GitHub Web UI (https://github.com/Eurekalo/Automatic_Industry/wiki) to initialize the wiki backend repository."
        return
    }

    Write-Host "Copying documentation pages..." -ForegroundColor Yellow
    Get-ChildItem -Path $WikiSource -Filter "*.md" | ForEach-Object {
        $dest = Join-Path $TempWikiDir $_.Name
        Copy-Item -Path $_.FullName -Destination $dest -Force
        Write-Host "  -> Copied $($_.Name)" -ForegroundColor Green
    }

    Push-Location $TempWikiDir
    try {
        git config user.name "Eurekalo"
        git config user.email "automatic-industry@users.noreply.github.com"
        git add -A
        
        $status = git status --porcelain
        if (-not $status) {
            Write-Host "No changes to commit. Wiki is already up-to-date!" -ForegroundColor Green
        } else {
            git commit -m "docs(wiki): update automated buildings logic and expand multi-mod compatibility documentation"
            Write-Host "Pushing changes to GitHub Wiki..." -ForegroundColor Yellow
            git push origin HEAD
            Write-Host "Successfully synchronized wiki to GitHub!" -ForegroundColor Green
        }
    } finally {
        Pop-Location
    }
} finally {
    if (Test-Path $TempWikiDir) {
        Remove-Item -Path $TempWikiDir -Recurse -Force -ErrorAction SilentlyContinue
    }
}
