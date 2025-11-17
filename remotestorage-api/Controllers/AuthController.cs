using Microsoft.AspNetCore.Mvc;
using Npgsql;
using remotestorage_api.Models;
using remotestorage_api.Services;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{   

    private readonly JwtService _jwtService;
    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }   

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await DatabaseService.UsernameExists(request.Username))
        {
            return Conflict(new { message = "Username already exists" });
        }

        string passwordHash = PasswordService.HashPassword(request.Password);

        User createdUser = await DatabaseService.CreateUser(request.Username, passwordHash);

        if (createdUser == null)
        {
            return StatusCode(500, new { message = "Failed to create user!" });
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

        User? fetchedUser = await DatabaseService.FetchUser(request.Username);
        if (fetchedUser == null)
        {
            return Conflict(new { message = "Invalid username or password." });
        }

        if (!PasswordService.VerifyPassword(request.Password, fetchedUser.PasswordHash))
        {
            return Conflict(new { message = "Invalid password or password." });
        }

        try
        {
            string token = _jwtService.GenerateToken(request.Username);

            CookieOptions cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,                    // HTTPS only
                SameSite = SameSiteMode.Lax,     // Required for cross-site
                Expires = DateTime.UtcNow.AddMinutes(60),
                Path = "/"
            };

            Response.Cookies.Append("auth_token", token, cookieOptions);
            
            return Ok(new {token});
        }
            catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }       
    }

    [HttpPost("logout")] // POST to prevent CSRF via GET
    public IActionResult Logout()
    {
        // Delete the auth cookie
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