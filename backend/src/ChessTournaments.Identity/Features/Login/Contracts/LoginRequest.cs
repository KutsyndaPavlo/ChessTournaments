using System.ComponentModel.DataAnnotations;

namespace ChessTournaments.Identity.Features.Login.Contracts;

public class LoginRequest
{
    [Required(ErrorMessage = "Please enter a valid Username")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter a valid Password")]
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
