using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services
{
    public class CustomProfileService(UserManager<ApplicationUser> userManager) : IProfileService
    {
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await userManager.GetUserAsync(context.Subject) ?? throw new InvalidOperationException("user not found");
            
            var existingClaims = await userManager.GetClaimsAsync(user) ?? throw new InvalidOperationException("exisitng claims not found");;
            
            if (string.IsNullOrWhiteSpace(user.UserName)) throw new InvalidOperationException("UserName cannot be null.");

            var claims = new List<Claim>
            {
                new("username", user.UserName)
            };

            context.IssuedClaims.AddRange(claims);
            var nameClaim = existingClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Name);

            if (nameClaim != null) context.IssuedClaims.Add(nameClaim);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}