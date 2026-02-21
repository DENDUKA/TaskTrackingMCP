using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Infrastructure.Repositories;

public interface ICommentRepository
{
    List<Comment> GetByTaskId(Guid taskId);
    Comment? GetById(Guid id);
    void Add(Comment comment);
    void Delete(Guid id);
    void DeleteByTaskId(Guid taskId);
}