using Npgsql;
using remotestorage_api.Models;

namespace remotestorage_api.Services;

public static class DatabaseService
{
    // Database connection parameters from environment variables or default values for development
    static private string connectionUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "guilhermeuser"; // Default user for development
    static private string connectionPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "LIbolo0$"; // Default password for development
    static private string connectionDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "database"; // Default database for development
    static private string connectionHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"; // Default host for development
    static private int connectionPort = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 6060; // Default port for development
    static private NpgsqlDataSource dataSource = NpgsqlDataSource.Create(GetConnectionString());

    private static string GetConnectionString()
    {
        return $"Host={connectionHost};Port={connectionPort};Username={connectionUser};Password={connectionPassword};Database={connectionDb}";
    }

    public static NpgsqlCommand CreateQuery(string query)
    {
        var command = dataSource.CreateCommand(query);
        return command;
    }

    public static async Task<NpgsqlDataReader> ExecuteQuery(string query)
    {
        var command = CreateQuery(query);
        return await command.ExecuteReaderAsync();
    }

    public static async Task<int> ExecuteNonQuery(string query)
    {
        var command = CreateQuery(query);
        return await command.ExecuteNonQueryAsync(); // For INSERT/UPDATE/DELETE
    }

    public static async Task<bool> UsernameExists(string username)
    {
        var command = dataSource.CreateCommand("SELECT COUNT(*) FROM users WHERE username = $1");
        command.Parameters.AddWithValue(username);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    public static async Task<User?> CreateUser(string username, string passwordHash)
    {
        var command = dataSource.CreateCommand(
            @"INSERT INTO users (username, password_hash, created_at, updated_at) 
                VALUES ($1, $2, NOW(), NOW()) 
                RETURNING id, username, password_hash, created_at, updated_at"
        );

        command.Parameters.AddWithValue(username);
        command.Parameters.AddWithValue(passwordHash);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetGuid(0),             // Column 0: id
                Username = reader.GetString(1),     // Column 1: username
                PasswordHash = reader.GetString(2), // Column 2: password_hash
                CreatedAt = reader.GetDateTime(3),  // Column 3: created_at
                UpdatedAt = reader.GetDateTime(4)   // Column 4: updated_at
            };
        }

        return null;
    }
    
    public static async Task<User?> FetchUser (string username)
    {
        var command = dataSource.CreateCommand("SELECT id, username, password_hash, created_at, updated_at FROM users WHERE username = $1");
        command.Parameters.AddWithValue(username);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new User
            {
                Id = reader.GetGuid(0),             // Column 0: id
                Username = reader.GetString(1),     // Column 1: username
                PasswordHash = reader.GetString(2), // Column 2: password_hash
                CreatedAt = reader.GetDateTime(3),  // Column 3: created_at
                UpdatedAt = reader.GetDateTime(4)   // Column 4: updated_at
            };
        }
        return null;
    }   

    public static async Task<Image?> FetchImageFromIdSecured (string userId, string imageName)
    {
        await using var command = CreateQuery(
            "SELECT id, name, user_id, path FROM images WHERE user_id = @userId AND name = @imageName;"
        );

        command.Parameters.AddWithValue("userId", Guid.Parse(userId));
        command.Parameters.AddWithValue("imageName", imageName);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        var id = reader.GetInt32(0);
        var name = reader.GetString(1);
        var ownerId = reader.GetGuid(2).ToString();
        var path = reader.GetString(3);

        if (ownerId != userId)
            return null;

        return new Image
        {
            Id = id,
            Name = name,
            Path = path
        };
    }

}