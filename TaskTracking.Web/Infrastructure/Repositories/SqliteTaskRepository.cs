using TaskTracking.Web.Infrastructure.Data;
using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class SqliteTaskRepository : ITaskRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteTaskRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<TaskItem> GetAll()
    {
        var tasks = new List<TaskItem>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, Status, BoardId, AssigneeId, CreatedAt FROM Tasks";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public TaskItem? GetById(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, Status, BoardId, AssigneeId, CreatedAt FROM Tasks WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapTask(reader);
        }
        return null;
    }

    public List<TaskItem> GetByBoardId(Guid boardId)
    {
        var tasks = new List<TaskItem>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, Status, BoardId, AssigneeId, CreatedAt FROM Tasks WHERE BoardId = @BoardId";
        command.Parameters.AddWithValue("@BoardId", boardId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tasks.Add(MapTask(reader));
        }
        return tasks;
    }

    public void Add(TaskItem task)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Tasks (Id, Title, Description, Status, BoardId, AssigneeId, CreatedAt)
            VALUES (@Id, @Title, @Description, @Status, @BoardId, @AssigneeId, @CreatedAt)";
        command.Parameters.AddWithValue("@Id", task.Id.ToString());
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", task.Description);
        command.Parameters.AddWithValue("@Status", (int)task.Status);
        command.Parameters.AddWithValue("@BoardId", task.BoardId.ToString());
        command.Parameters.AddWithValue("@AssigneeId", task.AssigneeId?.ToString() ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@CreatedAt", task.CreatedAt.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void Update(TaskItem task)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Tasks
            SET Title = @Title, Description = @Description, Status = @Status,
                BoardId = @BoardId, AssigneeId = @AssigneeId
            WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", task.Id.ToString());
        command.Parameters.AddWithValue("@Title", task.Title);
        command.Parameters.AddWithValue("@Description", task.Description);
        command.Parameters.AddWithValue("@Status", (int)task.Status);
        command.Parameters.AddWithValue("@BoardId", task.BoardId.ToString());
        command.Parameters.AddWithValue("@AssigneeId", task.AssigneeId?.ToString() ?? (object)DBNull.Value);
        command.ExecuteNonQuery();
    }

    public void Delete(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());
        command.ExecuteNonQuery();
    }

    public void DeleteByBoardId(Guid boardId)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Tasks WHERE BoardId = @BoardId";
        command.Parameters.AddWithValue("@BoardId", boardId.ToString());
        command.ExecuteNonQuery();
    }

    private static TaskItem MapTask(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new TaskItem
        {
            Id = Guid.Parse(reader.GetString(0)),
            Title = reader.GetString(1),
            Description = reader.GetString(2),
            Status = (KanbanStatus)reader.GetInt32(3),
            BoardId = Guid.Parse(reader.GetString(4)),
            AssigneeId = reader.IsDBNull(5) ? null : Guid.Parse(reader.GetString(5)),
            CreatedAt = DateTime.Parse(reader.GetString(6))
        };
    }
}
