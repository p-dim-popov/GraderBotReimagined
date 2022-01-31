using Api.Helpers.Authorization;
using Api.Models;
using Data.DbContexts;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public interface IUserService
{
    Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model);

    Task<User?> FindByEmailAsync(string email);

    Task<User?> FindByIdAsync(string id);
    Task<User?> FindByIdAsync(Guid id);

    Task RegisterAsync(RegisterRequest model);
}

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IJwtUtils _jwtUtils;

    public UserService(
        AppDbContext context,
        IJwtUtils jwtUtils
        )
    {
        _context = context;
        _jwtUtils = jwtUtils;
    }

    public async Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model)
    {
        var user = await _context.Users
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == model.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            throw new BadHttpRequestException("Username or password is incorrect");

        var response = new AuthenticateResponse
        {
            Email = user.Email,
            Id = user.Id,
            Token = _jwtUtils.GenerateToken(user),
        };
        return response;
    }

    public Task<User?> FindByEmailAsync(string email) => _context.Users.FirstOrDefaultAsync(x => x.Email == email);

    public Task<User?> FindByIdAsync(string id) => FindByIdAsync(Guid.Parse(id));

    public Task<User?> FindByIdAsync(Guid id) => _context.Users.FirstOrDefaultAsync(x => x.Id == id);

    public async Task RegisterAsync(RegisterRequest model)
    {
        if (await _context.Users.AnyAsync(x => x.Email == model.Email))
            throw new BadHttpRequestException("Email '" + model.Email + "' is already taken");

        var user = new User
        {
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password)
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}
