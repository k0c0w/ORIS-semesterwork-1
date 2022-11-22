using System.Net;
using Server.Services.ServerServices;
namespace Server.Controllers;
[ApiController]
public class MyController
{
    [HttpGet]
    public async Task<IActionResult> Do()
    {
        return new TemplateView("MyView", new { Name = "World" }, new Cookie[0]);
    }
}
