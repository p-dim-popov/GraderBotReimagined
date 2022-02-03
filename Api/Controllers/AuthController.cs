using Api.Helpers.Authorization;
using Api.Models.Auth;
using Api.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _userService;

    public AuthController(IAuthService userService)
    {
        _userService = userService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest model)
    {
        var response = await _userService.AuthenticateAsync(model);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest model)
    {
        await _userService.RegisterAsync(model);
        return Ok(new { message = "Registration successful" });
    }

    [HttpGet("profile")]
    public async Task<dynamic> Profile()
    {
        var user = await _userService.FindByIdAsync(User.GetId())
            .Include(x => x.Roles)
            .ThenInclude(x => x.Role)
            .Select(x => new { x.Id, x.Email, Roles = x.Roles.Select(y => y.Role.Name)  })
            .FirstOrDefaultAsync();
        return user!;
    }

    [HttpGet("is-admin")]
    [Authorize(Roles = "Administrator")]
    public IActionResult AdminCheck() => Ok();
}
