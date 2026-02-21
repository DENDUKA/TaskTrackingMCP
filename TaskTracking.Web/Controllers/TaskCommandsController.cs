using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Models.Dto;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskCommandsController(ITaskService taskService, IBoardService boardService, IAccountService accountService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;
    private readonly IBoardService _boardService = boardService;
    private readonly IAccountService _accountService = accountService;

    [HttpPost]
    public IActionResult CreateTask([FromBody] CreateTaskRequest request)
    {
        var user = _accountService.GetUserByAuthKey(request.AuthKey);
        if (user == null)
        {
            return Unauthorized();
        }

        var board = _boardService.GetBoard(request.BoardId);
        if (board == null)
        {
            return NotFound("Доска не найдена");
        }

        var status = request.Status ?? KanbanStatus.Todo;
        if (!Enum.IsDefined(typeof(KanbanStatus), status))
        {
            return BadRequest("Недопустимый статус");
        }

        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description ?? string.Empty,
            Status = status,
            BoardId = request.BoardId,
            AssigneeId = request.AssigneeId
        };

        _taskService.AddTask(task);

        var response = new TaskResponse
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            BoardId = task.BoardId,
            AssigneeId = task.AssigneeId,
            CreatedAt = task.CreatedAt,
            Comments = []
        };

        return Ok(response);
    }
}
