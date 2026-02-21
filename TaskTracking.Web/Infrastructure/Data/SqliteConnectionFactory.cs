using Microsoft.Data.Sqlite;

namespace TaskTracking.Web.Infrastructure.Data;

public class SqliteConnectionFactory(string connectionString)
{
    private readonly string _connectionString = connectionString;

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public void InitializeDatabase()
    {
        using var connection = CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Boards (
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Description TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Tasks (
                Id TEXT PRIMARY KEY,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                BoardId TEXT NOT NULL,
                AssigneeId TEXT,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                AuthKey TEXT NOT NULL UNIQUE,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Comments (
                Id TEXT PRIMARY KEY,
                TaskId TEXT NOT NULL,
                AuthorId TEXT NOT NULL,
                Text TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS IX_Tasks_BoardId ON Tasks(BoardId);
            CREATE INDEX IF NOT EXISTS IX_Comments_TaskId ON Comments(TaskId);
            CREATE INDEX IF NOT EXISTS IX_Users_AuthKey ON Users(AuthKey);
        ";
        command.ExecuteNonQuery();
    }
}
