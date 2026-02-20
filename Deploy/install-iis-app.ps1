# Скрипт создания приложения в IIS
# Запускать от имени администратора

$appName = "TaskTrackingMCP"
$appPoolName = "TaskTrackingMCP"
$publishPath = "C:\Apps\TaskTrackingPublish"
$siteName = "Default Web Site"  # Имя вашего сайта в IIS

Write-Host "=== Установка приложения в IIS ===" -ForegroundColor Green

# Импортируем модуль IIS
Import-Module WebAdministration -ErrorAction SilentlyContinue
if (-not (Get-Module WebAdministration)) {
    Write-Host "Ошибка: модуль WebAdministration не найден. Убедитесь, что IIS установлен." -ForegroundColor Red
    exit 1
}

# Создаём папку для логов
$logsPath = "$publishPath\logs"
if (-not (Test-Path $logsPath)) {
    New-Item -ItemType Directory -Path $logsPath -Force | Out-Null
}

# Копируем web.config
Copy-Item -Path ".\web.config" -Destination $publishPath -Force
Write-Host "web.config скопирован" -ForegroundColor Cyan

# Удаляем существующий пул приложений, если есть
if (Test-Path "IIS:\AppPools\$appPoolName") {
    Write-Host "Удаляю существующий пул приложений..." -ForegroundColor Yellow
    Remove-WebAppPool -Name $appPoolName
}

# Создаём пул приложений
Write-Host "Создаю пул приложений..." -ForegroundColor Yellow
New-WebAppPool -Name $appPoolName
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "managedRuntimeVersion" -Value ""  # No Managed Code
Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name "startMode" -Value "AlwaysRunning"

# Удаляем существующее приложение, если есть
$existingApp = Get-WebApplication -Site $siteName -Name $appName -ErrorAction SilentlyContinue
if ($existingApp) {
    Write-Host "Удаляю существующее приложение..." -ForegroundColor Yellow
    Remove-WebApplication -Site $siteName -Name $appName
}

# Создаём приложение
Write-Host "Создаю приложение..." -ForegroundColor Yellow
New-WebApplication -Site $siteName -Name $appName -PhysicalPath $publishPath -ApplicationPool $appPoolName

# Даём права на папку для IIS
$acl = Get-Acl $publishPath
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl $publishPath $acl
Write-Host "Права доступа настроены" -ForegroundColor Cyan

Write-Host ""
Write-Host "=== Установка завершена! ===" -ForegroundColor Green
Write-Host "Приложение доступно по адресу: http://localhost/$appName/" -ForegroundColor Cyan
