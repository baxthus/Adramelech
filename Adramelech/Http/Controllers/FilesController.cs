using Adramelech.Http.Attributes;
using Adramelech.Http.Server;
using Adramelech.Http.Utilities;

namespace Adramelech.Http.Controllers;

[ApiController]
[Route("/files")]
public class FilesController : ControllerBase
{
    [HttpGet]
    public async Task GetFiles()
    {
        await Context.RespondAsync("Hello, world!");
    }
}