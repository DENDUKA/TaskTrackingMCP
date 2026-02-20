using TaskTracking.Web.Models;

namespace TaskTracking.Web.Services;

public interface IBoardService
{
    List<Board> GetAllBoards();
    List<Board> GetBoards();
    Board? GetBoard(Guid id);
    void AddBoard(Board board);
    void UpdateBoard(Board board);
    void DeleteBoard(Guid id);
}
