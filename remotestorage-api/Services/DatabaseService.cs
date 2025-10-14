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

    private static string GetConnectionString() =>
        $"Host={connectionHost};Port={connectionPort};Username={connectionUser};Password={connectionPassword};Database={connectionDb}";


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

}