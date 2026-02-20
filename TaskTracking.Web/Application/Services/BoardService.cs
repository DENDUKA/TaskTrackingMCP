using TaskTracking.Web.Models;
using TaskTracking.Web.Repositories;

namespace TaskTracking.Web.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _boards;
        private readonly ITaskRepository _tasks;

        public BoardService(IBoardRepository boards, ITaskRepository tasks)
        {
            _boards = boards;
            _tasks = tasks;
            if (!_boards.GetAll().Any())
            {
                var defaultBoard = new Board { Name = "Default Board", Description = "My first board" };
                _boards.Add(defaultBoard);
                _tasks.Add(new TaskItem { Title = "Изучить Blazor", Description = "Создать простое приложение", Status = KanbanStatus.InProgress, BoardId = defaultBoard.Id });
                _tasks.Add(new TaskItem { Title = "Настроить CI/CD", Description = "Настроить пайплайн для сборки", Status = KanbanStatus.Todo, BoardId = defaultBoard.Id });
                _tasks.Add(new TaskItem { Title = "Написать документацию", Description = "Описать архитектуру", Status = KanbanStatus.Done, BoardId = defaultBoard.Id });
            }
        }

        public List<Board> GetAllBoards()
        {
            return _boards.GetAll();
        }

        public List<Board> GetBoards()
        {
            return _boards.GetActive();
        }

        public Board? GetBoard(Guid id)
        {
            return _boards.GetById(id);
        }

        public void AddBoard(Board board)
        {
            _boards.Add(board);
        }

        public void UpdateBoard(Board updatedBoard)
        {
            _boards.Update(updatedBoard);
        }

        public void DeleteBoard(Guid id)
        {
            _tasks.DeleteByBoardId(id);
            _boards.Delete(id);
        }
    }
}
