using remotestorage_api.Models;
using remotestorage_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetImages()
    {
        var user = User;
        User.FindFirst("name");
        var request = Request;
        var jwt = Request.Cookies["auth_token"]; 
        var images = await ImageService.GetAll();
        return Ok(images);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Image>> Get(int id)
    {
        var image = await ImageService.Get(id);
        if (image == null)
        {
            return NotFound();
        }
        return Ok(image);
    }

    const int MAX_FILE_SIZE = 10 * 1024 * 1024; // 10 MB

    [HttpPost("upload")]
    [Authorize]
    [RequestSizeLimit(MAX_FILE_SIZE)]
    public async Task<IActionResult> Upload([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Only image files are allowed.");

        if (User.FindFirst("user_id") == null)
            return BadRequest("Invalid user");
        else if (User.FindFirst("user_id").Value == null)
            return BadRequest("Invalid user");

        try
        {
            var newImage = await ImageService.Add(file, User.FindFirst("user_id").Value);

            if (newImage == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to save image.");

            return Ok(new
            {
                message = "Image uploaded successfully",
                image = newImage
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"Error uploading image: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var image = ImageService.Get(id);
        if (image != null)
        {
            await ImageService.Delete(id);
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }
}