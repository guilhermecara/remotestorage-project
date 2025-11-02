using remotestorage_api.Models;
using Microsoft.AspNetCore.Mvc;

using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace remotestorage_api.Services;

public static class FileService
{

    static string imageDir = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? Environment.GetEnvironmentVariable("FALLBACK_IMAGE_DIR");

    public static async Task<MemoryStream> ProcessPerformanceImage(string imagePath)
    {
        const int MAX_WIDTH = 1920;
        const int IMAGE_QUALITY = 60;

        SixLabors.ImageSharp.Image image = await SixLabors.ImageSharp.Image.LoadAsync(imagePath);
        if (image.Width > MAX_WIDTH)
        {
            var ratio = (double)MAX_WIDTH / image.Width;
            var newHeight = (int)(image.Height * ratio);
            image.Mutate(x => x.Resize(MAX_WIDTH, newHeight));
        }

        MemoryStream imageStream = new MemoryStream();

        await image.SaveAsync(imageStream, new WebpEncoder
        {
            Quality = IMAGE_QUALITY
        });

        imageStream.Position = 0;

        return imageStream;
    }
    
    public static async Task<FileStream> ProcessQualityImage (string imagePath)
    {
        var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
        return stream;
    }

    public static async Task<IActionResult> StreamImage(string imagePath, string mode)
    {
        var safePath = Path.GetFullPath(Path.Combine(imageDir, imagePath));

        if (!safePath.StartsWith(imageDir))
        {
            return new BadRequestObjectResult("Invalid path.");
        }

            var ext = Path.GetExtension(safePath).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
        {
            return new BadRequestObjectResult("File type not allowed.");
        }

        if (!System.IO.File.Exists(safePath))
        {
            Console.WriteLine($"Warning: Image file not found at path {safePath}");
            return new NotFoundResult();
        }

        if (mode == "lossless")
        {
            var stream = await ProcessQualityImage(safePath);
            var mimeType = GetMimeType(Path.GetExtension(safePath));
            return new FileStreamResult(stream, mimeType);
        }
        else
        {
            MemoryStream stream = await ProcessPerformanceImage(safePath);
            return new FileStreamResult(stream, "image/webp");
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