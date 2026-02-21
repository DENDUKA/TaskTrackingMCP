using TaskTracking.Web.Application.Abstractions;
using TaskTracking.Web.Domain.Entities;
using TaskTracking.Web.Infrastructure.Repositories;

namespace TaskTracking.Web.Application.Services;

public class AccountService(IUserRepository users) : IAccountService
{
    private readonly IUserRepository _users = users;

    public List<User> GetUsers()
    {
        return _users.GetAll();
    }

    public User? GetUserByAuthKey(Guid authKey)
    {
        return _users.GetByAuthKey(authKey);
    }

    public void AddUser(User user)
    {
        _users.Add(user);
    }

    public void UpdateUser(User user)
    {
        _users.Update(user);
    }

    public void DeleteUser(Guid id)
    {
        _users.Delete(id);
    }
}
