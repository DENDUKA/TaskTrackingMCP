using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Models.Dto;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("mcp")]
public class McpController(ITaskService taskService, IBoardService boardService, IAccountService accountService) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ITaskService _taskService = taskService;
    private readonly IBoardService _boardService = boardService;
    private readonly IAccountService _accountService = accountService;
    private readonly IReadOnlyList<ToolDefinition> _tools = CreateTools();

    [HttpPost]
    public async Task<IActionResult> Handle()
    {
        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(Request.Body);
        }
        catch
        {
            return BadRequest();
        }

        var root = document.RootElement;
        if (!root.TryGetProperty("method", out var methodElement) || methodElement.ValueKind != JsonValueKind.String)
        {
            return BadRequest();
        }

        var method = methodElement.GetString();
        var hasId = root.TryGetProperty("id", out var idElement);
        root.TryGetProperty("params", out var paramsElement);

        if (!hasId)
        {
            return NoContent();
        }

        object response = method switch
        {
            "initialize" => CreateResponse(idElement, new
            {
                protocolVersion = "2024-11-05",
                capabilities = new
                {
                    tools = new
                    {
                        listChanged = false
                    }
                },
                serverInfo = new
                {
                    name = "TaskTracking.Mcp",
                    version = "1.0.0"
                }
            }),
            "tools/list" => CreateResponse(idElement, new { tools = _tools }),
            "tools/call" => CreateResponse(idElement, await ExecuteToolAsync(paramsElement)),
            _ => CreateError(idElement, -32601, "Method not found")
        };

        return new JsonResult(response, JsonOptions);
    }

    private Task<object> ExecuteToolAsync(JsonElement paramsElement)
    {
        if (paramsElement.ValueKind != JsonValueKind.Object)
        {
            return Task.FromResult(CreateToolError("Invalid params"));
        }

        if (!paramsElement.TryGetProperty("name", out var nameElement) || nameElement.ValueKind != JsonValueKind.String)
        {
            return Task.FromResult(CreateToolError("Tool name is required"));
        }

        paramsElement.TryGetProperty("arguments", out var argsElement);
        var toolName = nameElement.GetString() ?? string.Empty;

        return toolName switch
        {
            "get_tasks_by_board" => Task.FromResult(GetTasksByBoard(argsElement)),
            "get_task" => Task.FromResult(GetTask(argsElement)),
            "get_status_catalog" => Task.FromResult(GetStatusCatalog()),
            "get_tasks_by_assignee" => Task.FromResult(GetTasksByAssignee(argsElement)),
            "get_tasks_unassigned" => Task.FromResult(GetTasksUnassigned()),
            "change_task_status" => Task.FromResult(ChangeTaskStatus(argsElement)),
            "cancel_task" => Task.FromResult(CancelTask(argsElement)),
            "add_comment" => Task.FromResult(AddComment(argsElement)),
            "delete_comment" => Task.FromResult(DeleteComment(argsElement)),
            _ => Task.FromResult(CreateToolError($"Unknown tool: {toolName}"))
        };
    }

    private object GetTasksByBoard(JsonElement argsElement)
    {
        var boardId = GetRequiredGuid(argsElement, "boardId");
        if (boardId is null)
        {
            return CreateToolError("boardId is required");
        }

        var board = _boardService.GetBoard(boardId.Value);
        if (board is null)
        {
            return CreateToolError("Доска не найдена");
        }

        var tasks = _taskService.GetTasks(boardId.Value);
        return CreateToolResult("Задачи получены", new { tasks });
    }

    private object GetTask(JsonElement argsElement)
    {
        var taskId = GetRequiredGuid(argsElement, "taskId");
        if (taskId is null)
        {
            return CreateToolError("taskId is required");
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
        {
            return CreateToolError("Задача не найдена");
        }

        var comments = _taskService.GetComments(taskId.Value);
        var users = _accountService.GetUsers();

        var response = new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            BoardId = task.BoardId,
            AssigneeId = task.AssigneeId,
            CreatedAt = task.CreatedAt,
            Comments = comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                TaskId = c.TaskId,
                AuthorId = c.AuthorId,
                AuthorName = users.FirstOrDefault(u => u.Id == c.AuthorId)?.Name ?? "Неизвестный",
                Text = c.Text,
                CreatedAt = c.CreatedAt
            }).ToList()
        };

        return CreateToolResult("Задача получена", new { task = response });
    }

    private object GetStatusCatalog()
    {
        var statuses = Enum.GetValues<KanbanStatus>()
            .Select(status => new { name = status.ToString(), value = (int)status })
            .ToList();

        return CreateToolResult("Справочник статусов", new { statuses });
    }

    private object GetTasksByAssignee(JsonElement argsElement)
    {
        var assigneeId = GetRequiredGuid(argsElement, "assigneeId");
        if (assigneeId is null)
        {
            return CreateToolError("assigneeId is required");
        }

        var tasks = _taskService.GetTasks()
            .Where(t => t.AssigneeId == assigneeId.Value)
            .ToList();

        return CreateToolResult("Задачи по исполнителю получены", new { tasks });
    }

    private object GetTasksUnassigned()
    {
        var tasks = _taskService.GetTasks()
            .Where(t => t.AssigneeId == null)
            .ToList();

        return CreateToolResult("Задачи без исполнителя получены", new { tasks });
    }

    private object ChangeTaskStatus(JsonElement argsElement)
    {
        var taskId = GetRequiredGuid(argsElement, "taskId");
        var authKey = GetRequiredGuid(argsElement, "authKey");
        var newStatus = GetRequiredInt(argsElement, "newStatus");
        if (taskId is null || authKey is null || newStatus is null)
        {
            return CreateToolError("taskId, authKey, newStatus are required");
        }

        if (!Enum.IsDefined(typeof(KanbanStatus), newStatus.Value))
        {
            return CreateToolError("Недопустимый статус");
        }

        var user = _accountService.GetUserByAuthKey(authKey.Value);
        if (user is null)
        {
            return CreateToolError("Unauthorized");
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
        {
            return CreateToolError("Задача не найдена");
        }

        _taskService.UpdateTaskStatus(taskId.Value, (KanbanStatus)newStatus.Value);
        return CreateToolResult("Статус обновлён", new { taskId, newStatus });
    }

    private object CancelTask(JsonElement argsElement)
    {
        var taskId = GetRequiredGuid(argsElement, "taskId");
        var authKey = GetRequiredGuid(argsElement, "authKey");
        if (taskId is null || authKey is null)
        {
            return CreateToolError("taskId, authKey are required");
        }

        var user = _accountService.GetUserByAuthKey(authKey.Value);
        if (user is null)
        {
            return CreateToolError("Unauthorized");
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
        {
            return CreateToolError("Задача не найдена");
        }

        _taskService.UpdateTaskStatus(taskId.Value, KanbanStatus.Cancelled);
        return CreateToolResult("Задача отменена", new { taskId });
    }

    private object AddComment(JsonElement argsElement)
    {
        var taskId = GetRequiredGuid(argsElement, "taskId");
        var authKey = GetRequiredGuid(argsElement, "authKey");
        var text = GetRequiredString(argsElement, "text");
        if (taskId is null || authKey is null || text is null)
        {
            return CreateToolError("taskId, authKey, text are required");
        }

        var user = _accountService.GetUserByAuthKey(authKey.Value);
        if (user is null)
        {
            return CreateToolError("Unauthorized");
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
        {
            return CreateToolError("Задача не найдена");
        }

        var comment = new Comment
        {
            TaskId = taskId.Value,
            AuthorId = user.Id,
            Text = text
        };

        _taskService.AddComment(comment);

        var response = new CommentResponse
        {
            Id = comment.Id,
            TaskId = comment.TaskId,
            AuthorId = comment.AuthorId,
            AuthorName = user.Name,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        };

        return CreateToolResult("Комментарий добавлен", new { comment = response });
    }

    private object DeleteComment(JsonElement argsElement)
    {
        var taskId = GetRequiredGuid(argsElement, "taskId");
        var commentId = GetRequiredGuid(argsElement, "commentId");
        var authKey = GetRequiredGuid(argsElement, "authKey");
        if (taskId is null || commentId is null || authKey is null)
        {
            return CreateToolError("taskId, commentId, authKey are required");
        }

        var user = _accountService.GetUserByAuthKey(authKey.Value);
        if (user is null)
        {
            return CreateToolError("Unauthorized");
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId.Value);
        if (task is null)
        {
            return CreateToolError("Задача не найдена");
        }

        var comments = _taskService.GetComments(taskId.Value);
        var comment = comments.FirstOrDefault(c => c.Id == commentId.Value);
        if (comment is null)
        {
            return CreateToolError("Комментарий не найден");
        }

        _taskService.DeleteComment(commentId.Value);
        return CreateToolResult("Комментарий удалён", new { taskId, commentId });
    }

    private static Guid? GetRequiredGuid(JsonElement argsElement, string propertyName)
    {
        var value = GetRequiredString(argsElement, propertyName);
        if (value is null)
        {
            return null;
        }

        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private static string? GetRequiredString(JsonElement argsElement, string propertyName)
    {
        if (argsElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!argsElement.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            return null;
        }

        return property.GetString();
    }

    private static int? GetRequiredInt(JsonElement argsElement, string propertyName)
    {
        if (argsElement.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!argsElement.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
        {
            return value;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static object CreateResponse(JsonElement id, object result)
    {
        return new
        {
            jsonrpc = "2.0",
            id,
            result
        };
    }

    private static object CreateError(JsonElement id, int code, string message)
    {
        return new
        {
            jsonrpc = "2.0",
            id,
            error = new
            {
                code,
                message
            }
        };
    }

    private static object CreateToolResult(string message, object structuredContent)
    {
        return new
        {
            content = new[]
            {
                new { type = "text", text = message }
            },
            structuredContent,
            isError = false
        };
    }

    private static object CreateToolError(string message)
    {
        return new
        {
            content = new[]
            {
                new { type = "text", text = message }
            },
            isError = true
        };
    }

    private static IReadOnlyList<ToolDefinition> CreateTools()
    {
        return new List<ToolDefinition>
        {
            new("get_tasks_by_board", "Get tasks by board", "Возвращает список задач по доске", ParseSchema("""
            {
              "type": "object",
              "properties": { "boardId": { "type": "string", "format": "uuid" } },
              "required": [ "boardId" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "tasks": { "type": "array" } }
            }
            """)),
            new("get_task", "Get task details", "Возвращает задачу с комментариями", ParseSchema("""
            {
              "type": "object",
              "properties": { "taskId": { "type": "string", "format": "uuid" } },
              "required": [ "taskId" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "task": { "type": "object" } }
            }
            """)),
            new("get_status_catalog", "Get status catalog", "Возвращает допустимые статусы задач", ParseSchema("""
            {
              "type": "object",
              "properties": { }
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": {
                "statuses": {
                  "type": "array",
                  "items": {
                    "type": "object",
                    "properties": {
                      "name": { "type": "string" },
                      "value": { "type": "integer" }
                    }
                  }
                }
              }
            }
            """)),
            new("get_tasks_by_assignee", "Get tasks by assignee", "Возвращает задачи по исполнителю", ParseSchema("""
            {
              "type": "object",
              "properties": { "assigneeId": { "type": "string", "format": "uuid" } },
              "required": [ "assigneeId" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "tasks": { "type": "array" } }
            }
            """)),
            new("get_tasks_unassigned", "Get unassigned tasks", "Возвращает задачи без исполнителя", ParseSchema("""
            {
              "type": "object",
              "properties": { }
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "tasks": { "type": "array" } }
            }
            """)),
            new("change_task_status", "Change task status", "Меняет статус задачи", ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string", "format": "uuid" },
                "authKey": { "type": "string", "format": "uuid" },
                "newStatus": { "type": "integer" }
              },
              "required": [ "taskId", "authKey", "newStatus" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string" },
                "newStatus": { "type": "integer" }
              }
            }
            """)),
            new("cancel_task", "Cancel task", "Отменяет задачу", ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string", "format": "uuid" },
                "authKey": { "type": "string", "format": "uuid" }
              },
              "required": [ "taskId", "authKey" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "taskId": { "type": "string" } }
            }
            """)),
            new("add_comment", "Add comment", "Добавляет комментарий к задаче", ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string", "format": "uuid" },
                "authKey": { "type": "string", "format": "uuid" },
                "text": { "type": "string" }
              },
              "required": [ "taskId", "authKey", "text" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": { "comment": { "type": "object" } }
            }
            """)),
            new("delete_comment", "Delete comment", "Удаляет комментарий у задачи", ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string", "format": "uuid" },
                "commentId": { "type": "string", "format": "uuid" },
                "authKey": { "type": "string", "format": "uuid" }
              },
              "required": [ "taskId", "commentId", "authKey" ]
            }
            """), ParseSchema("""
            {
              "type": "object",
              "properties": {
                "taskId": { "type": "string" },
                "commentId": { "type": "string" }
              }
            }
            """))
        };
    }

    private static JsonElement ParseSchema(string json) => JsonSerializer.Deserialize<JsonElement>(json);

    private sealed record ToolDefinition(string Name, string Title, string Description, JsonElement InputSchema, JsonElement OutputSchema);
}
