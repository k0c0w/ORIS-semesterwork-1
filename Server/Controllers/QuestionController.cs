using System.Net;
using Server.Models;
using Server.Services;
using Server.Services.ServerServices;

namespace Server.Controllers;

[ApiController("help")]
public class QuestionController
{
    [HttpGet("question")]
    public IActionResult RedirectToMain() => new Redirect("/");
    
    [HttpPost("question")]
    public IActionResult CreateUserQuestion([FromQuery] string name, [FromQuery] string email, 
        [FromQuery] string phone, [FromQuery] string textarea, [FromQuery] string accept, [UserRequired] User? user)
    {
        var allChecksPassed = true;
        if (!FormFieldValidator.IsEmailValid(email))
            allChecksPassed = false;
        if(phone.Length < 2 || !FormFieldValidator.IsTelephoneNumberValid(phone.Substring(2)))
            allChecksPassed = false;
        if (accept != "on")
            allChecksPassed = false;
        if (string.IsNullOrEmpty(textarea) || string.IsNullOrEmpty(name))
            allChecksPassed = false;
        
        if(!allChecksPassed)
            return new BadRequest();

        var question = new CustomerQuestion
        {
            UserId = user?.Id, Question = textarea, ResponseEmail = email,
            Telephone = phone.Substring(2), Name = name, StatusCode = (int)QuestionStatus.Opened
        };
        var affected = new ORM().Insert(question);

        return affected != 0 ? new HtmlResult("Спасибо за обращение! Мы с Вами свяжемся.") : new BadRequest();
    }
}