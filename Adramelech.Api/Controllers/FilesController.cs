using Microsoft.AspNetCore.Mvc;

namespace Adramelech.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class FilesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return Ok();
    }
}