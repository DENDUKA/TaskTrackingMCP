using TaskTracking.Web.Domain.Entities;

namespace TaskTracking.Web.Application.Abstractions;

public interface ICurrentUserService
{
    User? CurrentUser { get; }
    event Action? OnCurrentUserChanged;
    Task<bool> Login(Guid authKey);
    Task Logout();
    Task Initialize();
}
