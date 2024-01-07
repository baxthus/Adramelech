using Adramelech.Api.Services;
using Adramelech.Server.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Adramelech.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class FilesController(TcpService tcpService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        Console.WriteLine("Listing files");
        var files = await tcpService.SendCommandAsync(new Request("files-list", null));
        return files.IsFailure ? StatusCode(500, files.Exception?.Message ?? "Unknown error") : Ok(files.Value);
    }
}