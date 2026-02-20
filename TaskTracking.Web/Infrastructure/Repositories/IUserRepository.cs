using TaskTracking.Web.Models;

namespace TaskTracking.Web.Repositories;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(Guid id);
    User? GetByAuthKey(Guid authKey);
    void Add(User user);
    void Update(User user);
    void Delete(Guid id);
}