using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Models.Dto;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskQueriesController(ITaskService taskService, IBoardService boardService, IAccountService accountService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;
    private readonly IBoardService _boardService = boardService;
    private readonly IAccountService _accountService = accountService;

    [HttpGet("by-board/{boardId}")]
    public IActionResult GetTasksByBoard(Guid boardId)
    {
        var board = _boardService.GetBoard(boardId);
        if (board == null)
        {
            return NotFound("Доска не найдена");
        }

        var tasks = _taskService.GetTasks(boardId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public IActionResult GetTask(Guid id)
    {
        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        var comments = _taskService.GetComments(id);
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

        return Ok(response);
    }
}
