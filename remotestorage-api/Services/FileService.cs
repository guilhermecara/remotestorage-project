using Microsoft.AspNetCore.Http.HttpResults;
using remotestorage_api.Models;
using System;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Mvc;

namespace remotestorage_api.Services;

public static class FileService
{

    static string imageDir = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? Environment.GetEnvironmentVariable("FALLBACK_IMAGE_DIR");

    public static async Task<IActionResult> StreamImage(string imagePath)
    {
        var safePath = Path.GetFullPath(Path.Combine(imageDir, imagePath));

        if (!safePath.StartsWith(imageDir))
            return new BadRequestObjectResult("Invalid path.");

        var ext = Path.GetExtension(safePath).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            return new BadRequestObjectResult("File type not allowed.");

        if (System.IO.File.Exists(safePath))
        {
            string mimeType = GetMimeType(Path.GetExtension(imagePath));
            var stream = new FileStream(safePath, FileMode.Open, FileAccess.Read);
            return new FileStreamResult(stream, mimeType);
        }
        else
        {
            Console.WriteLine($"Warning: Image file not found at path {safePath}");
            return new NotFoundResult();
        }
    }
    
    public static async Task<Image> UploadImage (IFormFile file)
    {
        string fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(imageDir, fileName);
        
        using (FileStream stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
    
        return new Image
        {
            Name = fileName,
            Url = fileName // Store only the relative path, not the full URL, Will add more functionality once I add a system for users.
        };
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
}