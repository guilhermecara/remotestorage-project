using System;
using remotestorage_api.Models;
using Npgsql;
using Microsoft.AspNetCore.Http;

namespace remotestorage_api.Services;

public static class ImageService
{
    // Database connection parameters from environment variables or default values for development
    static string connectionUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "guilhermeuser"; // Default user for development
    static string connectionPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "LIbolo0$"; // Default password for development
    static string connectionDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "database"; // Default database for development
    static string connectionHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"; // Default host for development
    static int connectionPort = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 6060; // Default port for development
    static string imageDir = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? Environment.GetEnvironmentVariable("FALLBACK_IMAGE_DIR");

    private static string GetConnectionString() =>
        $"Host={connectionHost};Port={connectionPort};Username={connectionUser};Password={connectionPassword};Database={connectionDb}";

    public static async Task<List<Image>> GetAll()
    {
        List<Image> imageData = new List<Image>();
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(GetConnectionString());
        await using NpgsqlCommand command = dataSource.CreateCommand("SELECT id, name, url FROM images");
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            string relativeUrl = reader.GetString(2).TrimStart('/');
            imageData.Add(new Image
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Url = "/api/file/" + relativeUrl
            });
        }

        return imageData;
    }

    public static async Task<Image?> Get(int ImageId)
    {
        await using var dataSource = NpgsqlDataSource.Create(GetConnectionString());
        await using var command = dataSource.CreateCommand("SELECT id, name, url FROM images WHERE id = @id");
        command.Parameters.AddWithValue("id", ImageId);

        await using var reader = await command.ExecuteReaderAsync();

        Image fetchImage = new Image();
        while (await reader.ReadAsync())
        {
            fetchImage.Id = reader.GetInt32(0);     // column 0 -> id
            fetchImage.Name = reader.GetString(1);   // column 1 -> name
            fetchImage.Url = reader.GetString(2);     // column 2 -> url

            return fetchImage;
        }

        return null;
    }

    public static void Add(Image image)
    {
    }

    public static async Task Delete(int id)
    {
        if (Get(id) != null)
        {
            await using var dataSource = NpgsqlDataSource.Create(GetConnectionString());
            await using var command = dataSource.CreateCommand("DELETE FROM images WHERE id = @id");
            command.Parameters.AddWithValue("id", id);
            await command.ExecuteReaderAsync();

            Console.WriteLine("Image deleted successfully");
        }
    }

    public static void Update(Image image)
    {
    }

}