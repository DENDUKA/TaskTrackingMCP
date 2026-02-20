namespace TaskTracking.Web.Models;

public class TaskItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public KanbanStatus Status { get; set; } = KanbanStatus.Todo;
    public Guid BoardId { get; set; }
    public Guid? AssigneeId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum KanbanStatus
{
    Todo,
    InProgress,
    CodeReview,
    Done,
    Accepted,
    Cancelled
}
