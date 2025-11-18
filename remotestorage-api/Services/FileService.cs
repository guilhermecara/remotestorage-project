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

    static string applicationRootDirectory = Environment.GetEnvironmentVariable("IMAGE_DIR") ?? Environment.GetEnvironmentVariable("FALLBACK_IMAGE_DIR");

    private static async Task<MemoryStream> CachePerformanceImage(IFormFile file)
    {
        const int IMAGE_QUALITY = 60;

        using var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream());
        var imageStream = new MemoryStream();

        await image.SaveAsync(imageStream, new WebpEncoder
        {
            Quality = IMAGE_QUALITY
        });

        imageStream.Position = 0;

        return imageStream;
    }

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

    public static async Task<IActionResult> StreamImage(string imagePath)
    {
        var safePath = Path.GetFullPath(Path.Combine(applicationRootDirectory, imagePath));

        if (!safePath.StartsWith(applicationRootDirectory))
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
            return new NotFoundResult();
        }

        var stream = new FileStream(safePath, FileMode.Open, FileAccess.Read);
        var mimeType = GetMimeType(Path.GetExtension(safePath));
        return new FileStreamResult(stream, mimeType);
    }

    public static async Task<Models.Image> UploadImage(IFormFile file, string ownerGuid)
    {
        // Validate the file
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file uploaded.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            throw new ArgumentException("File type not allowed.");

        // Ensure directory exists
        if (!System.IO.Directory.Exists(applicationRootDirectory))
            System.IO.Directory.CreateDirectory(applicationRootDirectory);

        // Create a unique file name
        
        string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "-" + GUIDService.GenerateGuidString() + Path.GetExtension(file.FileName);

        // Define a safe path for the file

        string userDirectory = Path.Combine(applicationRootDirectory,ownerGuid);
        string filePath = Path.Combine(userDirectory, fileName);
        
        // Save original file to disk

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Save low-res version of image to disk. PROCESSING INTENSIVE, USE RABBITMQ

        string lowresFilePath = Path.Combine(userDirectory, "lowres-"+fileName);
        MemoryStream lowResImageStream = await CachePerformanceImage(file);
        using (var outputStream = new FileStream(lowresFilePath + ".webp", FileMode.Create))
        {
            await lowResImageStream.CopyToAsync(outputStream);
        }

        // Read metadata using MetadataExtractor
        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(filePath);

        // Try to extract EXIF DateTimeOriginal
        var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);

        // Return your image model
        return new Models.Image
        {
            Name = fileName,
            Path = Path.Combine(ownerGuid,fileName)
        };
    }

    public static Task<bool> ContainsImageInPath(string image, string path)
    {
        if (string.IsNullOrWhiteSpace(image) || string.IsNullOrWhiteSpace(path))
            return Task.FromResult(false);

        // Combine root + user path safely
        string root = Path.GetFullPath(applicationRootDirectory);
        string userPath = Path.GetFullPath(Path.Combine(root, path));

        // Prevent directory traversal attempts
        if (!userPath.StartsWith(root))
            return Task.FromResult(false);

        // Full image path
        string imagePath = Path.GetFullPath(Path.Combine(userPath, image));

        // Extra security: ensure the image stays inside the target user directory
        if (!imagePath.StartsWith(userPath))
            return Task.FromResult(false);

        bool exists = File.Exists(imagePath);
        return Task.FromResult(exists);
    }



    public static async void CreateDirectory (string path) // Assuming the root is the physical folder
    {
        string newDirectoryPath = Path.Combine(applicationRootDirectory, path);
        DirectoryInfo directoryInfo = System.IO.Directory.CreateDirectory(newDirectoryPath);
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