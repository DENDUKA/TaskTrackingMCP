# Скрипт создания задачи в планировщике Windows
# Запускать от имени администратора

$taskName = "TaskTracking-AutoDeploy"
$scriptPath = "C:\Deploy\deploy.ps1"
$intervalMinutes = 5

Write-Host "=== Создание задачи планировщика ===" -ForegroundColor Green

# Удаляем существующую задачу, если есть
$existingTask = Get-ScheduledTask -TaskName $taskName -ErrorAction SilentlyContinue
if ($existingTask) {
    Write-Host "Удаляю существующую задачу..." -ForegroundColor Yellow
    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false
}

# Создаём папку для скриптов, если её нет
if (-not (Test-Path "C:\Deploy")) {
    New-Item -ItemType Directory -Path "C:\Deploy" -Force
}

# Копируем скрипт деплоя
Copy-Item -Path ".\deploy.ps1" -Destination $scriptPath -Force
Write-Host "Скрипт скопирован в $scriptPath" -ForegroundColor Cyan

# Создаём триггер - каждые N минут
$trigger = New-ScheduledTaskTrigger -Once -At (Get-Date) -RepetitionInterval (New-TimeSpan -Minutes $intervalMinutes) -RepetitionDuration (New-TimeSpan -Days 365)

# Создаём действие
$action = New-ScheduledTaskAction -Execute "powershell.exe" -Argument "-ExecutionPolicy Bypass -WindowStyle Hidden -File `"$scriptPath`""

# Настройки задачи
$settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable

# Регистрируем задачу (запуск от SYSTEM)
Register-ScheduledTask -TaskName $taskName -Trigger $trigger -Action $action -Settings $settings -User "SYSTEM" -RunLevel Highest

Write-Host "Задача '$taskName' создана!" -ForegroundColor Green
Write-Host "Проверка обновлений будет выполняться каждые $intervalMinutes минут" -ForegroundColor Cyan
