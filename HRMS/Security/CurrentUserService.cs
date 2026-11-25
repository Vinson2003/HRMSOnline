using HRMS.Service.Interface;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace HRMS.Security
{
    public class CurrentUserService(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserService
    {
        public async Task<string?> GetUsernameAsync()
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            var name = state.User.Identity?.Name;

            return string.IsNullOrWhiteSpace(name) ? "System" : name;
        }

        public async Task<int?> GetUserIdAsync()
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();

            // Gunakan operator null-conditional (?.) atau method FindFirstValue
            var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(userId, out var id) ? id : null;
        }
    }
}