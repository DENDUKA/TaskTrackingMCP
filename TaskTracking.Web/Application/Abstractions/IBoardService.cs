using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Application.Abstractions;

public interface IBoardService
{
    List<Board> GetAllBoards();
    List<Board> GetBoards();
    Board? GetBoard(Guid id);
    Board? GetMainBoard();
    void AddBoard(Board board);
    void UpdateBoard(Board board);
    void SetMainBoard(Guid id);
    void DeleteBoard(Guid id);
}
