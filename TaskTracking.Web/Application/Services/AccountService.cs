using TaskTracking.Web.Models;
using TaskTracking.Web.Repositories;

namespace TaskTracking.Web.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _users;

        public AccountService(IUserRepository users)
        {
            _users = users;
            if (!_users.GetAll().Any())
            {
                _users.Add(new User { Name = "Иван Иванов", Email = "ivan@example.com", AuthKey = Guid.Parse("11111111-1111-1111-1111-111111111111") });
                _users.Add(new User { Name = "Петр Петров", Email = "petr@example.com", AuthKey = Guid.Parse("22222222-2222-2222-2222-222222222222") });
            }
        }

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
}
