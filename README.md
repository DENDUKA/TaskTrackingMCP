# TaskTrackingMCP

Система управления задачами на базе **ASP.NET Core Blazor** с поддержкой Kanban-досок.

## Описание

TaskTrackingMCP — это веб-приложение для отслеживания задач, реализованное с использованием:

- **ASP.NET Core 10.0**
- **Blazor Interactive Server**
- **In-memory хранилище данных**

## Возможности

- Авторизация по ключу (AuthKey) с поддержкой входа через URL
- Создание и управление досками (Boards) с возможностью архивации
- Kanban-доска для задач со статусами:
  - Todo (Запланировано)
  - In Progress (В работе)
  - Code Review (На проверке)
  - Done (Готово)
  - Accepted (Принято)
- Управление пользователями
- Назначение исполнителей на задачи
- Переключение светлой/тёмной темы с сохранением в профиле пользователя
- Интерактивный серверный режим рендеринга
- Обработка переподключений при потере связи

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

## Структура проекта

```
TaskTrackingMCP/
├── TaskTracking.slnx              # Решение
└── TaskTracking.Web/              # Веб-проект
    ├── Components/
    │   ├── Layout/                # Компоненты макета
    │   │   ├── MainLayout.razor   # Основной макет
    │   │   ├── LoginLayout.razor  # Макет страницы входа
    │   │   ├── NavMenu.razor      # Навигационное меню
    │   │   ├── ThemeToggle.razor  # Переключатель темы
    │   │   └── ReconnectModal.razor # Модальное окно переподключения
    │   ├── Pages/                 # Страницы приложения
    │   │   ├── Home.razor         # Главная страница
    │   │   ├── Login.razor        # Страница входа
    │   │   ├── Boards.razor       # Управление досками
    │   │   ├── Users.razor        # Управление пользователями
    │   │   ├── TestUsers.razor    # Тестовые пользователи
    │   │   ├── Error.razor        # Страница ошибок
    │   │   └── NotFound.razor     # Страница 404
    │   ├── _Imports.razor
    │   ├── App.razor
    │   └── Routes.razor
    ├── Models/
    │   ├── Board.cs               # Модель доски
    │   ├── TaskItem.cs            # Модель задачи
    │   └── User.cs                # Модель пользователя
    ├── Services/
    │   ├── TaskService.cs         # Сервис управления задачами
    │   └── CurrentUserService.cs  # Сервис текущего пользователя
    ├── wwwroot/                   # Статические файлы
    ├── appsettings.json           # Конфигурация
    └── Program.cs                 # Точка входа
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

## Конфигурация

### appsettings.Development.json

Настройки для среды разработки.

## API

Приложение использует сервисную архитектуру с интерфейсами:

### ITaskService

```csharp
public interface ITaskService
{
    // Boards
    List<Board> GetBoards();           // Только активные доски
    List<Board> GetAllBoards();        // Все доски включая архивированные
    Board? GetBoard(Guid id);
    void AddBoard(Board board);
    void UpdateBoard(Board board);
    void DeleteBoard(Guid id);

    // Tasks
    List<TaskItem> GetTasks(Guid boardId);
    List<TaskItem> GetTasks();         // Все задачи
    void AddTask(TaskItem task);
    void UpdateTaskStatus(Guid id, KanbanStatus newStatus);
    void UpdateTask(TaskItem task);
    void DeleteTask(Guid id);

    // Users
    List<User> GetUsers();
    User? GetUserByAuthKey(Guid authKey);
    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(Guid id);
}
```

### ICurrentUserService

```csharp
public interface ICurrentUserService
{
    User? CurrentUser { get; }
    event Action? OnCurrentUserChanged;
    bool Login(Guid authKey);
    void Logout();
}
```

## Безопасность

- Авторизация по уникальному ключу AuthKey (GUID)
- Включена защита от CSRF-атак (Antiforgery)
- HSTS для production-среды
- Обработка ошибок через Exception Handler

## Лицензия

MIT
