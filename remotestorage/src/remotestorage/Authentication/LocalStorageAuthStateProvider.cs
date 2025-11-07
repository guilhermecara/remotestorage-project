// remotestorage/Authentication/LocalStorageAuthStateProvider.cs
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
namespace remotestorage.AuthenticationService;
using System.IdentityModel.Tokens.Jwt;

public class LocalStorageAuthStateProvider : AuthenticationStateProvider
{
    private readonly JwtService _jwt;

    public LocalStorageAuthStateProvider(JwtService jwt)
    {
        _jwt = jwt;
    }
    
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _jwt.GetTokenAsync();
        var identity = string.IsNullOrEmpty(token)
            ? new ClaimsIdentity()
            : new ClaimsIdentity(ParseClaims(token), "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
    
    private IEnumerable<Claim> ParseClaims (string jwt)
    {
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(jwt);
        return token.Claims;
    }
}