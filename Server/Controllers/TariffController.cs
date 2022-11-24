using Server.Models;
using Server.Services;
using Server.Services.ServerServices;

namespace Server.Controllers;

[ApiController("tariff")]
public class TariffController
{
    private readonly ORM _orm = new ORM();

    [HttpGet("everyday")]
    public IActionResult GetEveryDayPage() => GetView("Everyday");

    [HttpGet("travel")]
    public IActionResult GetTravelPage() => GetView("Travel");
    
    [HttpGet("whatever")]
    public IActionResult GetWhateverYouWantPage() => GetView("Whatever You Want");

    private IActionResult GetView(string tariff)
    {
        var tariffs = _orm.Select(new WhereModel<Tariff>(new Tariff { Name = tariff }));
        
        var carsCondition = tariffs.Select(x => 
            new WhereModel<CarModel>(new CarModel {Id = (int)x.Car}));
        
        var cars = _orm.Select(carsCondition);
        var withPrice = cars.Join(tariffs, c => (int)c.Id, t => (int)t.Car, 
            (car, tariff) => new { CarInfo = car, Price = tariff.Price});
        
        var description = tariffs.FirstOrDefault()?.Description;
        return new TemplateView("Tariff", new { name = tariff, description = description, cars = withPrice});
    } 
}