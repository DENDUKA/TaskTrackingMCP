using Microsoft.AspNetCore.Mvc;
using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Models.Dto;

namespace TaskTracking.Web.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskStatusController(ITaskService taskService, IAccountService accountService) : ControllerBase
{
    private readonly ITaskService _taskService = taskService;
    private readonly IAccountService _accountService = accountService;

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
}
