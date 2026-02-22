using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Infrastructure.Repositories;

namespace TaskTracking.Web.Application.Services
{
    public class TaskService(ITaskRepository tasks, ICommentRepository comments, ITaskUpdateNotifier notifier) : ITaskService
    {
        private readonly ITaskRepository _tasks = tasks;
        private readonly ICommentRepository _comments = comments;
        private readonly ITaskUpdateNotifier _notifier = notifier;

        public List<TaskItem> GetTasks(Guid boardId)
        {
            return _tasks.GetByBoardId(boardId);
        }

        public List<TaskItem> GetTasks()
        {
            return _tasks.GetAll();
        }

        public void AddTask(TaskItem task)
        {
            _tasks.Add(task);
            _notifier.NotifyBoardTasksChanged(task.BoardId);
        }

        public void UpdateTaskStatus(Guid id, KanbanStatus newStatus)
        {
            var task = _tasks.GetById(id);
            if (task != null)
            {
                task.Status = newStatus;
                _tasks.Update(task);
                _notifier.NotifyBoardTasksChanged(task.BoardId);
            }
        }

        public void UpdateTask(TaskItem updatedTask)
        {
            var task = _tasks.GetById(updatedTask.Id);
            if (task != null)
            {
                var previousBoardId = task.BoardId;
                task.Title = updatedTask.Title;
                task.Description = updatedTask.Description;
                task.Status = updatedTask.Status;
                task.AssigneeId = updatedTask.AssigneeId;
                task.BoardId = updatedTask.BoardId;
                _tasks.Update(task);
                if (previousBoardId != task.BoardId)
                {
                    _notifier.NotifyBoardTasksChanged(previousBoardId);
                }
                _notifier.NotifyBoardTasksChanged(task.BoardId);
            }
        }

        public void DeleteTask(Guid id)
        {
            var task = _tasks.GetById(id);
            _comments.DeleteByTaskId(id);
            _tasks.Delete(id);
            if (task != null)
            {
                _notifier.NotifyBoardTasksChanged(task.BoardId);
            }
        }

        public List<Comment> GetComments(Guid taskId)
        {
            return _comments.GetByTaskId(taskId);
        }

        public void AddComment(Comment comment)
        {
            _comments.Add(comment);
        }

        public void DeleteComment(Guid commentId)
        {
            _comments.Delete(commentId);
        }
    }
}
