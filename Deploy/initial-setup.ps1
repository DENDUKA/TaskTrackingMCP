# Первоначальная настройка сервера
# Запускать от имени администратора

param(
    [string]$GitRepoUrl = "https://github.com/DENDUKA/TaskTrackingMCP"
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

# Копируем web.config
Copy-Item -Path "$repoPath\Deploy\web.config" -Destination $publishPath -Force
Write-Host "web.config скопирован" -ForegroundColor Cyan

# Создаём папку для логов
New-Item -ItemType Directory -Path "$publishPath\logs" -Force | Out-Null

Write-Host ""
Write-Host "=== Первоначальная настройка завершена! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Следующие шаги:" -ForegroundColor Yellow
Write-Host "1. Установите ASP.NET Core Hosting Bundle (если ещё не установлен)" -ForegroundColor Cyan
Write-Host "   https://dotnet.microsoft.com/download/dotnet/10.0" -ForegroundColor Gray
Write-Host "2. Запустите install-iis-app.ps1 для создания приложения в IIS" -ForegroundColor Cyan
Write-Host "3. Запустите install-scheduled-task-iis.ps1 для автодеплоя" -ForegroundColor Cyan
