using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class InMemoryBoardRepository : IBoardRepository
{
    private readonly List<Board> _boards = new();

    public List<Board> GetAll() => _boards;

    public List<Board> GetActive() => _boards.Where(b => b.IsActive).ToList();

    public Board? GetById(Guid id) => _boards.FirstOrDefault(b => b.Id == id);

    public void Add(Board board) => _boards.Add(board);

    public void Update(Board board)
    {
        var existing = _boards.FirstOrDefault(b => b.Id == board.Id);
        if (existing != null)
        {
            existing.Name = board.Name;
            existing.Description = board.Description;
            existing.IsActive = board.IsActive;
        }
    }

    public void Delete(Guid id)
    {
        var b = _boards.FirstOrDefault(x => x.Id == id);
        if (b != null) _boards.Remove(b);
    }
}