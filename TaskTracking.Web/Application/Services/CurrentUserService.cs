using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using TaskTracking.Web.Models;

namespace TaskTracking.Web.Services
{
    public class CurrentUserService(IAccountService accountService, ProtectedLocalStorage localStorage) : ICurrentUserService
    {
        private readonly IAccountService _accountService = accountService;
        private readonly ProtectedLocalStorage _localStorage = localStorage;
        private User? _currentUser;
        private bool _initialized;

        private const string AuthKeyStorageKey = "authKey";

        public User? CurrentUser => _currentUser;

        public event Action? OnCurrentUserChanged;

        public async Task Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            try
            {
                var result = await _localStorage.GetAsync<Guid>(AuthKeyStorageKey);
                if (result.Success && result.Value != Guid.Empty)
                {
                    var user = _accountService.GetUserByAuthKey(result.Value);
                    if (user != null)
                    {
                        _currentUser = user;
                        OnCurrentUserChanged?.Invoke();
                    }
                }
            }
            catch
            {
                // Storage may not be available during prerender
            }
        }

        public async Task<bool> Login(Guid authKey)
        {
            var user = _accountService.GetUserByAuthKey(authKey);
            if (user != null)
            {
                _currentUser = user;
                try
                {
                    await _localStorage.SetAsync(AuthKeyStorageKey, authKey);
                }
                catch
                {
                    // Storage may not be available during prerender
                }
                OnCurrentUserChanged?.Invoke();
                return true;
            }
            return false;
        }

        public async Task Logout()
        {
            _currentUser = null;
            try
            {
                await _localStorage.DeleteAsync(AuthKeyStorageKey);
            }
            catch
            {
                // Storage may not be available during prerender
            }
            OnCurrentUserChanged?.Invoke();
        }
    }
}
