using System.ComponentModel.DataAnnotations;

namespace Api.Models.Auth;

public class RegisterRequest
{
    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}
