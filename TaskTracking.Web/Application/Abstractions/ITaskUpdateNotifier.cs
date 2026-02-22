namespace TaskTracking.Web.Application.Abstractions;

public interface ITaskUpdateNotifier
{
    event Action<Guid> BoardTasksChanged;
    void NotifyBoardTasksChanged(Guid boardId);
}
