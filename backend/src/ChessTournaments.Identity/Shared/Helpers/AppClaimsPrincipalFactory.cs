using System.Security.Claims;
using ChessTournaments.Identity.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChessTournaments.Identity.Shared.Helpers;

public class AppClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor
)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(
        userManager,
        roleManager,
        optionsAccessor
    )
{
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);

        if (principal.Identity is ClaimsIdentity claimsIdentity)
        {
            //add custom claims
        }

        return principal;
    }
}
