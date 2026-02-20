using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class InMemoryCommentRepository : ICommentRepository
{
    private readonly List<Comment> _comments = new();

    public List<Comment> GetByTaskId(Guid taskId) => _comments.Where(c => c.TaskId == taskId).OrderBy(c => c.CreatedAt).ToList();

    public Comment? GetById(Guid id) => _comments.FirstOrDefault(c => c.Id == id);

    public void Add(Comment comment) => _comments.Add(comment);

    public void Delete(Guid id)
    {
        var c = _comments.FirstOrDefault(x => x.Id == id);
        if (c != null) _comments.Remove(c);
    }

    public void DeleteByTaskId(Guid taskId)
    {
        _comments.RemoveAll(c => c.TaskId == taskId);
    }
}