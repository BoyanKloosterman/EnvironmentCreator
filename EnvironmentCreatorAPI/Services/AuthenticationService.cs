using Microsoft.AspNetCore.Identity;

namespace EnvironmentCreatorAPI.Services
{
    public interface IAuthenticationService
    {
        Task<string> GetCurrentUserIdAsync();
    }

    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthenticationService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<IdentityUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<string> GetCurrentUserIdAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return null;
            }

            var identityUser = await _userManager.GetUserAsync(user);
            return identityUser?.Id;
        }
    }
}