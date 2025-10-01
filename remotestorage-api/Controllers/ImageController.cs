using remotestorage_api.Models;
using remotestorage_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    public ImageController()
    {
    }

    // GET all action
    [HttpGet]
    public ActionResult<List<Image>> GetAll() => ImageService.GetAll(); //Replaces the default action by our custom-build one.

    // GET by Id action

    [HttpGet("{id}")]//ByField
    public ActionResult<Image> Get(int id)
    {
        var image = ImageService.Get(id);
        if (image == null)
        {
            return NotFound(); // API specific syntax to return back an unsucessfull call!
        }
        return image;
    }

    [HttpGet("by-query")]
    public ActionResult<Image> GetByQuery([FromQuery] int id) //Fromquery filters the value haha!
    {
        var image = ImageService.Get(id);
        if (image == null)
        {
            return NotFound(); // API specific syntax to return back an unsucessfull call!
        }
        return image;
    }

    // POST action

    [HttpPost]
    public IActionResult Create(Image Image)
    {
        ImageService.Add(Image);
        return CreatedAtAction(nameof(Get), new { id = Image.Id }, Image);
    }

    // PUT action



    // DELETE action
    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var image = ImageService.Get(id);
        if (image != null)
        {
            ImageService.Delete(id);
            return NoContent();
        }
        else
        {
            return NotFound();
        }
    }


}