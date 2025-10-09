using Microsoft.AspNetCore.Http.HttpResults;
using remotestorage_api.Models;
using System;
using Npgsql;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace remotestorage_api.Services;

public static class ImageService
{
    // Database connection parameters from environment variables or default values for development
    static string connectionUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "guilhermeuser"; // Default user for development
    static string connectionPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "LIbolo0$"; // Default password for development
    static string connectionDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "database"; // Default database for development
    static string connectionHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"; // Default host for development
    static int connectionPort = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 6060; // Default port for development
    static string imageDir = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? "/home/guilherme/.remotestorage-images"; // Default path for development

    private static string GetConnectionString() =>
        $"Host={connectionHost};Port={connectionPort};Username={connectionUser};Password={connectionPassword};Database={connectionDb}";

    public static async Task<List<Image>> GetAll()
{
    List<Image> imageData = new List<Image>();
    await using var dataSource = NpgsqlDataSource.Create(GetConnectionString());
    await using var command = dataSource.CreateCommand("SELECT id, name, url FROM images");
    await using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        string relativeUrl = reader.GetString(2).TrimStart('/'); 
        string fullPath = Path.Combine(imageDir, relativeUrl); 

        // Log the path being checked
        Console.WriteLine($"Checking image file at path: {fullPath}");

        // Check if file exists
        if (!System.IO.File.Exists(fullPath))
        {
            Console.WriteLine($"Warning: Image file not found at path {fullPath}");
            continue; // Skip missing images
        }

        // Read image file as bytes
        byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        string mimeType = GetMimeType(Path.GetExtension(fullPath));
        string base64Image = $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";

        imageData.Add(new Image
        {
            Id = reader.GetInt32(0),
            Name = reader.GetString(1),
            Url = fullPath,
            ImageData = base64Image, 
        });
    }

    return imageData;
}

    private static string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream" // Fallback
        };
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