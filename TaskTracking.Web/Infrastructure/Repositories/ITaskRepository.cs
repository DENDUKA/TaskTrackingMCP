using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public interface ITaskRepository
{
    List<TaskItem> GetAll();
    TaskItem? GetById(Guid id);
    List<TaskItem> GetByBoardId(Guid boardId);
    void Add(TaskItem task);
    void Update(TaskItem task);
    void Delete(Guid id);
    void DeleteByBoardId(Guid boardId);
}