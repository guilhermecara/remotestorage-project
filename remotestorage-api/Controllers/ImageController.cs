using remotestorage_api.Models;
using remotestorage_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
    public async Task<IActionResult> GetImages()
    {
        var images = await ImageService.GetAll();
        return Ok(images);
    }

    // GET by Id action

    [HttpGet("{id}")]//ByField
    public async Task<ActionResult<Image>> Get(int id)
    {
        var image = await ImageService.Get(id);
        if (image == null)
        {
            return NotFound(); // API specific syntax to return back an unsucessfull call!
        }
        return Ok(image);
    }

    //[HttpGet("by-query")]
    ////public ActionResult<Image> GetByQuery([FromQuery] int id) //Fromquery filters the value haha!
    //{
    //    var image = ImageService.Get(id);
    //    if (image == null)
    //    {
    //        return NotFound(); // API specific syntax to return back an unsucessfull call!
    //    }
    //    return Ok(Image);
    //}

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