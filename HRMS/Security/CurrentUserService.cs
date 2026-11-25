using HRMS.Service.Interface;
using Microsoft.AspNetCore.Components.Authorization;

namespace HRMS.Security
{
    public class CurrentUserService(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserService
    {
        public async Task<string> GetUsernameAsync()
        {
            var state = await authenticationStateProvider.GetAuthenticationStateAsync();
            var name = state.User.Identity?.Name;

            return string.IsNullOrWhiteSpace(name) ? "System" : name;
        }
    }
}