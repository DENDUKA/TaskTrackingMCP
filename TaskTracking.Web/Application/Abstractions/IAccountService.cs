using TaskTracking.Web.Models;

namespace TaskTracking.Web.Services;

public interface IAccountService
{
    List<User> GetUsers();
    User? GetUserByAuthKey(Guid authKey);
    void AddUser(User user);
    void UpdateUser(User user);
    void DeleteUser(Guid id);
}
