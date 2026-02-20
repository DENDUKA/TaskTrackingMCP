using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Models;
using TaskTracking.Web.Models.Dto;
using TaskTracking.Web.Services;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IBoardService _boardService;
    private readonly IAccountService _accountService;

    public TasksController(ITaskService taskService, IBoardService boardService, IAccountService accountService)
    {
        _taskService = taskService;
        _boardService = boardService;
        _accountService = accountService;
    }

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

    [HttpPost("{id}/status")]
    public IActionResult ChangeStatus(Guid id, [FromBody] ChangeStatusRequest request)
    {
        var user = _accountService.GetUserByAuthKey(request.AuthKey);
        if (user == null)
        {
            return Unauthorized();
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        _taskService.UpdateTaskStatus(id, request.NewStatus);
        return Ok();
    }

    [HttpPost("{id}/cancel")]
    public IActionResult CancelTask(Guid id, [FromBody] CancelTaskRequest request)
    {
        var user = _accountService.GetUserByAuthKey(request.AuthKey);
        if (user == null)
        {
            return Unauthorized();
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        _taskService.UpdateTaskStatus(id, KanbanStatus.Cancelled);
        return Ok();
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

    [HttpPost("{id}/comments")]
    public IActionResult AddComment(Guid id, [FromBody] AddCommentRequest request)
    {
        var user = _accountService.GetUserByAuthKey(request.AuthKey);
        if (user == null)
        {
            return Unauthorized();
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == id);
        if (task == null)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            TaskId = id,
            AuthorId = user.Id,
            Text = request.Text
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

        return Ok(response);
    }

    [HttpDelete("{taskId}/comments/{commentId}")]
    public IActionResult DeleteComment(Guid taskId, Guid commentId, [FromBody] DeleteCommentRequest request)
    {
        var user = _accountService.GetUserByAuthKey(request.AuthKey);
        if (user == null)
        {
            return Unauthorized();
        }

        var task = _taskService.GetTasks().FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            return NotFound();
        }

        var comments = _taskService.GetComments(taskId);
        var comment = comments.FirstOrDefault(c => c.Id == commentId);
        if (comment == null)
        {
            return NotFound();
        }

        _taskService.DeleteComment(commentId);
        return Ok();
    }
}
