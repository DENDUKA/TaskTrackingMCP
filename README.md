# TaskTrackingMCP

Система управления задачами на базе **ASP.NET Core 10 + Blazor (Interactive Server)** с поддержкой Kanban-досок и хранилищем **SQLite**.

## Описание

TaskTrackingMCP — это веб‑приложение для отслеживания задач, реализованное с использованием:

- **ASP.NET Core 10.0**
- **Blazor Interactive Server**
- **SQLite** в качестве встроенной БД (создаётся и инициализируется автоматически)
- Архитектура с разделением на слои: Application, Domain, Infrastructure, UI

## Возможности

- Авторизация по ключу (AuthKey) с поддержкой входа через URL
- Создание и управление досками (Boards) с возможностью архивации
- Kanban-доска для задач со статусами:
  - Todo (Запланировано)
  - In Progress (В работе)
  - Code Review (На проверке)
  - Done (Готово)
  - Accepted (Принято)
  - Cancelled (Отменено)
- Управление пользователями
- Назначение исполнителей на задачи
- Комментарии к задачам (добавление/удаление)
- Переключение светлой/тёмной темы с сохранением в профиле пользователя
- Интерактивный серверный режим рендеринга
- Обработка переподключений при потере связи
- REST API для работы с задачами + Swagger в режиме Development

## Требования

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Установка и запуск

```bash
# Клонирование репозитория
cd c:\Program\TaskTrackingMCP

# Восстановление зависимостей
dotnet restore

# Запуск приложения
dotnet run --project TaskTracking.Web
```

Приложение будет доступно по адресу:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

(точные порты могут отличаться)

## Проверка качества

В проекте не зафиксированы отдельные команды lint/typecheck. Рекомендуемые базовые команды:

```bash
# Сборка (проверка типов и компиляции)
dotnet build TaskTracking.Web/TaskTracking.Web.csproj

# Форматирование (если установлен dotnet-format)
dotnet format

# Тесты (если есть тестовые проекты)
dotnet test
```

### База данных

- Используется файл SQLite `tasktracking.db` в каталоге `TaskTracking.Web/`.
- Схема создаётся автоматически при старте приложения.
- Начальные данные (см. ниже) добавляются при первом запуске.

## Структура проекта

```
TaskTrackingMCP/
├── TaskTracking.slnx                      # Решение
└── TaskTracking.Web/                      # Веб‑проект
    ├── Application/
    │   ├── Abstractions/                  # Интерфейсы сервисов приложения
    │   └── Services/                      # Реализации сервисов (Task/Board/Account/CurrentUser)
    ├── Domain/
    │   └── Entities/                      # Доменные сущности (Board, TaskItem, User, Comment)
    ├── Infrastructure/
    │   ├── Data/                          # Фабрика подключения к SQLite
    │   └── Repositories/                  # Репозитории поверх SQLite
    ├── Components/
    │   ├── Layout/                        # Макеты и навигация
    │   ├── Pages/                         # Страницы приложения (Home, Boards, Users, Login и др.)
    │   └── Shared/                        # Общие компоненты (KanbanColumn, TaskCard и др.)
    ├── Controllers/                       # REST API (TasksController)
    ├── Models/                            # DTO и вспомогательные модели
    ├── wwwroot/                           # Статические файлы
    ├── appsettings.json                   # Конфигурация
    └── Program.cs                         # Точка входа
```

## Тестовые данные

При запуске приложение создаёт тестовые данные:

**Доска:**
- Default Board — "My first board"

**Задачи:**
- Изучить Blazor (In Progress)
- Настроить CI/CD (Todo)
- Написать документацию (Done)

**Пользователи:**

| Имя | Email | AuthKey |
|-----|-------|---------|
| Иван Иванов | ivan@example.com | `11111111-1111-1111-1111-111111111111` |
| Петр Петров | petr@example.com | `22222222-2222-2222-2222-222222222222` |

Для входа используйте AuthKey на странице `/login` или добавьте параметр `?key=<AuthKey>` к URL.

## Аутентификация

- Аутентификация происходит с помощью уникального ключа `AuthKey` (GUID).
- Можно войти через форму `/login` или передав `?key=<AuthKey>` в адресной строке.
- Текущий пользователь хранится в состоянии приложения и может быть использован в UI и API.

## Конфигурация

### appsettings.Development.json / appsettings.json

Базовые настройки среды разработки/продакшна. Для SQLite дополнительная настройка не требуется — файл БД создаётся автоматически.

## API

Основные REST‑эндпоинты для работы с задачами:

- GET `/api/tasks/by-board/{boardId}` — получить задачи по доске
- GET `/api/tasks/{id}` — получить задачу с комментариями
- POST `/api/tasks/{id}/status` — сменить статус задачи
- POST `/api/tasks/{id}/cancel` — отменить задачу (статус Cancelled)
- POST `/api/tasks/{id}/comments` — добавить комментарий
- DELETE `/api/tasks/{taskId}/comments/{commentId}` — удалить комментарий

Тело запросов:

```json
// POST /api/tasks/{id}/status
{ "authKey": "11111111-1111-1111-1111-111111111111", "newStatus": 1 }

// POST /api/tasks/{id}/cancel
{ "authKey": "11111111-1111-1111-1111-111111111111" }

// POST /api/tasks/{id}/comments
{ "authKey": "11111111-1111-1111-1111-111111111111", "text": "Мой комментарий" }

// DELETE /api/tasks/{taskId}/comments/{commentId} + body
{ "authKey": "11111111-1111-1111-1111-111111111111" }
```

Swagger доступен в режиме разработки по адресу `/swagger`.

## Архитектура и слои

- Application: бизнес‑сервисы и интерфейсы (`ITaskService`, `IBoardService`, `IAccountService`, `ICurrentUserService`)
- Domain: доменные сущности (`Board`, `TaskItem`, `User`, `Comment`)
- Infrastructure: реализация доступа к данным (SQLite), репозитории
- UI: компоненты Blazor и API‑контроллеры

## Безопасность

- Авторизация по уникальному ключу AuthKey (GUID)
- Включена защита от CSRF‑атак для веб‑части (Antiforgery)
- HSTS для production‑среды
- Обработка ошибок через Exception Handler

## Деплой

В каталоге `Deploy/` находятся PowerShell‑скрипты для установки и запуска приложения в среде Windows (сервис/планировщик задач). Используйте их как основу для автоматизации.

## Лицензия

MIT
