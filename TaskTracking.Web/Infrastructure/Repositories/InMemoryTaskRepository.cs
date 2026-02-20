using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<TaskItem> _tasks = new();

    public List<TaskItem> GetAll() => _tasks;

    public TaskItem? GetById(Guid id) => _tasks.FirstOrDefault(t => t.Id == id);

    public List<TaskItem> GetByBoardId(Guid boardId) => _tasks.Where(t => t.BoardId == boardId).ToList();

    public void Add(TaskItem task) => _tasks.Add(task);

    public void Update(TaskItem task)
    {
        var existing = _tasks.FirstOrDefault(t => t.Id == task.Id);
        if (existing != null)
        {
            existing.Title = task.Title;
            existing.Description = task.Description;
            existing.Status = task.Status;
            existing.AssigneeId = task.AssigneeId;
            existing.BoardId = task.BoardId;
        }
    }

    public void Delete(Guid id)
    {
        var t = _tasks.FirstOrDefault(x => x.Id == id);
        if (t != null) _tasks.Remove(t);
    }

    public void DeleteByBoardId(Guid boardId)
    {
        _tasks.RemoveAll(t => t.BoardId == boardId);
    }
}