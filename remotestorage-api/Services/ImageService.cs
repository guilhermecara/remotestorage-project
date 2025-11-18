using System;
using remotestorage_api.Models;
using Npgsql;
using Microsoft.AspNetCore.Http;

namespace remotestorage_api.Services;

public static class ImageService
{
    public static async Task<List<Image>> GetAll()
    {
        List<Image> imageData = new List<Image>();

        try
        {
            await using var command = DatabaseService.CreateQuery("SELECT id, name, path FROM images");
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string relativeUrl = reader.GetString(2).TrimStart('/');
                imageData.Add(new Image
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Path = "/api/file/" + relativeUrl
                });
            }
        }
        catch (Exception e)
        {
        }

        

        return imageData;
    }

    public static async Task<Image?> Get(int ImageId)
    {
        NpgsqlCommand command = DatabaseService.CreateQuery("SELECT id, name, path FROM images WHERE id = @id");
        command.Parameters.AddWithValue("id", ImageId);

        await using var reader = await command.ExecuteReaderAsync();

        Image fetchImage = new Image();
        while (await reader.ReadAsync())
        {
            fetchImage.Id = reader.GetInt32(0);     // column 0 -> id
            fetchImage.Name = reader.GetString(1);   // column 1 -> name
            fetchImage.Path = reader.GetString(2);     // column 2 -> url

            return fetchImage;
        }

        return null;
    }

    public static async Task<Image?> Add(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        Image uploadedImage = await FileService.UploadImage(file,userId);
        if (uploadedImage == null)
            return null;
        
        await using var command = DatabaseService.CreateQuery(
            "INSERT INTO images (name, path, user_id) VALUES (@name, @path, @user_id) RETURNING id;"
        );
        command.Parameters.AddWithValue("name", uploadedImage.Name);
        command.Parameters.AddWithValue("path", uploadedImage.Path);
        command.Parameters.AddWithValue("user_id", Guid.Parse(userId));

        var result = await command.ExecuteScalarAsync(); 
        if (result is int id)
        {
            uploadedImage.Id = id;
            return uploadedImage;
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
        }
    }

    public static void Update(Image image)
    {
    }

}