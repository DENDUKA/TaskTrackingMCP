using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Infrastructure.Repositories;

namespace TaskTracking.Web.Application.Services;

public class BoardService(IBoardRepository boards, ITaskRepository tasks) : IBoardService
{
    private readonly IBoardRepository _boards = boards;
    private readonly ITaskRepository _tasks = tasks;
    private readonly Lazy<bool> _initialized = new(() => Initialize(boards, tasks));

    public List<Board> GetAllBoards()
    {
        EnsureInitialized();
        return _boards.GetAll();
    }

    public List<Board> GetBoards()
    {
        EnsureInitialized();
        return _boards.GetActive();
    }

    public Board? GetBoard(Guid id)
    {
        EnsureInitialized();
        return _boards.GetById(id);
    }

    public Board? GetMainBoard()
    {
        EnsureInitialized();
        var mainBoard = _boards.GetMain();
        if (mainBoard != null)
        {
            return mainBoard;
        }

        var fallback = _boards.GetActive().FirstOrDefault() ?? _boards.GetAll().FirstOrDefault();
        if (fallback == null)
        {
            return null;
        }

        _boards.SetMainBoard(fallback.Id);
        fallback.IsMain = true;
        return fallback;
    }

    public void AddBoard(Board board)
    {
        EnsureInitialized();
        _boards.Add(board);
        if (board.IsMain)
        {
            _boards.SetMainBoard(board.Id);
        }
        else
        {
            EnsureMainBoard();
        }
    }

    public void UpdateBoard(Board updatedBoard)
    {
        EnsureInitialized();
        var existingBoard = _boards.GetById(updatedBoard.Id);
        if (existingBoard == null)
        {
            throw new InvalidOperationException("Доска не найдена");
        }

        var shouldStayMain = existingBoard.IsMain && updatedBoard.IsActive;
        updatedBoard.IsMain = shouldStayMain;
        _boards.Update(updatedBoard);

        if (!shouldStayMain && existingBoard.IsMain)
        {
            EnsureMainBoard();
        }
    }

    public void SetMainBoard(Guid id)
    {
        EnsureInitialized();
        var board = _boards.GetById(id);
        if (board == null)
        {
            throw new InvalidOperationException("Доска не найдена");
        }

        if (!board.IsActive)
        {
            throw new InvalidOperationException("Нельзя сделать архивированную доску главной");
        }

        _boards.SetMainBoard(id);
    }

    public void DeleteBoard(Guid id)
    {
        EnsureInitialized();
        var board = _boards.GetById(id);
        _tasks.DeleteByBoardId(id);
        _boards.Delete(id);

        if (board?.IsMain == true)
        {
            EnsureMainBoard();
        }
    }

    private static bool Initialize(IBoardRepository boards, ITaskRepository tasks)
    {
        if (!boards.GetAll().Any())
        {
            var defaultBoard = new Board { Name = "Default Board", Description = "My first board", IsMain = true };
            boards.Add(defaultBoard);
            tasks.Add(new TaskItem { Title = "Изучить Blazor", Description = "Создать простое приложение", Status = KanbanStatus.InProgress, BoardId = defaultBoard.Id });
            tasks.Add(new TaskItem { Title = "Настроить CI/CD", Description = "Настроить пайплайн для сборки", Status = KanbanStatus.Todo, BoardId = defaultBoard.Id });
            tasks.Add(new TaskItem { Title = "Написать документацию", Description = "Описать архитектуру", Status = KanbanStatus.Done, BoardId = defaultBoard.Id });
            return true;
        }

        if (boards.GetMain() == null)
        {
            var fallback = boards.GetActive().FirstOrDefault() ?? boards.GetAll().FirstOrDefault();
            if (fallback != null)
            {
                boards.SetMainBoard(fallback.Id);
            }
        }

        return false;
    }

    private void EnsureMainBoard()
    {
        if (_boards.GetMain() != null)
        {
            return;
        }

        var fallback = _boards.GetActive().FirstOrDefault() ?? _boards.GetAll().FirstOrDefault();
        if (fallback != null)
        {
            _boards.SetMainBoard(fallback.Id);
        }
    }

    private void EnsureInitialized()
    {
        _ = _initialized.Value;
    }
}
