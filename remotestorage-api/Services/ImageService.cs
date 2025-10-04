using Microsoft.AspNetCore.Http.HttpResults;
using remotestorage_api.Models;
using System;
using Npgsql;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace remotestorage_api.Services;

public static class ImageService
{
    static string connectionUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "guilhermeuser";
    static string connectionPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "LIbolo0$";
    static string connectionDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "database";
    static string connectionHost = Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost";
    static int connectionPort = int.TryParse(Environment.GetEnvironmentVariable("DB_PORT"), out var port) ? port : 6060;
    
    private static string GetConnectionString() =>
        $"Host={connectionHost};Port={connectionPort};Username={connectionUser};Password={connectionPassword};Database={connectionDb}";

    static List<Image>? Images;

    public static async Task<List<Image>> GetAll()
    {
        Console.WriteLine("Testing the connection string:");
        Console.WriteLine(GetConnectionString());
        var images = new List<Image>();

        await using var dataSource = NpgsqlDataSource.Create(GetConnectionString());
        await using var command = dataSource.CreateCommand("SELECT id, name, url FROM images");
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            images.Add(new Image
            {
                Id = reader.GetInt32(0),       // column 0 -> id
                Name = reader.GetString(1),    // column 1 -> name
                Url = reader.GetString(2)      // column 2 -> url
            });
        }

        return images;
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