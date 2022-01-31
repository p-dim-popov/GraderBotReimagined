using Api.Models;
using Api.Models.Auth;
using Data.Models;

namespace Api.Services.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest model);

    Task<User?> FindByEmailAsync(string email);

    Task<User?> FindByIdAsync(string id);
    Task<User?> FindByIdAsync(Guid id);

    Task RegisterAsync(RegisterRequest model);
}
