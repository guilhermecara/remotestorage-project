using System;
using remotestorage_api.Models;
using Npgsql;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace remotestorage_api.Services;

public static class ImageService
{
    public static async Task<List<Image>> GetAll(string userId)
    {
        List<Image> imageData = new List<Image>();

        try
        {
            await using var command = DatabaseService.CreateQuery(
            "SELECT id, name, path FROM images WHERE user_id = @userId;"
            );

            command.Parameters.AddWithValue("userId", Guid.Parse(userId));
            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string imageName = reader.GetString(1); 
                string imagePath = reader.GetString(2);
                string imageFolder = Path.GetDirectoryName(imagePath)?.Replace("\\", "/") ?? "";

                string finalRelativePath = Path.Combine(imagePath, imageName);

                imageData.Add(new Image
                {
                    Id = -999999,
                    Name = imageName,
                    Path = finalRelativePath
                });
            }

        }
        catch (Exception e)
        {
        }

        return imageData;
    }

    public static async Task<Image?> Get(string userId, string imageName)
    {
        Image retrievedImage = await DatabaseService.FetchImageFromIdSecured(userId,imageName);
        
        if (retrievedImage == null)
        {
            return null;
        }

        return retrievedImage;
    }

    public static async Task<Image?> GetPreview (string userId, string imageName)
    {
        Image? retrievedImage = await Get(userId,imageName);
        
        if (retrievedImage == null)
        {
            return null;
        }

        // Manipulate the image path to find the lowres version of it if it exists.

        string fullImagePath = retrievedImage.Path ?? "";
        string lowresImageName = "lowres-" + Path.GetFileName(fullImagePath) + ".webp" ?? "";
        string imageDirectory = Path.GetDirectoryName(fullImagePath) ?? "";
        string lowresImagePath = imageDirectory + "/" + lowresImageName;

        if (await FileService.ContainsImageInPath(lowresImageName, imageDirectory)) 
        {
            retrievedImage.Path = lowresImagePath;
        }

        return retrievedImage;
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

/*
    public static async Task Delete(int id)
    {
        if (Get(id) != null)
        {
            NpgsqlCommand command = DatabaseService.CreateQuery("DELETE FROM images WHERE id = @id");
            command.Parameters.AddWithValue("id", id);
            await command.ExecuteReaderAsync();
        }
    }
*/
    public static void Update(Image image)
    {
    }

}