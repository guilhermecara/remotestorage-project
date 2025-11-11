using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using remotestorage.Components;
using remotestorage.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURATION ===
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var apiBaseUrl = Environment.GetEnvironmentVariable("API_DOCKER_URL")
                 ?? builder.Configuration["API_BASE_URL"];

// === SERVICES ===
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<UIState>();
builder.Services.AddHttpContextAccessor();

// === AUTHENTICATION ===
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Cookies["auth_token"];
                if (!string.IsNullOrEmpty(token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddRouting();
builder.Services.AddCascadingAuthenticationState();

// === BLAZOR SETUP ===
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRazorPages();

// === API CLIENT ===
builder.Services.AddHttpClient("API", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

var app = builder.Build();

// === MIDDLEWARE PIPELINE ===
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    switch (response.StatusCode)
    {
        case 401:
            response.Redirect("/login");
            break;
        case 404:
            response.Redirect("/NotFound");
            break;
    }
});

// === BLAZOR MAP ===
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();



app.Run();
