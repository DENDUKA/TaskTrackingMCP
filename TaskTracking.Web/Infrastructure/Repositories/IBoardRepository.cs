using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Infrastructure.Repositories;

public interface IBoardRepository
{
    List<Board> GetAll();
    List<Board> GetActive();
    Board? GetById(Guid id);
    Board? GetMain();
    void Add(Board board);
    void Update(Board board);
    void SetMainBoard(Guid id);
    void Delete(Guid id);
}
