# Первоначальная настройка сервера
# Запускать от имени администратора

param(
    [Parameter(Mandatory=$true)]
    [string]$GitRepoUrl https://github.com/DENDUKA/TaskTrackingMCP  # URL репозитория
)

$repoPath = "C:\Apps\TaskTrackingMCP"
$publishPath = "C:\Apps\TaskTrackingPublish"
$deployPath = "C:\Deploy"

Write-Host "=== Первоначальная настройка сервера ===" -ForegroundColor Green

# Создаём необходимые папки
Write-Host "Создаю папки..." -ForegroundColor Yellow
@($repoPath, $publishPath, $deployPath) | ForEach-Object {
    if (-not (Test-Path $_)) {
        New-Item -ItemType Directory -Path $_ -Force | Out-Null
        Write-Host "  Создана: $_" -ForegroundColor Cyan
    }
}

# Клонируем репозиторий
Write-Host "Клонирую репозиторий..." -ForegroundColor Yellow
if (Test-Path "$repoPath\.git") {
    Write-Host "  Репозиторий уже существует, делаю pull..." -ForegroundColor Cyan
    Set-Location $repoPath
    git pull origin main
} else {
    git clone $GitRepoUrl $repoPath
    Set-Location $repoPath
}

# Первоначальная сборка
Write-Host "Собираю проект..." -ForegroundColor Yellow
dotnet publish TaskTracking.Web/TaskTracking.Web.csproj -c Release -o $publishPath

Write-Host ""
Write-Host "=== Первоначальная настройка завершена! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Следующие шаги:" -ForegroundColor Yellow
Write-Host "1. Запустите install-service.ps1 для создания Windows службы" -ForegroundColor Cyan
Write-Host "2. Запустите install-scheduled-task.ps1 для автоматического деплоя" -ForegroundColor Cyan
Write-Host "3. Запустите службу: nssm start TaskTracking" -ForegroundColor Cyan
