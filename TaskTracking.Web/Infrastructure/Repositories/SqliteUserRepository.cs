using System.Collections.Concurrent;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Infrastructure.Data;

namespace TaskTracking.Web.Infrastructure.Repositories;

public class SqliteUserRepository(SqliteConnectionFactory connectionFactory) : IUserRepository
{
    private readonly SqliteConnectionFactory _connectionFactory = connectionFactory;
    private readonly Lock _cacheLock = new();
    private ConcurrentDictionary<Guid, User>? _usersById;
    private ConcurrentDictionary<Guid, User>? _usersByAuthKey;
    private bool _isCacheInitialized;

    public List<User> GetAll()
    {
        var cache = EnsureCacheLoaded();
        return cache.UsersById.Values.ToList();
    }

    public User? GetById(Guid id)
    {
        var cache = EnsureCacheLoaded();
        return cache.UsersById.GetValueOrDefault(id);
    }

    public User? GetByAuthKey(Guid authKey)
    {
        var cache = EnsureCacheLoaded();
        return cache.UsersByAuthKey.GetValueOrDefault(authKey);
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

        if (_isCacheInitialized)
        {
            var cache = EnsureCacheLoaded();
            cache.UsersById[user.Id] = user;
            cache.UsersByAuthKey[user.AuthKey] = user;
        }
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

        if (_isCacheInitialized)
        {
            var cache = EnsureCacheLoaded();
            if (cache.UsersById.TryGetValue(user.Id, out var existing) && existing.AuthKey != user.AuthKey)
            {
                cache.UsersByAuthKey.TryRemove(existing.AuthKey, out _);
            }
            cache.UsersById[user.Id] = user;
            cache.UsersByAuthKey[user.AuthKey] = user;
        }
    }

    public void Delete(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Users WHERE Id = @Id";
        command.Parameters.AddWithValue("@Id", id.ToString());
        command.ExecuteNonQuery();

        if (_isCacheInitialized)
        {
            var cache = EnsureCacheLoaded();
            if (cache.UsersById.TryRemove(id, out var removed))
            {
                cache.UsersByAuthKey.TryRemove(removed.AuthKey, out _);
            }
        }
    }

    private UserCache EnsureCacheLoaded()
    {
        if (_isCacheInitialized && _usersById != null && _usersByAuthKey != null)
        {
            return new UserCache(_usersById, _usersByAuthKey);
        }

        lock (_cacheLock)
        {
            if (_isCacheInitialized && _usersById != null && _usersByAuthKey != null)
            {
                return new UserCache(_usersById, _usersByAuthKey);
            }

            var users = LoadUsersFromDatabase();
            _usersById = new ConcurrentDictionary<Guid, User>(users.ToDictionary(user => user.Id));
            _usersByAuthKey = new ConcurrentDictionary<Guid, User>(users.ToDictionary(user => user.AuthKey));
            _isCacheInitialized = true;
            return new UserCache(_usersById, _usersByAuthKey);
        }
    }

    private readonly record struct UserCache(ConcurrentDictionary<Guid, User> UsersById, ConcurrentDictionary<Guid, User> UsersByAuthKey);

    private List<User> LoadUsersFromDatabase()
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
