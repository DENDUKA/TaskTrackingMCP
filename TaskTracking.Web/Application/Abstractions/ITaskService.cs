using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Application.Abstractions;

public interface ITaskService
{
    List<TaskItem> GetTasks(Guid boardId);
    List<TaskItem> GetTasks();
    void AddTask(TaskItem task);
    void UpdateTaskStatus(Guid id, KanbanStatus newStatus);
    void UpdateTask(TaskItem task);
    void DeleteTask(Guid id);

    List<Comment> GetComments(Guid taskId);
    void AddComment(Comment comment);
    void DeleteComment(Guid commentId);
}
