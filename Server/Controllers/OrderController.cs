using Server.Models;
using Server.Services;
using Server.Services.ServerServices;

namespace Server.Controllers;

[ApiController("order")]
public class OrderControllers
{
    private readonly ORM _orm = new ORM();
    
    [HttpPost("car")]
    [AuthorizeRequired]
    public IActionResult OrderCar([FromQuery] string tariff, [FromQuery] int car, [FromQuery] DateTime start, 
        [FromQuery] DateTime end, [FromQuery] string city, [UserRequired] User user)
    {
        city = city.ToLower();
        var cities = _orm.Select<AvailableCity>();
        if (!cities.Select(x => x.City.ToLower()).Contains(city))
            return Json(new OperationResultDto{ Errors = new[] {new InputError("city", "Город не доступен.")}});

        var tariffs = _orm.Select<Tariff>();
        if(tariffs.All(x => x.Name.ToLower() != tariff.ToLower()))
            return Json(new OperationResultDto { Errors = new []{new InputError("tariff", "Тариф не доступен!")}});
        var tariffId = tariffs.FirstOrDefault(x => x.Name.ToLower() == tariff.ToLower()).Id;
        
        var cityId = cities.FirstOrDefault(x => x.City.ToLower() == city).Id;
        var freeCar = (bool)_orm.CallFunction("IsCarAvailableInCity", car, cityId);
        if(!freeCar)
            return Json(new OperationResultDto{ Errors = new[] {new InputError("car", "Нет свободных машин.")}});
        
        
        var actualCar = (int)_orm.CallFunction("GetFreeCar", car, cityId, start);
        var badArgs = new List<InputError>(2);
        if(actualCar == -1)
            badArgs.Add(new InputError("car", "Подходящей машины не нашлось."));
        if(start > end)
            badArgs.Add(new InputError("date", "Некорректные даты."));

        if (badArgs.Any())
            return Json(new OperationResultDto { Errors = badArgs.ToArray() });
        
        var order = new Order { SubscriptionStart = start, SubscriptionEnd = end, UserId = user.Id,
            CarId = actualCar, CityId = cityId, TariffId = tariffId, IsCancled = false};

        return TryCreateOrder(order);
    }

    private IActionResult Json<T>(T model) => new Json<T>(model);

    private IActionResult TryCreateOrder(Order order)
    {
        var insertedRows = _orm.Insert(order);
        if (insertedRows == 0)
            return Json(new OperationResultDto { Errors = new[] { new InputError("serverError", "Ошибка сервера") } });
        var carSign = _orm.Select(new WhereModel<ActualCar>(new ActualCar { Car = order.CarId })).FirstOrDefault().RegisterSign;

        return Json(new { Success = true, Sign = carSign });
    }
}