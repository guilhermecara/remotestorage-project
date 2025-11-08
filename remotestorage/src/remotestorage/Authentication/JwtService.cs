namespace remotestorage.AuthenticationService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http.Headers;

public class JwtService
{
    private readonly ProtectedSessionStorage _browserStorage;
    private readonly IHttpClientFactory _httpClientFactory;

    private const string TOKEN_KEY = "api-jtw-token";

    public JwtService(ProtectedSessionStorage browserStorage, IHttpClientFactory factory)
    {
        _browserStorage = browserStorage;
        _httpClientFactory = factory;
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

    public async Task<HttpClient> GetAuthorizedClientAsync()
    {
        var client = _httpClientFactory.CreateClient("API");
        var token = await GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public async Task DeleteTokenAsync()
    {
        await _browserStorage.DeleteAsync(TOKEN_KEY);
    }
}