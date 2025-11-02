using remotestorage_api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();

// CORS for frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:8080",              // Local frontend
            "http://localhost:5184",              // Local alternate port
            "http://frontend:8080")
          .AllowAnyHeader()
          .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

app.UseCors("AllowFrontend");

app.UseAuthorization();
app.MapControllers();

app.Run("http://0.0.0.0:5050");  // Listen on all interfaces