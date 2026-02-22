using TaskTracking.Web.Application.Abstractions;

namespace TaskTracking.Web.Application.Services;

public class TaskUpdateNotifier : ITaskUpdateNotifier
{
    public event Action<Guid>? BoardTasksChanged;

    public void NotifyBoardTasksChanged(Guid boardId)
    {
        BoardTasksChanged?.Invoke(boardId);
    }
}
