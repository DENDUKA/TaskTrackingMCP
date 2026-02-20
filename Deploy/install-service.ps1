# Скрипт установки Windows службы для TaskTracking
# Запускать от имени администратора

$serviceName = "TaskTracking"
$publishPath = "C:\Apps\TaskTrackingPublish"
$dotnetPath = "C:\Program Files\dotnet\dotnet.exe"

Write-Host "=== Установка службы $serviceName ===" -ForegroundColor Green

# Проверяем, установлен ли NSSM
$nssm = Get-Command nssm -ErrorAction SilentlyContinue
if (-not $nssm) {
    Write-Host "Устанавливаю NSSM..." -ForegroundColor Yellow
    winget install nssm
}

# Останавливаем службу, если она уже существует
$existingService = Get-Service -Name $serviceName -ErrorAction SilentlyContinue
if ($existingService) {
    Write-Host "Останавливаю существующую службу..." -ForegroundColor Yellow
    nssm stop $serviceName
    nssm remove $serviceName confirm
}

# Создаём папку для публикации, если её нет
if (-not (Test-Path $publishPath)) {
    New-Item -ItemType Directory -Path $publishPath -Force
}

# Устанавливаем службу
Write-Host "Устанавливаю службу..." -ForegroundColor Yellow
nssm install $serviceName $dotnetPath "$publishPath\TaskTracking.Web.dll"
nssm set $serviceName AppDirectory $publishPath
nssm set $serviceName Start SERVICE_AUTO_START
nssm set $serviceName AppStdout "C:\Deploy\service-stdout.log"
nssm set $serviceName AppStderr "C:\Deploy\service-stderr.log"

# Устанавливаем переменные окружения
nssm set $serviceName AppEnvironmentExtra "ASPNETCORE_ENVIRONMENT=Production" "ASPNETCORE_URLS=http://*:80"

Write-Host "Служба установлена!" -ForegroundColor Green
Write-Host "Для запуска выполните: nssm start $serviceName" -ForegroundColor Cyan
