using System;
using remotestorage_api.Models;
using Npgsql;
using Microsoft.AspNetCore.Http;

namespace remotestorage_api.Services;

public static class ImageService
{
    static string imageDir = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? Environment.GetEnvironmentVariable("FALLBACK_IMAGE_DIR");

    public static async Task<List<Image>> GetAll()
    {
        List<Image> imageData = new List<Image>();

        try
        {
            await using var command = DatabaseService.CreateQuery("SELECT id, name, url FROM images");
            await using var reader = await command.ExecuteReaderAsync();
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
        }
        catch (Exception e)
        {
            Console.WriteLine("An exception has been found while loading the images: " + e);
            Console.WriteLine("The probable cause is related to the databse connection.");
        }

        

        return imageData;
    }

    public static async Task<Image?> Get(int ImageId)
    {
        NpgsqlCommand command = DatabaseService.CreateQuery("SELECT id, name, url FROM images WHERE id = @id");
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

    public static async Task<Image?> Add(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        Image uploadedImage = await FileService.UploadImage(file);
        if (uploadedImage == null)
            return null;

        var newImage = new Image
        {
            Name = uploadedImage.Name,
            Url = uploadedImage.Url
        };

        await using var command = DatabaseService.CreateQuery(
            "INSERT INTO images (name, url) VALUES (@name, @url) RETURNING id;"
        );
        command.Parameters.AddWithValue("name", newImage.Name);
        command.Parameters.AddWithValue("url", newImage.Url);

        var result = await command.ExecuteScalarAsync(); 
        if (result is int id)
        {
            newImage.Id = id;
            return newImage;
        }

        return null;
    }


    public static async Task Delete(int id)
    {
        if (Get(id) != null)
        {
            NpgsqlCommand command = DatabaseService.CreateQuery("DELETE FROM images WHERE id = @id");
            command.Parameters.AddWithValue("id", id);
            await command.ExecuteReaderAsync();

            Console.WriteLine("Image deleted successfully");
        }
    }

    public static void Update(Image image)
    {
    }

}