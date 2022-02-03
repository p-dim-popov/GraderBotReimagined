using Api.Models.Auth;
using Data.Models;

namespace Api.Services.Abstractions;

public interface IAuthService
{
    Task<LoginResponse> AuthenticateAsync(LoginRequest model);

    IQueryable<User> FindByIdAsync(string id);
    IQueryable<User> FindByIdAsync(Guid id);

    Task RegisterAsync(RegisterRequest model);
}
