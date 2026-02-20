using TaskTracking.Web.Models;

namespace TaskTracking.Web.Services;

public interface ICurrentUserService
{
    User? CurrentUser { get; }
    event Action? OnCurrentUserChanged;
    Task<bool> Login(Guid authKey);
    Task Logout();
    Task Initialize();
}
