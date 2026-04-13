using Microsoft.AspNetCore.Identity;

namespace ChessTournaments.Identity.Database.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }
}
