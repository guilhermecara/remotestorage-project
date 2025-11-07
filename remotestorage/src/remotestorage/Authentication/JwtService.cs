namespace remotestorage.AuthenticationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

public class JwtService
{
    private readonly ProtectedSessionStorage _browserStorage;
    private const string TOKEN_KEY = "api-jtw-token";
    
    public JwtService(ProtectedSessionStorage browserStorage)
    {
        _browserStorage = browserStorage;   
    }

    public async Task SaveTokenAsync(string token)
    {
        await _browserStorage.SetAsync(TOKEN_KEY, token);
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _browserStorage.GetAsync<string>(TOKEN_KEY);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearTokenAsync()
    {
        await _browserStorage.DeleteAsync(TOKEN_KEY);
    }
}