using Server.Models;
using Server.Services;
using Server.Services.ServerServices;

namespace Server.Controllers;

[ApiController("/")]
public class AvailableCitiesController
{
    [HttpGet("city")]
    public IActionResult AvailableCities()
    {
        var cities = new ORM().Select<AvailableCity>().Select(x => x.City).Distinct().ToArray();
        return new TemplateView("Cities", new {cities = cities});
    }
}