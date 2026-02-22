using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Infrastructure.Data;

namespace TaskTracking.Web.Infrastructure.Repositories;

public class SqliteBoardRepository(SqliteConnectionFactory connectionFactory) : IBoardRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;

    public List<Board> GetAll()
    {
        var boards = new List<Board>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, IsActive, IsMain FROM Boards";

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
        command.CommandText = "SELECT Id, Name, Description, IsActive, IsMain FROM Boards WHERE IsActive = 1";

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
        command.CommandText = "SELECT Id, Name, Description, IsActive, IsMain FROM Boards WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapBoard(reader);
        }
        return null;
    }

    public Board? GetMain()
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Description, IsActive, IsMain FROM Boards WHERE IsMain = 1 LIMIT 1";

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
            INSERT INTO Boards (Id, Name, Description, IsActive, IsMain)
            VALUES (@Id, @Name, @Description, @IsActive, @IsMain)";
        command.Parameters.AddWithValue("@Id", board.Id.ToString());
        command.Parameters.AddWithValue("@Name", board.Name);
        command.Parameters.AddWithValue("@Description", board.Description);
        command.Parameters.AddWithValue("@IsActive", board.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@IsMain", board.IsMain ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void Update(Board board)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Boards
            SET Name = @Name, Description = @Description, IsActive = @IsActive, IsMain = @IsMain
            WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", board.Id.ToString());
        command.Parameters.AddWithValue("@Name", board.Name);
        command.Parameters.AddWithValue("@Description", board.Description);
        command.Parameters.AddWithValue("@IsActive", board.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@IsMain", board.IsMain ? 1 : 0);
        command.ExecuteNonQuery();
    }

    public void SetMainBoard(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        using var transaction = connection.BeginTransaction();

        var resetCommand = connection.CreateCommand();
        resetCommand.Transaction = transaction;
        resetCommand.CommandText = "UPDATE Boards SET IsMain = 0 WHERE IsMain = 1";
        resetCommand.ExecuteNonQuery();

        var setCommand = connection.CreateCommand();
        setCommand.Transaction = transaction;
        setCommand.CommandText = "UPDATE Boards SET IsMain = 1 WHERE Id = @Id";
        setCommand.Parameters.AddWithValue("@Id", id.ToString());
        setCommand.ExecuteNonQuery();

        transaction.Commit();
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
            IsActive = reader.GetInt64(3) == 1,
            IsMain = reader.GetInt64(4) == 1
        };
    }
}
