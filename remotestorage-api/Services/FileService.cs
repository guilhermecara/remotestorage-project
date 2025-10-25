using remotestorage_api.Models;
using Microsoft.AspNetCore.Mvc;

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

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

    public static async Task<Models.Image> UploadImage(IFormFile file)
    {
        // Validate the file
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            throw new ArgumentException("File type not allowed.");

        // Ensure directory exists
        if (!System.IO.Directory.Exists(imageDir))
            System.IO.Directory.CreateDirectory(imageDir);

        // Create a unique file name
        string fileName = Path.GetRandomFileName() + Path.GetExtension(file.FileName);
        string filePath = Path.Combine(imageDir, fileName);

        // Save file to disk
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Default to current time
        //DateTimeOffset postedDate = DateTimeOffset.UtcNow;
        
        // Read metadata using MetadataExtractor
        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

         // Try to extract EXIF DateTimeOriginal
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

        Console.WriteLine("subIfdDirectory is : ");
        Console.WriteLine(subIfdDirectory);

        Console.WriteLine("Date time of the image is : ");
        Console.WriteLine(dateTime);

        // Return your image model
        return new Models.Image
        {
            Name = fileName,
            Url = fileName, // relative path or to be prefixed later
            
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