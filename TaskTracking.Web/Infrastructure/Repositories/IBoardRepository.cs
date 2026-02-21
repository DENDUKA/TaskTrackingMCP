using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Infrastructure.Repositories;

public interface IBoardRepository
{
    List<Board> GetAll();
    List<Board> GetActive();
    Board? GetById(Guid id);
    void Add(Board board);
    void Update(Board board);
    void Delete(Guid id);
}
