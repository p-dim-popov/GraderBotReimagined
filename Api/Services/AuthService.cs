using Api.Helpers.Authorization;
using Api.Models.Auth;
using Api.Services.Abstractions;
using Data.DbContexts;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtUtils _jwtUtils;

    public AuthService(
        AppDbContext context,
        IJwtUtils jwtUtils
    )
    {
        _context = context;
        _jwtUtils = jwtUtils;
    }

    public async Task<LoginResponse> AuthenticateAsync(LoginRequest model)
    {
        var user = await _context.Users
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == model.Email);

        if (user is null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
            throw new BadHttpRequestException("Email or password is incorrect");

        var response = new LoginResponse
        {
            Token = _jwtUtils.GenerateToken(user),
        };
        return response;
    }

    public IQueryable<User> FindByIdAsync(string id) => FindByIdAsync(Guid.Parse(id));

    public IQueryable<User> FindByIdAsync(Guid id) => _context.Users
        .Where(x => x.Id == id);

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
