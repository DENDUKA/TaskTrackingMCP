using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public interface IBoardRepository
{
    List<Board> GetAll();
    List<Board> GetActive();
    Board? GetById(Guid id);
    void Add(Board board);
    void Update(Board board);
    void Delete(Guid id);
}
