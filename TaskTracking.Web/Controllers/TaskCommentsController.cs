using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Models;
using TaskTracking.Web.Models.Dto;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskCommentsController(ITaskService taskService, IAccountService accountService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;
    private readonly IAccountService _accountService = accountService;

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
