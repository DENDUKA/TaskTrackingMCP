# Скрипт автоматического деплоя для IIS
# Запускается планировщиком задач каждые N минут

$repoPath = "C:\Server\TaskTrackingMCP\TaskTrackingMCP"
$publishPath = "C:\Server\TaskTrackingMCP\TaskTrackingPublish"
$appPoolName = "TaskTrackingMCP"
$logFile = "C:\Deploy\deploy.log"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    "$timestamp - $Message" | Out-File -Append -FilePath $logFile
    Write-Host $Message
}

try {
    Set-Location $repoPath

    git fetch origin main 2>&1 | Out-Null

    $localCommit = git rev-parse HEAD
    $remoteCommit = git rev-parse origin/main

    if ($localCommit -ne $remoteCommit) {
        Write-Log "Обнаружено обновление: $localCommit -> $remoteCommit"

        # Останавливаем пул приложений
        Import-Module WebAdministration
        Stop-WebAppPool -Name $appPoolName
        Write-Log "Пул приложений остановлен"

        # Ждём завершения процессов
        Start-Sleep -Seconds 3

        # Пулим изменения
        git pull origin main
        Write-Log "Git pull выполнен"

        # Собираем проект
        dotnet publish TaskTracking.Web/TaskTracking.Web.csproj -c Release -o $publishPath
        Write-Log "Проект собран"

        # Копируем web.config если его нет
        if (-not (Test-Path "$publishPath\web.config")) {
            Copy-Item -Path "$repoPath\Deploy\web.config" -Destination $publishPath -Force
        }

        # Запускаем пул приложений
        Start-WebAppPool -Name $appPoolName
        Write-Log "Пул приложений запущен"

        Write-Log "Деплой завершён успешно"
    }
}
catch {
    Write-Log "Ошибка: $_"

    # Пытаемся запустить пул в случае ошибки
    try {
        Import-Module WebAdministration
        Start-WebAppPool -Name $appPoolName -ErrorAction SilentlyContinue
    } catch {}
}
