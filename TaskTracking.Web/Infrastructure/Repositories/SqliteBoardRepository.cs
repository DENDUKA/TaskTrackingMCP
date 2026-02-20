using TaskTracking.Web.Infrastructure.Data;
using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class SqliteBoardRepository : IBoardRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteBoardRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<Board> GetAll()
    {
        var boards = new List<Board>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, IsActive FROM Boards";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            boards.Add(MapBoard(reader));
        }
        return boards;
    }

    public List<Board> GetActive()
    {
        var boards = new List<Board>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, IsActive FROM Boards WHERE IsActive = 1";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            boards.Add(MapBoard(reader));
        }
        return boards;
    }

    public Board? GetById(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, IsActive FROM Boards WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBoard(reader);
        }
        return null;
    }

    public void Add(Board board)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Boards (Id, Name, Description, IsActive)
            VALUES (@Id, @Name, @Description, @IsActive)";
        command.Parameters.AddWithValue("@Id", board.Id.ToString());
        command.Parameters.AddWithValue("@Name", board.Name);
        command.Parameters.AddWithValue("@Description", board.Description);
        command.Parameters.AddWithValue("@IsActive", board.IsActive ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void Update(Board board)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Boards
            SET Name = @Name, Description = @Description, IsActive = @IsActive
            WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", board.Id.ToString());
        command.Parameters.AddWithValue("@Name", board.Name);
        command.Parameters.AddWithValue("@Description", board.Description);
        command.Parameters.AddWithValue("@IsActive", board.IsActive ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void Delete(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Boards WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());
        command.ExecuteNonQuery();
    }

    private static Board MapBoard(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new Board
        {
            Id = Guid.Parse(reader.GetString(0)),
            Name = reader.GetString(1),
            Description = reader.GetString(2),
            IsActive = reader.GetInt64(3) == 1
        };
    }
}
