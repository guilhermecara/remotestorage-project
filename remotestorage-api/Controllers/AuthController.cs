using Microsoft.AspNetCore.Mvc;
using Npgsql;
using remotestorage_api.Models;
using remotestorage_api.Services;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{   

    private readonly AuthService _authService;
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }   

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        
        User? createdUser = await _authService.Register(request);

        if (createdUser == null)
        {
            return Conflict(new {message = "Invalid username"});
        }

        return Ok(
            new
            {
                Id = createdUser.Id,
                Username = createdUser.Username,
                CreatedAt = createdUser.CreatedAt
            }
        );
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login ([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var token = await _authService.Login(request);
        if (token == null)
            return Unauthorized(new { message = "Invalid username or password" });

        Response.Cookies.Append("auth_token", token, new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = false,
            Expires = DateTime.UtcNow.AddMinutes(60),
            Path = "/"
        });

        return Ok(new { token });       
    }

    [HttpPost("logout")] // POST to prevent CSRF via GET
    public IActionResult Logout()
    {
        Response.Cookies.Delete("auth_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok(new { Message = "Logged out successfully" });
    }
}