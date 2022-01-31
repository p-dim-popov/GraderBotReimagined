using Api.Models;
using Data.Models;

namespace Api.Services.Abstractions;

public interface IAuthService
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);

    Task<User?> FindByEmailAsync(string email);

    Task<User?> FindByIdAsync(string id);
    Task<User?> FindByIdAsync(Guid id);

    Task RegisterAsync(RegisterRequest model);
}
