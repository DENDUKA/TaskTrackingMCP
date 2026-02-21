using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Models.Dto;

public sealed record CreateTaskRequest(Guid AuthKey, Guid BoardId, string Title, string? Description, Guid? AssigneeId, KanbanStatus? Status);
