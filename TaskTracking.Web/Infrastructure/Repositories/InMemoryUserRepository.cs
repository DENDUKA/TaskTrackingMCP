using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public List<User> GetAll() => _users;

    public User? GetById(Guid id) => _users.FirstOrDefault(u => u.Id == id);

    public User? GetByAuthKey(Guid authKey) => _users.FirstOrDefault(u => u.AuthKey == authKey);

    public void Add(User user) => _users.Add(user);

    public void Update(User user)
    {
        var existing = _users.FirstOrDefault(u => u.Id == user.Id);
        if (existing != null)
        {
            existing.Name = user.Name;
            existing.Email = user.Email;
        }
    }

    public void Delete(Guid id)
    {
        var u = _users.FirstOrDefault(x => x.Id == id);
        if (u != null) _users.Remove(u);
    }
}
