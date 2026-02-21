param(
    [string]$AppPoolName = "TaskTrackingAppPool",
    [string]$PublishPath = "C:\Server\TaskTrackingMCP\TaskTrackingPublish",
    [string]$ProjectPath = ".\TaskTracking.Web\TaskTracking.Web.csproj"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting deployment..."

# 1. Stop IIS AppPool (if exists)
Write-Host "Checking IIS AppPool: $AppPoolName"
Import-Module WebAdministration
if (Test-Path "IIS:\AppPools\$AppPoolName") {
    $state = Get-WebAppPoolState -Name $AppPoolName
    if ($state.Value -eq "Started") {
        Write-Host "Stopping AppPool..."
        Stop-WebAppPool -Name $AppPoolName
        Start-Sleep -Seconds 5 # Give it time to release locks
    }
} else {
    Write-Warning "AppPool '$AppPoolName' not found! Please ensure it is created in IIS."
}

# 2. Publish Application
Write-Host "Publishing application..."
Write-Host "Source: $ProjectPath"
Write-Host "Destination: $PublishPath"

# Ensure destination directory exists
if (-not (Test-Path $PublishPath)) {
    New-Item -Path $PublishPath -ItemType Directory -Force
}

# Run dotnet publish
try {
    dotnet publish $ProjectPath -c Release -o $PublishPath
}
catch {
    Write-Error "Publish failed: $_"
    # Attempt to restart AppPool even if publish failed, so site isn't down forever
    if (Test-Path "IIS:\AppPools\$AppPoolName") {
        Start-WebAppPool -Name $AppPoolName
    }
    exit 1
}

# 3. Start IIS AppPool
if (Test-Path "IIS:\AppPools\$AppPoolName") {
    Write-Host "Starting AppPool..."
    Start-WebAppPool -Name $AppPoolName
}

Write-Host "Deployment completed successfully!"
