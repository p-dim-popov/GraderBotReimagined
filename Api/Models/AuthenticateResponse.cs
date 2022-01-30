namespace Api.Models;

public class AuthenticateResponse
{
    public Guid Id { get; set; }

    public string Email { get; set; }

    public string Token { get; set; }
}
