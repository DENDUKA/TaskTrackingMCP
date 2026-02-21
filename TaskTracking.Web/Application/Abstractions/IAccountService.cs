using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Application.Abstractions;

public interface IAccountService
{
    List<User> GetUsers();
    User? GetUserByAuthKey(Guid authKey);
    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(Guid id);
}
