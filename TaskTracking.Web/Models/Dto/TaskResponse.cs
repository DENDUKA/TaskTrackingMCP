using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Models.Dto;

public class TaskResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public KanbanStatus Status { get; set; }
    public Guid BoardId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CommentResponse> Comments { get; set; } = new();
}
