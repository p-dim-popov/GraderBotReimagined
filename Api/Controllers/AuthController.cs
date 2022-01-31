using Api.Models;
using Api.Models.Auth;
using Api.Services.Abstractions;
using Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<User> Profile()
    {
        var user = await _userService.FindByIdAsync(User.Identity!.Name!);
        return user!;
    }

    [HttpGet("is-admin")]
    [Authorize(Roles = "Administrator")]
    public IActionResult AdminCheck() => Ok();
}
