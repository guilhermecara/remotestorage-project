using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using remotestorage_api.Models;
using remotestorage_api.Services;

namespace remotestorage_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{   
    [HttpGet("testauth")]
    [Authorize]
    public async Task<IActionResult> ProtectedEndpoint ()
    {
        return Ok(new {message = "Logged in"});
    }
}