using System.Net;
using System.Text.RegularExpressions;
using Server.Models;
using Server.Services;
using Server.Services.ServerServices;
namespace Server.Controllers;

[ApiController("profile")]
public class MyController
{
    private readonly ORM _orm = new ORM();
    private readonly SessionManager _sessionManager = SessionManager.Instance;
    
    
    [HttpGet("edit")]
    [AuthorizeRequired]
    public async Task<IActionResult> GetProfileInfo([SessionRequired] Session userSession)
    {
        var user = GetUserBySession(userSession);
        var info = _orm.Select(new WhereModel<PersonalInfo>(new PersonalInfo { UserId = userSession.AccountId }))
            .FirstOrDefault();
        
        return new TemplateView("ProfileEdit", 
            new { User = user, PersonalInfo = info });
    }
    
    
    [HttpPost("edit")]
    [AuthorizeRequired]
    public async Task<IActionResult> EditUserPersonalInfo([FromQuery] string firstName,
        [FromQuery] string middleName, [FromQuery] string lastName, [FromQuery] string telephone, 
        [FromQuery] string driverLicense, [FromQuery] string passport, [FromQuery] string card,
        [FromQuery] string cardOwner, [FromQuery] string cvc, [SessionRequired] Session userSession)
    {
        var badFields = new List<InputError>(9);
        var personalInfo = _orm.Select(new WhereModel<PersonalInfo>(new PersonalInfo() { UserId = userSession.AccountId }))
            .FirstOrDefault();
        
        if(!string.IsNullOrEmpty(telephone))
        {
            telephone = telephone.Replace("-", "").Replace("(", "")
                .Replace(")", "").Replace("+", "");
            telephone = telephone.Length > 0 ? telephone.Substring(1) : telephone;
            if(!FormFieldValidator.IsTelephoneNumberValid(telephone))
                badFields.Add(new InputError(telephone, "Формат телефона: +7**********"));
        }
        if(!string.IsNullOrEmpty(passport) && !FormFieldValidator.IsPassportValid(passport))
            badFields.Add(new InputError(passport, "Не валидные данные."));
        if(!string.IsNullOrEmpty(driverLicense) && !FormFieldValidator.IsDriverLicenseValid(driverLicense))
            badFields.Add(new InputError(driverLicense, "Не валидные данные."));
        if(!string.IsNullOrEmpty(card) && !(card.Length == 16 && Regex.IsMatch(card, @"^[0-9]+$")))
            badFields.Add(new InputError(card, "Поддерживаются только карты с 16-ти значным номером."));
        if(!string.IsNullOrEmpty(cvc) && !(Regex.IsMatch(cvc, @"^[0-9][0-9][0-9]$") && cvc != "000"))
            badFields.Add(new InputError(card, "Неверный CVV/CVC код"));
        
        if(badFields.Any())
            return ActionResultFactory.Json(new OperationResultDto() {Errors = badFields.ToArray()});

        var updatedRows = _orm.Update(new PersonalInfo()
        {
            FirstName = string.IsNullOrEmpty(firstName) ? personalInfo.FirstName : firstName,
            MiddleName = string.IsNullOrEmpty(middleName) ? personalInfo.MiddleName : middleName,
            LastName = string.IsNullOrEmpty(lastName) ? personalInfo.LastName : lastName,
            Passport = string.IsNullOrEmpty(passport) ? personalInfo.Passport : ulong.Parse(passport),
            TelephoneNumber = string.IsNullOrEmpty(telephone) ? personalInfo.TelephoneNumber : uint.Parse(telephone),
            DriverLicense = string.IsNullOrEmpty(driverLicense) ? personalInfo.DriverLicense : int.Parse(driverLicense),
            CardNumber = string.IsNullOrEmpty(card) ? personalInfo.CardNumber : ulong.Parse(card),
            CardOwner = string.IsNullOrEmpty(cardOwner) ? personalInfo.CardOwner : cardOwner,
            CVC = string.IsNullOrEmpty(cvc) ? personalInfo.CVC : int.Parse(cvc),
        }, new WhereModel<PersonalInfo>(personalInfo));
        
        return ActionResultFactory.Json(new OperationResultDto() {Success = updatedRows > 0});
    }
    
    [HttpPost("edit")]
    [AuthorizeRequired]
    public async Task<IActionResult> EditUserInfo([FromQuery] string firstName,
        [FromQuery] string email, [FromQuery] string birthDate, [SessionRequired] Session userSession)
    {
        var badFields = new List<InputError>(3);
        var user = GetUserBySession(userSession);
        
        if(string.IsNullOrEmpty(firstName))
            badFields.Add(new InputError(firstName, "Поле с именем обязательно для заполнения!"));
        
        DateTime? date = DateTime.TryParse(birthDate, out var value) ? value : null;
        if (!date.HasValue)
            badFields.Add(new InputError(date, "Неверный формат даты!"));
        else if (!FormFieldValidator.IsCorrectAge(date.Value))
            badFields.Add(new InputError(date, "Вам должно быть не меньше 18 лет и не больше 90 лет."));
        
        email = email.Replace("%40", "@");
        if (!FormFieldValidator.IsEmailValid(email))
            badFields.Add(new InputError(email, "Неверный формат почты!"));
        else if(_orm.Select(new WhereModel<User>(new User() { Email = email })).Skip(1).Any())
            badFields.Add(new InputError(email, "Данная почта уже кем-то используется!"));
        
        if(badFields.Any())
            return ActionResultFactory.Json(new OperationResultDto() {Errors = badFields.ToArray()});

        var updatedRows = _orm.Update(new User()
        {
            FirstName = firstName,
            BirthDate = date,
            Email = email
        }, new WhereModel<User?>(user));
        
        return ActionResultFactory.Json(new OperationResultDto() {Success = updatedRows > 0});
    }

    [HttpPost("edit")]
    [AuthorizeRequired]
    public async Task<IActionResult> ChangeUserPassword([FromQuery] string newPassword, [SessionRequired] Session userSession)
    {
        if(!FormFieldValidator.IsPasswordValid(newPassword))
            return ActionResultFactory.Json(new OperationResultDto() {Errors = new[] 
                    {new InputError("", "Пароль должен состоять из цифр и латинских символов (от 5 до 50).")}});
        var user = GetUserBySession(userSession);
        if(newPassword == user.Password)
            return ActionResultFactory.Json(new OperationResultDto() 
                {Errors = new[] {new InputError("", "Новый пароль совпадает с предыдущим!") }});
        
        var updated = new User() { Password = newPassword };
        _orm.Update(updated, new WhereModel<User>(user));
        var newSession = new Session() { Id = Guid.NewGuid(), AccountId = (int)user.Id, CreateDateTime = DateTime.Now };
        _sessionManager.TerminateSession(userSession);
        _sessionManager.CreateSession(newSession.Id, () => newSession);
        
        return ActionResultFactory.Json(new OperationResultDto() {Success = true});
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string? email, [FromQuery] string? password, 
        [CookieRequired] CookieCollection cookies)
    {
        email = email.Replace("%40", "@");
        var sessionCookie = cookies["SessionId"]?.Value;
        var user = _orm.Select(new WhereModel<User>(new User() { Email = email, Password = password }))
            .FirstOrDefault();
        if (user != null)
        {
            var guid = Guid.Empty;
            var correctCookie = string.IsNullOrEmpty(sessionCookie) || Guid.TryParse(sessionCookie, out guid);
            if (correctCookie)
            {
                if (!_sessionManager.TryGetSession(guid, out var session) || session!.AccountId != user.Id)
                {
                    guid = Guid.NewGuid();
                    _sessionManager.CreateSession(guid,
                        () => new Session() { Id = guid, AccountId = (int)user.Id, CreateDateTime = DateTime.Now });
                    return ActionResultFactory.SendHtml("true", 
                        new SessionInfo() { Guid = guid });
                }
                
                return ActionResultFactory.SendHtml("true", 
                        new SessionInfo() { Guid = session.Id });
            }
        }

        return ActionResultFactory.Unauthorized();
    }
    
    private User? GetUserBySession(Session userSession)
        => _orm.Select(new WhereModel<User>(new User{ Id = userSession.AccountId })).FirstOrDefault();
}
