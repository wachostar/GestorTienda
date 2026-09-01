# Smoke test script for GestorTienda
# Usage: Open PowerShell and run: .\scripts\smoke_tests.ps1

$ErrorActionPreference = 'Stop'

Write-Host 'Restoring and building solution...'
dotnet restore

dotnet build

Write-Host 'Running DB initializer tool (creates DB under %LOCALAPPDATA%\GestorTienda)...'
dotnet run --project Tools/InitDb/InitDb.vbproj

$localApp = [Environment]::GetFolderPath('LocalApplicationData')
$dbPath = Join-Path $localApp 'GestorTienda\GestorTienda.db'
$logPath = Join-Path $localApp 'GestorTienda\logs\app.log'

If (Test-Path $dbPath) {
    Write-Host "OK: DB file exists at $dbPath"
} else {
    Write-Error "FAIL: DB file was not found at $dbPath"
    exit 1
}

Write-Host 'Checking logger path...'
If (Test-Path (Split-Path $logPath)) {
    Write-Host "OK: log directory exists: $(Split-Path $logPath)"
} else {
    Write-Host "WARN: log directory not present (it will be created at runtime): $(Split-Path $logPath)"
}

Write-Host 'Smoke tests finished successfully.'
exit 0
