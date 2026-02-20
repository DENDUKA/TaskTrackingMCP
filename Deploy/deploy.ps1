# Скрипт автоматического деплоя при обновлении ветки main
# Запускается планировщиком задач каждые N минут

$repoPath = "C:\Apps\TaskTrackingMCP"
$publishPath = "C:\Apps\TaskTrackingPublish"
$serviceName = "TaskTracking"
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

        # Пулим изменения
        git pull origin main
        Write-Log "Git pull выполнен"

        # Собираем проект
        dotnet publish TaskTracking.Web/TaskTracking.Web.csproj -c Release -o $publishPath
        Write-Log "Проект собран"

        # Перезапускаем службу
        Restart-Service $serviceName
        Write-Log "Служба перезапущена"

        Write-Log "Деплой завершён успешно"
    }
}
catch {
    Write-Log "Ошибка: $_"
}
