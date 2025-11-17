namespace remotestorage_api.Services;
using remotestorage_api.Models;
public class AuthService
{
    private readonly JwtService _jwtService;

    public AuthService(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    public async Task<string?> Login(LoginRequest request)
    {
        var user = await DatabaseService.FetchUser(request.Username);
        if (user == null) return null;

        if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
            return null;

        return _jwtService.GenerateToken(request.Username);
    }

    public async Task<User?> Register(RegisterRequest request)
    {
        if (await DatabaseService.UsernameExists(request.Username))
        {
            return null;
        }

        string passwordHash = PasswordService.HashPassword(request.Password);
        var user = await DatabaseService.CreateUser(request.Username, passwordHash); 

        return user;
    }



}
