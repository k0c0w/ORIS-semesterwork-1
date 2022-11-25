using System.Text.RegularExpressions;
using Server.Models;
using Server.Services;
using Server.Services.ServerServices;
namespace Server.Controllers;

[ApiController("profile")]
public class ProfileController
{
    private readonly ORM _orm = new ORM();
    private readonly SessionManager _sessionManager = SessionManager.Instance;
    
    
    [HttpGet("edit")]
    [AuthorizeRequired]
    public IActionResult GetProfileInfo([UserRequired] User user)
    {
        var info = _orm.Select(new WhereModel<PersonalInfo>(new PersonalInfo { UserId = user.Id }))
            .FirstOrDefault();
        
        return new TemplateView("ProfileEdit", 
            new { User = user, PersonalInfo = info });
    }
    
    
    [HttpPost("edit")]
    [AuthorizeRequired]
    public IActionResult EditUserPersonalInfo([FromQuery] string firstName,
        [FromQuery] string middleName, [FromQuery] string lastName, [FromQuery] string telephone, 
        [FromQuery] string license, [FromQuery] string passport, [FromQuery] string card,
        [FromQuery] string cardOwner, [FromQuery] string cvc, [UserRequired] User user)
    {
        var badFields = new List<InputError>(9);
        var personalInfo = _orm.Select(new WhereModel<PersonalInfo>(new PersonalInfo() { UserId = user.Id }))
            .FirstOrDefault();
        
        if(!string.IsNullOrEmpty(telephone))
        {
            telephone = telephone.Replace("-", "").Replace("(", "")
                .Replace(")", "").Replace("+", "");
            telephone = telephone.Length > 0 ? telephone.Substring(1) : telephone;
            if(!FormFieldValidator.IsTelephoneNumberValid(telephone))
                badFields.Add(new InputError(nameof(telephone), "Формат телефона: +7**********"));
        }
        if(!string.IsNullOrEmpty(passport) && !FormFieldValidator.IsPassportValid(passport))
            badFields.Add(new InputError(nameof(passport), "Не валидные данные."));
        if(!string.IsNullOrEmpty(license) && !FormFieldValidator.IsDriverLicenseValid(license))
            badFields.Add(new InputError(nameof(license), "Не валидные данные."));
        if(!string.IsNullOrEmpty(card) && !(card.Length == 16 && Regex.IsMatch(card, @"^[0-9]+$")))
            badFields.Add(new InputError(nameof(card), "Поддерживаются только карты с 16-ти значным номером."));
        if(!string.IsNullOrEmpty(cvc) && !(Regex.IsMatch(cvc, @"^[0-9][0-9][0-9]$") && cvc != "000"))
            badFields.Add(new InputError(nameof(cvc), "Неверный CVV/CVC код"));
        
        if(badFields.Any())
            return ActionResultFactory.Json(new OperationResultDto() {Errors = badFields.ToArray()});

        var updatedRows = _orm.Update(new PersonalInfo()
        {
            FirstName = string.IsNullOrEmpty(firstName) ? personalInfo.FirstName : firstName,
            MiddleName = string.IsNullOrEmpty(middleName) ? personalInfo.MiddleName : middleName,
            LastName = string.IsNullOrEmpty(lastName) ? personalInfo.LastName : lastName,
            Passport = string.IsNullOrEmpty(passport) ? personalInfo.Passport : ulong.Parse(passport),
            TelephoneNumber = string.IsNullOrEmpty(telephone) ? personalInfo.TelephoneNumber : uint.Parse(telephone),
            DriverLicense = string.IsNullOrEmpty(license) ? personalInfo.DriverLicense : int.Parse(license),
            CardNumber = string.IsNullOrEmpty(card) ? personalInfo.CardNumber : ulong.Parse(card),
            CardOwner = string.IsNullOrEmpty(cardOwner) ? personalInfo.CardOwner : cardOwner,
            CVC = string.IsNullOrEmpty(cvc) ? personalInfo.CVC : int.Parse(cvc),
        }, new WhereModel<PersonalInfo>(personalInfo));
        
        return ActionResultFactory.Json(new OperationResultDto() {Success = updatedRows > 0});
    }
    
    [HttpPost("edit")]
    [AuthorizeRequired]
    public IActionResult EditUserInfo([FromQuery] string firstName,
        [FromQuery] string email, [FromQuery] string birthDate, [UserRequired] User user)
    {
        var badFields = new List<InputError>(3);

        if(string.IsNullOrEmpty(firstName))
            badFields.Add(new InputError(nameof(firstName), ErrorMessages.RequiredField));
        
        DateTime? date = DateTime.TryParse(birthDate, out var value) ? value : null;
        if (!date.HasValue)
            badFields.Add(new InputError(nameof(birthDate), ErrorMessages.IncorrectDateFormat));
        else if (!FormFieldValidator.IsCorrectAge(date.Value))
            badFields.Add(new InputError(nameof(birthDate), ErrorMessages.IncorrectAge));
        
        if (!FormFieldValidator.IsEmailValid(email))
            badFields.Add(new InputError(nameof(email), ErrorMessages.IncorrectEmailFormat));
        else if(_orm.Select(new WhereModel<User>(new User() { Email = email })).Skip(1).Any())
            badFields.Add(new InputError(nameof(email), ErrorMessages.EmailAlreadyUsed));
        
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

    [HttpPost("edit/password")]
    [AuthorizeRequired]
    public IActionResult ChangeUserPassword([FromQuery] string password, [UserRequired] User user, 
        [SessionRequired] Session session)
    {
        var userName = user?.FirstName;
        var errors = new List<string>(2);
        if(!FormFieldValidator.IsPasswordValid(password))
            errors.Add(ErrorMessages.PasswordShould);
        
        if(password == user.Password)
            errors.Add("Новый пароль совпадает с предыдущим!");
        if (!errors.Any())
        {
            var updated = new User { Password = password };
            _orm.Update(updated, new WhereModel<User>(user));
            var newSession = new Session() { Id = Guid.NewGuid(), AccountId = (int)user.Id, CreateDateTime = DateTime.Now };
            _sessionManager.TerminateSession(session);
            _sessionManager.CreateQuickSession(newSession.Id, () => newSession);

            return ReturnTemplate(userName, Array.Empty<string>(), true);
        }

        return ReturnTemplate(userName, errors.ToArray(), false);
    }
    
    [HttpGet("edit/password")]
    [AuthorizeRequired]
    public IActionResult ChangeUserPasswordPage([UserRequired] User user)
    {
        return ReturnTemplate(user?.FirstName,Array.Empty<string>(), false);
    }
    
    [HttpGet("getUserName")]
    [AuthorizeRequired]
    public IActionResult GetUserNameJson([UserRequired] User user) 
        => ActionResultFactory.Json(new {name = user.FirstName});
    

    private IActionResult ReturnTemplate(string name, string[] errors, bool isChanged)
    {
        return new TemplateView("PasswordChangePage", 
            new
            {
                name = name,
                errors = errors,
                success = isChanged,
            });
    }
}
