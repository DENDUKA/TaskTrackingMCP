using TaskTracking.Web.Infrastructure.Data;
using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class SqliteUserRepository : IUserRepository
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteUserRepository(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public List<User> GetAll()
    {
        var users = new List<User>();
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, AuthKey, Name, Email FROM Users";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            users.Add(MapUser(reader));
        }
        return users;
    }

    public User? GetById(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, AuthKey, Name, Email FROM Users WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }
        return null;
    }

    public User? GetByAuthKey(Guid authKey)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, AuthKey, Name, Email FROM Users WHERE AuthKey = @AuthKey";
        command.Parameters.AddWithValue("@AuthKey", authKey.ToString());

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return MapUser(reader);
        }
        return null;
    }

    public void Add(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Users (Id, AuthKey, Name, Email)
            VALUES (@Id, @AuthKey, @Name, @Email)";
        command.Parameters.AddWithValue("@Id", user.Id.ToString());
        command.Parameters.AddWithValue("@AuthKey", user.AuthKey.ToString());
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.ExecuteNonQuery();
    }

    public void Update(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Users
            SET AuthKey = @AuthKey, Name = @Name, Email = @Email
            WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", user.Id.ToString());
        command.Parameters.AddWithValue("@AuthKey", user.AuthKey.ToString());
        command.Parameters.AddWithValue("@Name", user.Name);
        command.Parameters.AddWithValue("@Email", user.Email);
        command.ExecuteNonQuery();
    }

    public void Delete(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());
        command.ExecuteNonQuery();
    }

    private static User MapUser(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        return new User
        {
            Id = Guid.Parse(reader.GetString(0)),
            AuthKey = Guid.Parse(reader.GetString(1)),
            Name = reader.GetString(2),
            Email = reader.GetString(3)
        };
    }
}
