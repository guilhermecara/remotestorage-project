using remotestorage_api.Models;
using remotestorage_api.Services;
using Microsoft.AspNetCore.Mvc;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    [HttpGet("{**path}")]
    public async Task<IActionResult> ReadImage(string path,[FromQuery] string? mode = "performance")
    {
        Console.WriteLine($"Tried accessing the image at path: {path}");
        return await FileService.StreamImage(path,mode);
    }
}