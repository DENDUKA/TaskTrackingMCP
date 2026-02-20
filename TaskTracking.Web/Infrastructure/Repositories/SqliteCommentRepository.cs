using TaskTracking.Web.Infrastructure.Data;
using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class SqliteCommentRepository : ICommentRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteCommentRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<Comment> GetByTaskId(Guid taskId)
    {
        var comments = new List<Comment>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TaskId, AuthorId, Text, CreatedAt FROM Comments WHERE TaskId = @TaskId ORDER BY CreatedAt";
        command.Parameters.AddWithValue("@TaskId", taskId.ToString());

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            comments.Add(MapComment(reader));
        }
        return comments;
    }

    public Comment? GetById(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, TaskId, AuthorId, Text, CreatedAt FROM Comments WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapComment(reader);
        }
        return null;
    }

    public void Add(Comment comment)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Comments (Id, TaskId, AuthorId, Text, CreatedAt)
            VALUES (@Id, @TaskId, @AuthorId, @Text, @CreatedAt)";
        command.Parameters.AddWithValue("@Id", comment.Id.ToString());
        command.Parameters.AddWithValue("@TaskId", comment.TaskId.ToString());
        command.Parameters.AddWithValue("@AuthorId", comment.AuthorId.ToString());
        command.Parameters.AddWithValue("@Text", comment.Text);
        command.Parameters.AddWithValue("@CreatedAt", comment.CreatedAt.ToString("O"));
        command.ExecuteNonQuery();
    }

    public void Delete(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Comments WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());
        command.ExecuteNonQuery();
    }

    public void DeleteByTaskId(Guid taskId)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Comments WHERE TaskId = @TaskId";
        command.Parameters.AddWithValue("@TaskId", taskId.ToString());
        command.ExecuteNonQuery();
    }

    private static Comment MapComment(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new Comment
        {
            Id = Guid.Parse(reader.GetString(0)),
            TaskId = Guid.Parse(reader.GetString(1)),
            AuthorId = Guid.Parse(reader.GetString(2)),
            Text = reader.GetString(3),
            CreatedAt = DateTime.Parse(reader.GetString(4))
        };
    }
}
