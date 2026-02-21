using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Infrastructure.Repositories;

public interface IUserRepository
{
    List<User> GetAll();
    User? GetById(Guid id);
    User? GetByAuthKey(Guid authKey);
    void Add(User user);
    void Update(User user);
    void Delete(Guid id);
}